using UnityEngine;

public class ExploreMapSpawner : MonoBehaviour
{
    [SerializeField] private ExplorePlayerController playerController;

    [SerializeField] private RoomController roomPrefab;
    [SerializeField] private Transform roomRoot;

    [SerializeField] private ExploreNodeSpawnCount[] nodeCounts;
    [SerializeField] private float roomSpacing = 12f;

    private RoomController currentRoomController;

    private ExploreMapData currentMap;



    public void ApplyStartRoom(ExploreMapData mapData)
    {
        currentMap = mapData;

        if (currentMap == null)
        {
            Debug.LogWarning("ExploreMapSpawner: MapData is null.");
            return;
        }

        RunSession runSession = GameFlowController.Instance?.CurrentRunSession;
        string roomId = runSession != null && !string.IsNullOrEmpty(runSession.CurrentExploreRoomId)
            ? runSession.CurrentExploreRoomId
            : currentMap.StartRoomId;

        ApplyRoom(roomId);
    }

    public void ApplyRoom(string roomId)
    {
        ApplyRoom(roomId, null);
    }

    public void ApplyRoom(string roomId, DoorDirectionType? exitDirection)
    {
        if (currentMap == null)
        {
            Debug.LogWarning("ExploreMapSpawner: Current map is null.");
            return;
        }

        RoomData roomData = currentMap.GetRoom(roomId);

        if (roomData == null)
        {
            Debug.LogWarning($"ExploreMapSpawner: Room not found: {roomId}");
            return;
        }

        RunSession runSession = GameFlowController.Instance?.CurrentRunSession;

        if (runSession != null)
        {
            runSession.SetCurrentExploreRoom(roomData.RoomId);
        }

        SpawnRoom(roomData);

        if (currentRoomController == null)
        {
            return;
        }

        if (playerController != null)
        {
            if (exitDirection.HasValue)
            {
                Transform entryPoint = currentRoomController.GetEntryPointFromDirection(exitDirection.Value);
                playerController.TeleportTo(entryPoint);
            }
            else if (runSession != null && runSession.HasExploreReturnPosition)
            {
                playerController.TeleportTo(runSession.ExploreReturnPosition);
                runSession.ClearExploreReturnPosition();
            }
        }

        Debug.Log($"Applied room: {roomData.RoomId}, Node: {roomData.NodeType}");
    }

    private void SpawnRoom(RoomData roomData)
    {
        if (currentRoomController != null)
        {
            Destroy(currentRoomController.gameObject);
            currentRoomController = null;
        }

        if (roomPrefab == null)
        {
            Debug.LogWarning("ExploreMapSpawner: Room prefab is not assigned.");
            return;
        }

        Transform spawnParent = roomRoot != null ? roomRoot : transform;
        Vector3 spawnPosition = new Vector3(
            roomData.GridPosition.x * roomSpacing,
            0f,
            roomData.GridPosition.y * roomSpacing);

        currentRoomController = Instantiate(roomPrefab, spawnPosition, Quaternion.identity, spawnParent);
        currentRoomController.InitializeDoorTransitions(this);
        currentRoomController.Initialize(roomData);
    }


}
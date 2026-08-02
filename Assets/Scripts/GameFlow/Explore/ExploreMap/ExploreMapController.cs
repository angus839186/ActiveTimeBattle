using UnityEngine;
using System.Collections.Generic;

public class ExploreMapController : MonoBehaviour
{
    [SerializeField] private ExplorePlayerController playerController;

    [SerializeField] private RoomController roomPrefab;
    [SerializeField] private Transform roomRoot;

    [SerializeField] private ExploreNodeSpawnCount[] nodeCounts;
    [SerializeField] private float roomSpacing = 12f;
    [SerializeField] private RoomCameraDirector cameraDirector;

    private readonly Dictionary<string, RoomController> spawnedRooms = new Dictionary<string, RoomController>();
    private RoomController currentRoomController;
    private ExploreMapData currentMap;
    public void BuildAndApplyMap(RunSession runSession)
    {
        if (runSession == null)
        {
            Debug.LogWarning("ExploreMapSpawner: RunSession is null.");
            return;
        }

        if (runSession.ExploreMap == null)
        {
            ExploreMapData map = ExploreMapFactory.CreateMap(runSession.Seed, nodeCounts);
            runSession.SetExploreMap(map);
        }

        currentMap = runSession.ExploreMap;
        SpawnAllRooms(currentMap);

        string roomId = !string.IsNullOrEmpty(runSession.CurrentExploreRoomId)
            ? runSession.CurrentExploreRoomId
            : currentMap.StartRoomId;

        ApplyRoom(roomId);
    }
    private void SpawnAllRooms(ExploreMapData mapData)
    {
        spawnedRooms.Clear();

        foreach (RoomData roomData in mapData.Rooms)
        {
            Transform spawnParent = roomRoot != null ? roomRoot : transform;
            Vector3 spawnPosition = new Vector3(
                roomData.GridPosition.x * roomSpacing,
                0f,
                roomData.GridPosition.y * roomSpacing);

            RoomController room = Instantiate(roomPrefab, spawnPosition, Quaternion.identity, spawnParent);
            room.InitializeDoors(this);
            room.Initialize(roomData);

            spawnedRooms.Add(roomData.RoomId, room);
        }
    }

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

        if (!spawnedRooms.TryGetValue(roomData.RoomId, out currentRoomController))
        {
            Debug.LogWarning($"ExploreMapSpawner: Spawned room not found: {roomData.RoomId}");
            return;
        }

        if (currentRoomController == null) return;

        if (cameraDirector != null)
        {
            cameraDirector.MoveTo(currentRoomController.CameraPoint);
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
            else
            {
                playerController.TeleportTo(currentRoomController.PlayerSpawnPoint);
            }
        }

        Debug.Log($"Applied room: {roomData.RoomId}, Node: {roomData.NodeType}");
    }
    public void OpenConnectedDoors(RoomController sourceRoom)
    {
        if (sourceRoom == null || currentMap == null)
        {
            return;
        }

        RoomData sourceRoomData = currentMap.GetRoom(sourceRoom.RoomId);

        if (sourceRoomData == null)
        {
            return;
        }

        foreach (RoomDoorConnection connection in sourceRoomData.DoorConnections)
        {
            if (!spawnedRooms.TryGetValue(connection.ConnectedRoomId, out RoomController connectedRoom))
            {
                continue;
            }

            DoorDirectionType oppositeDirection = GetOppositeDirection(connection.Direction);
            connectedRoom.OpenDoor(oppositeDirection);
        }
    }

    private DoorDirectionType GetOppositeDirection(DoorDirectionType direction)
    {
        switch (direction)
        {
            case DoorDirectionType.Top:
                return DoorDirectionType.Down;
            case DoorDirectionType.Down:
                return DoorDirectionType.Top;
            case DoorDirectionType.Left:
                return DoorDirectionType.Right;
            case DoorDirectionType.Right:
                return DoorDirectionType.Left;
            default:
                return DoorDirectionType.Down;
        }
    }

}
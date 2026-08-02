using UnityEngine;

public class RoomController : MonoBehaviour
{
    [SerializeField] private string roomId;
    [SerializeField] private RoomDoor[] doors;

    [SerializeField] private Transform topEntryPoint;
    [SerializeField] private Transform downEntryPoint;
    [SerializeField] private Transform leftEntryPoint;
    [SerializeField] private Transform rightEntryPoint;

    [SerializeField] private Transform nodeSpawnPoint;
    [SerializeField] private Transform nodeRoot;
    [SerializeField] private ExploreNode nodePrefab;

    [SerializeField] private RoomCameraPoint cameraPoint;


    [SerializeField] private Transform playerSpawnPoint;

    private ExploreMapController mapController;

    private ExploreNode currentNode;
    public bool IsCompleted { get; private set; }
    public Transform PlayerSpawnPoint => playerSpawnPoint;

    public RoomCameraPoint CameraPoint => cameraPoint;

    public string RoomId => roomId;

    private void Start()
    {
        RestoreRoomState();
    }


    public void Initialize(string roomId)
    {
        this.roomId = roomId;
        RestoreRoomState();
    }
    public void Initialize(RoomData roomData)
    {
        roomId = roomData.RoomId;

        ApplyDoorAvailability(roomData);
        RestoreRoomState();
        SpawnNode(roomData);
    }
    private void ApplyDoorAvailability(RoomData roomData)
    {
        foreach (RoomDoor door in doors)
        {
            string connectedRoomId = roomData.GetConnectedRoomId(door.Direction);
            bool hasConnectedRoom = !string.IsNullOrEmpty(connectedRoomId);

            door.SetConnectedRoom(connectedRoomId);
            door.SetAvailable(hasConnectedRoom);
        }
    }

    private void RestoreRoomState()
    {
        RunSession runSession = GameFlowController.Instance?.CurrentRunSession;

        if (runSession != null && runSession.IsRoomCompleted(roomId))
        {
            IsCompleted = true;
            OpenDoors();
        }
        else
        {
            CloseDoors();
        }
    }

    public void CompleteRoom()
    {
        if (IsCompleted)
        {
            return;
        }

        IsCompleted = true;

        RunSession runSession = GameFlowController.Instance?.CurrentRunSession;

        if (runSession != null)
        {
            runSession.CompleteRoom(roomId);
        }

        OpenDoors();
        if (mapController != null)
        {
            mapController.OpenConnectedDoors(this);
        }
        Debug.Log($"Room completed: {roomId}");
    }

    private void OpenDoors()
    {
        foreach (RoomDoor door in doors)
        {
            door.Open();
        }
    }

    private void CloseDoors()
    {
        foreach (RoomDoor door in doors)
        {
            door.Close();
        }
    }
    public Transform GetEntryPointFromDirection(DoorDirectionType exitDirection)
    {
        switch (exitDirection)
        {
            case DoorDirectionType.Top:
                return downEntryPoint;
            case DoorDirectionType.Down:
                return topEntryPoint;
            case DoorDirectionType.Left:
                return rightEntryPoint;
            case DoorDirectionType.Right:
                return leftEntryPoint;
            default:
                return null;
        }
    }
    private void SpawnNode(RoomData roomData)
    {
        if (currentNode != null)
        {
            Destroy(currentNode.gameObject);
            currentNode = null;
        }

        if (nodePrefab == null)
        {
            Debug.LogWarning("RoomController: Node prefab is not assigned.");
            return;
        }

        Transform spawnParent = nodeRoot != null ? nodeRoot : transform;
        Vector3 spawnPosition = nodeSpawnPoint != null ? nodeSpawnPoint.position : transform.position;
        Quaternion spawnRotation = nodeSpawnPoint != null ? nodeSpawnPoint.rotation : Quaternion.identity;

        currentNode = Instantiate(nodePrefab, spawnPosition, spawnRotation, spawnParent);
        currentNode.Activate(roomData.NodeId, roomData.NodeType, this);
    }

    public void InitializeDoors(ExploreMapController mapController)
    {
        this.mapController = mapController;

        foreach (RoomDoor door in doors)
        {
            if (door != null)
            {
                door.Initialize(mapController);
            }
        }
    }
    public void OpenDoor(DoorDirectionType direction)
    {
        foreach (RoomDoor door in doors)
        {
            if (door != null && door.Direction == direction)
            {
                door.Open();
                return;
            }
        }
    }

    
}
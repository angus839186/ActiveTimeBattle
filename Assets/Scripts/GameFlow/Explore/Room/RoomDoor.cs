using UnityEngine;
public enum DoorDirectionType
{
    Left,
    Right,
    Top,
    Down
}
public class RoomDoor : MonoBehaviour, IExploreInteractable
{
    [SerializeField] private DoorDirectionType direction;
    public DoorDirectionType Direction => direction;
    [SerializeField] private GameObject doorObject;

    [SerializeField] private string connectedRoomId;

    [SerializeField] private ExploreMapController mapSpawner;

    public string ConnectedRoomId => connectedRoomId;

    private bool isOpen;

    public bool IsOpen => isOpen;
    private bool isAvailable;

    private void Awake()
    {
        Close();
    }

    public void Initialize(ExploreMapController mapSpawner)
    {
        this.mapSpawner = mapSpawner;
    }

    public void Open()
    {
        if (!isAvailable)
        {
            return;
        }
        if (isOpen)
        {
            return;
        }

        isOpen = true;

        if (doorObject != null)
        {
            doorObject.SetActive(false);
        }

        // Debug.Log($"Door opened: {direction}");
    }

    public void Close()
    {
        isOpen = false;

        if (doorObject != null)
        {
            doorObject.SetActive(true);
        }

        // Debug.Log($"Door closed: {direction}");
    }

    public void SetAvailable(bool isAvailable)
    {
        this.isAvailable = isAvailable;
        gameObject.SetActive(isAvailable);
    }

    public void SetConnectedRoom(string roomId)
    {
        connectedRoomId = roomId;
    }
    public void Interact()
    {
        if (string.IsNullOrEmpty(connectedRoomId))
        {
            Debug.LogWarning("RoomDoor: ConnectedRoomId is empty.");
            return;
        }

        if (!isOpen)
        {
            return;
        }

        mapSpawner.ApplyRoom(connectedRoomId, direction);
    }
}
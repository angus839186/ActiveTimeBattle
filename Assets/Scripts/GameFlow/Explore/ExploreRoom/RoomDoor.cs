using UnityEngine;

public class RoomDoor : MonoBehaviour
{
    [SerializeField] private DoorDirectionType direction;
    public DoorDirectionType Direction => direction;
    [SerializeField] private GameObject doorVisual;
    [SerializeField] private Collider doorCollider;

    [SerializeField] private string connectedRoomId;

    public string ConnectedRoomId => connectedRoomId;

    private bool isOpen;

    public bool IsOpen => isOpen;

    private void Awake()
    {
        Close();
    }

    public void Open()
    {
        if (isOpen)
        {
            return;
        }

        isOpen = true;

        if (doorVisual != null)
        {
            doorVisual.SetActive(false);
        }

        if (doorCollider != null)
        {
            doorCollider.enabled = false;
        }

        // Debug.Log($"Door opened: {direction}");
    }

    public void Close()
    {
        isOpen = false;

        if (doorVisual != null)
        {
            doorVisual.SetActive(true);
        }

        if (doorCollider != null)
        {
            doorCollider.enabled = true;
        }

        // Debug.Log($"Door closed: {direction}");
    }

    public void SetAvailable(bool isAvailable)
    {
        gameObject.SetActive(isAvailable);
    }

    public void SetConnectedRoom(string roomId)
    {
        connectedRoomId = roomId;
    }
}
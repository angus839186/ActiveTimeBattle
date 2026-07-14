using UnityEngine;

public class RoomDoor : MonoBehaviour
{
    [SerializeField] private string direction;

    public void Open()
    {
        Debug.Log($"Door opened: {direction}");
    }

    public void Close()
    {
        Debug.Log($"Door closed: {direction}");
    }
}
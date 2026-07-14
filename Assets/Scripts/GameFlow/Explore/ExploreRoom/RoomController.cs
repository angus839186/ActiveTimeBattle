using UnityEngine;

public class RoomController : MonoBehaviour
{
    [SerializeField] private string roomId;
    [SerializeField] private RoomDoor[] doors;

    public string RoomId => roomId;
    public bool IsCompleted { get; private set; }

    public void CompleteRoom()
    {
        if (IsCompleted)
        {
            return;
        }

        IsCompleted = true;

        foreach (RoomDoor door in doors)
        {
            door.Open();
        }

        Debug.Log($"Room completed: {roomId}");
    }
}
using UnityEngine;

public class RoomDoorTransition : MonoBehaviour
{
    [SerializeField] private ExploreMapSpawner mapSpawner;
    [SerializeField] private RoomDoor roomDoor;

    public void Initialize(ExploreMapSpawner mapSpawner)
    {
        this.mapSpawner = mapSpawner;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent<CharacterController>(out _))
        {
            return;
        }

        if (roomDoor == null)
        {
            Debug.LogWarning("RoomDoorTransition: RoomDoor is not assigned.");
            return;
        }

        if (string.IsNullOrEmpty(roomDoor.ConnectedRoomId))
        {
            Debug.LogWarning("RoomDoorTransition: ConnectedRoomId is empty.");
            return;
        }

        if (!roomDoor.IsOpen)
        {
            return;
        }

        if (mapSpawner == null)
        {
            Debug.LogWarning("RoomDoorTransition: MapSpawner is not assigned.");
            return;
        }

        mapSpawner.ApplyRoom(roomDoor.ConnectedRoomId, roomDoor.Direction);
    }
}
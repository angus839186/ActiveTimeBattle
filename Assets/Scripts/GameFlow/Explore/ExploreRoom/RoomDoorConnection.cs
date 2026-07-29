public class RoomDoorConnection
{
    public DoorDirectionType Direction { get; }
    public string ConnectedRoomId { get; }

    public RoomDoorConnection(DoorDirectionType direction, string connectedRoomId)
    {
        Direction = direction;
        ConnectedRoomId = connectedRoomId;
    }
}
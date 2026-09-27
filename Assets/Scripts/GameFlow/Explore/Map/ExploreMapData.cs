using System.Collections.Generic;

public class ExploreMapData
{
    private readonly List<RoomData> rooms = new List<RoomData>();

    public IReadOnlyList<RoomData> Rooms => rooms;
    public string StartRoomId { get; private set; }

    public void SetStartRoom(string roomId)
    {
        StartRoomId = roomId;
    }

    public void AddRoom(RoomData roomData)
    {
        rooms.Add(roomData);
    }
    public RoomData GetRoom(string roomId)
    {
        foreach (RoomData room in rooms)
        {
            if (room.RoomId == roomId)
            {
                return room;
            }
        }

        return null;
    }
}
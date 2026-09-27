using System.Collections.Generic;
using UnityEngine;

public class RoomData
{
    private readonly List<RoomDoorConnection> doorConnections = new List<RoomDoorConnection>();

    public string RoomId { get; }
    public Vector2Int GridPosition { get; }
    public string NodeId { get; }
    public ExploreNodeType NodeType { get; }
    public IReadOnlyList<RoomDoorConnection> DoorConnections => doorConnections;

    public RoomData(
        string roomId,
        Vector2Int gridPosition,
        string nodeId,
        ExploreNodeType nodeType)
    {
        RoomId = roomId;
        GridPosition = gridPosition;
        NodeId = nodeId;
        NodeType = nodeType;
    }

    public void AddDoorConnection(DoorDirectionType direction, string connectedRoomId)
    {
        doorConnections.Add(new RoomDoorConnection(direction, connectedRoomId));
    }

    public string GetConnectedRoomId(DoorDirectionType direction)
    {
        foreach (RoomDoorConnection connection in doorConnections)
        {
            if (connection.Direction == direction)
            {
                return connection.ConnectedRoomId;
            }
        }

        return string.Empty;
    }
}
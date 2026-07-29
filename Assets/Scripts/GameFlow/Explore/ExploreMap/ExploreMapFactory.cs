using System;
using UnityEngine;
public static class ExploreMapFactory
{
    public static ExploreMapData CreateMap(int seed, ExploreNodeSpawnCount[] nodeCounts)
    {
        ExploreMapData map = new ExploreMapData();

        RoomData enterRoom = new RoomData(
            "room_001",
            UnityEngine.Vector2Int.zero,
            "room_001_enter",
            ExploreNodeType.Enter);

        map.AddRoom(enterRoom);
        map.SetStartRoom("room_001");

        return map;
    }

    private static DoorDirectionType GetDirection(Vector2Int from, Vector2Int to)
    {
        Vector2Int delta = to - from;

        if (delta == Vector2Int.up) return DoorDirectionType.Top;
        if (delta == Vector2Int.down) return DoorDirectionType.Down;
        if (delta == Vector2Int.left) return DoorDirectionType.Left;
        if (delta == Vector2Int.right) return DoorDirectionType.Right;

        Debug.LogWarning($"Invalid room direction: {from} -> {to}");
        return DoorDirectionType.Top;
    }

    private static DoorDirectionType GetOppositeDirection(DoorDirectionType direction)
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
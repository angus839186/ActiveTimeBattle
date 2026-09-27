using System;
using UnityEngine;
using System.Collections.Generic;
public static class ExploreMapFactory
{
    public static ExploreMapData CreateMap(int seed, ExploreNodeSpawnCount[] nodeCounts)
    {
        System.Random random = new System.Random(seed);
        List<ExploreNodeType> nodeTypes = BuildNodeTypes(nodeCounts);
        List<Vector2Int> positions = GenerateConnectedPositions(nodeTypes.Count, random);

        int enterIndex = 0;
        int outIndex = GetFarthestPositionIndex(positions, positions[enterIndex]);

        ExploreMapData map = new ExploreMapData();
        Dictionary<Vector2Int, RoomData> roomsByPosition = new Dictionary<Vector2Int, RoomData>();

        List<ExploreNodeType> remainingNodes = BuildRemainingNodes(nodeCounts);
        Shuffle(remainingNodes, random);

        int remainingIndex = 0;

        for (int i = 0; i < positions.Count; i++)
        {
            ExploreNodeType nodeType;

            if (i == enterIndex) nodeType = ExploreNodeType.Enter;
            else if (i == outIndex) nodeType = ExploreNodeType.Out;
            else nodeType = remainingNodes[remainingIndex++];

            string roomId = $"room_{i + 1:000}";
            string nodeId = $"{roomId}_{nodeType.ToString().ToLowerInvariant()}";

            RoomData room = new RoomData(roomId, positions[i], nodeId, nodeType);
            map.AddRoom(room);
            roomsByPosition.Add(positions[i], room);

            if (nodeType == ExploreNodeType.Enter)
            {
                map.SetStartRoom(roomId);
            }
        }

        AddConnections(roomsByPosition);

        return map;
    }
    private static List<ExploreNodeType> BuildNodeTypes(ExploreNodeSpawnCount[] nodeCounts)
    {
        List<ExploreNodeType> nodeTypes = new List<ExploreNodeType>();
        nodeTypes.Add(ExploreNodeType.Enter);
        nodeTypes.AddRange(BuildRemainingNodes(nodeCounts));
        nodeTypes.Add(ExploreNodeType.Out);
        return nodeTypes;
    }
    private static void AddConnections(Dictionary<Vector2Int, RoomData> roomsByPosition)
    {
        foreach (KeyValuePair<Vector2Int, RoomData> pair in roomsByPosition)
        {
            Vector2Int position = pair.Key;
            RoomData room = pair.Value;

            TryAddConnection(room, roomsByPosition, position, Vector2Int.up, DoorDirectionType.Top);
            TryAddConnection(room, roomsByPosition, position, Vector2Int.down, DoorDirectionType.Down);
            TryAddConnection(room, roomsByPosition, position, Vector2Int.left, DoorDirectionType.Left);
            TryAddConnection(room, roomsByPosition, position, Vector2Int.right, DoorDirectionType.Right);
        }
    }
    private static void TryAddConnection(
    RoomData room,
    Dictionary<Vector2Int, RoomData> roomsByPosition,
    Vector2Int position,
    Vector2Int offset,
    DoorDirectionType direction)
    {
        if (roomsByPosition.TryGetValue(position + offset, out RoomData connectedRoom))
        {
            room.AddDoorConnection(direction, connectedRoom.RoomId);
        }
    }
    private static List<ExploreNodeType> BuildRemainingNodes(ExploreNodeSpawnCount[] nodeCounts)
    {
        List<ExploreNodeType> nodes = new List<ExploreNodeType>();

        if (nodeCounts == null)
        {
            return nodes;
        }

        foreach (ExploreNodeSpawnCount count in nodeCounts)
        {
            if (count == null || count.Count <= 0) continue;
            if (count.NodeType == ExploreNodeType.Enter || count.NodeType == ExploreNodeType.Out) continue;

            for (int i = 0; i < count.Count; i++)
            {
                nodes.Add(count.NodeType);
            }
        }

        return nodes;
    }
    private static int GetFarthestPositionIndex(List<Vector2Int> positions, Vector2Int from)
    {
        int farthestIndex = 0;
        int farthestDistance = -1;

        for (int i = 0; i < positions.Count; i++)
        {
            int distance = Mathf.Abs(positions[i].x - from.x) + Mathf.Abs(positions[i].y - from.y);

            if (distance > farthestDistance)
            {
                farthestDistance = distance;
                farthestIndex = i;
            }
        }

        return farthestIndex;
    }
    private static List<Vector2Int> GenerateConnectedPositions(int count, System.Random random)
    {
        List<Vector2Int> positions = new List<Vector2Int> { Vector2Int.zero };
        HashSet<Vector2Int> used = new HashSet<Vector2Int> { Vector2Int.zero };

        Vector2Int[] directions =
        {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right
    };

        while (positions.Count < count)
        {
            Vector2Int basePosition = positions[random.Next(positions.Count)];
            Shuffle(directions, random);

            foreach (Vector2Int direction in directions)
            {
                Vector2Int candidate = basePosition + direction;

                if (used.Contains(candidate)) continue;

                used.Add(candidate);
                positions.Add(candidate);
                break;
            }
        }

        return positions;
    }

    private static void Shuffle<T>(IList<T> list, System.Random random)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
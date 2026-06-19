using System;
using System.Collections.Generic;
using System.Linq;

namespace ActiveTimeBattle.Domain.Exploration
{
    public sealed class ExplorationMap
    {
        private readonly Dictionary<int, ExplorationNode> _nodes;

        public ExplorationMap(
            int seed,
            int startNodeId,
            IEnumerable<ExplorationNode> nodes)
        {
            Seed = seed;
            _nodes = nodes?.ToDictionary(node => node.Id)
                ?? throw new ArgumentNullException(nameof(nodes));
            if (!_nodes.ContainsKey(startNodeId))
            {
                throw new ArgumentException(
                    "Start node must exist in the map.",
                    nameof(startNodeId));
            }

            StartNodeId = startNodeId;
        }

        public int Seed { get; }

        public int StartNodeId { get; }

        public IReadOnlyCollection<ExplorationNode> Nodes => _nodes.Values;

        public ExplorationNode GetNode(int nodeId)
        {
            return _nodes.TryGetValue(nodeId, out ExplorationNode node)
                ? node
                : throw new ArgumentOutOfRangeException(nameof(nodeId));
        }

        public IReadOnlyList<ExplorationNode> GetNextNodes(int nodeId)
        {
            return GetNode(nodeId).NextNodeIds
                .Select(GetNode)
                .ToArray();
        }

        public IReadOnlyList<ExplorationNode> GetConnectedNodes(int nodeId)
        {
            ExplorationNode current = GetNode(nodeId);
            return _nodes.Values
                .Where(
                    node => current.NextNodeIds.Contains(node.Id)
                        || node.NextNodeIds.Contains(nodeId))
                .OrderBy(node => node.Id)
                .ToArray();
        }
    }
}

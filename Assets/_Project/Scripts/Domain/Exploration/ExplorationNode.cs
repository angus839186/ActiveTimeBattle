using System;
using System.Collections.Generic;
using System.Linq;

namespace ActiveTimeBattle.Domain.Exploration
{
    public sealed class ExplorationNode
    {
        private readonly int[] _nextNodeIds;

        public ExplorationNode(
            int id,
            int depth,
            ExplorationNodeType type,
            IEnumerable<int> nextNodeIds)
        {
            if (id < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(id));
            }

            if (depth < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(depth));
            }

            Id = id;
            Depth = depth;
            Type = type;
            _nextNodeIds = nextNodeIds?.Distinct().OrderBy(value => value).ToArray()
                ?? throw new ArgumentNullException(nameof(nextNodeIds));
        }

        public int Id { get; }

        public int Depth { get; }

        public ExplorationNodeType Type { get; }

        public IReadOnlyList<int> NextNodeIds => _nextNodeIds;
    }
}

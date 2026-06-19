using System;
using System.Collections.Generic;
using System.Linq;

namespace ActiveTimeBattle.Domain.Exploration
{
    public sealed class ExplorationMapGenerator
    {
        private static readonly int[] LayerWidths = { 1, 2, 2, 2, 1 };

        private static readonly ExplorationNodeType[] RegularNodeTypes =
        {
            ExplorationNodeType.Combat,
            ExplorationNodeType.Combat,
            ExplorationNodeType.Treasure,
            ExplorationNodeType.Rest
        };

        public ExplorationMap Generate(int seed)
        {
            Random random = new Random(seed);
            List<List<NodeBuilder>> layers = CreateLayers(random);
            ConnectLayers(layers, random);

            ExplorationNode[] nodes = layers
                .SelectMany(layer => layer)
                .Select(builder => builder.Build())
                .ToArray();
            return new ExplorationMap(seed, layers[0][0].Id, nodes);
        }

        private static List<List<NodeBuilder>> CreateLayers(Random random)
        {
            List<List<NodeBuilder>> layers =
                new List<List<NodeBuilder>>(LayerWidths.Length);
            int nextId = 0;

            for (int depth = 0; depth < LayerWidths.Length; depth++)
            {
                List<NodeBuilder> layer =
                    new List<NodeBuilder>(LayerWidths[depth]);
                for (int index = 0; index < LayerWidths[depth]; index++)
                {
                    ExplorationNodeType type;
                    if (depth == 0)
                    {
                        type = ExplorationNodeType.Entrance;
                    }
                    else if (depth == LayerWidths.Length - 1)
                    {
                        type = ExplorationNodeType.Boss;
                    }
                    else
                    {
                        type = RegularNodeTypes[
                            random.Next(RegularNodeTypes.Length)];
                    }

                    layer.Add(new NodeBuilder(nextId++, depth, type));
                }

                layers.Add(layer);
            }

            return layers;
        }

        private static void ConnectLayers(
            IReadOnlyList<List<NodeBuilder>> layers,
            Random random)
        {
            for (int depth = 0; depth < layers.Count - 1; depth++)
            {
                List<NodeBuilder> current = layers[depth];
                List<NodeBuilder> next = layers[depth + 1];

                for (int index = 0; index < current.Count; index++)
                {
                    int targetIndex = random.Next(next.Count);
                    current[index].AddNext(next[targetIndex].Id);

                    if (next.Count > 1 && random.NextDouble() < 0.65d)
                    {
                        current[index].AddNext(
                            next[(targetIndex + 1) % next.Count].Id);
                    }
                }

                for (int nextIndex = 0; nextIndex < next.Count; nextIndex++)
                {
                    if (current.Any(
                            node => node.NextNodeIds.Contains(next[nextIndex].Id)))
                    {
                        continue;
                    }

                    current[nextIndex % current.Count].AddNext(next[nextIndex].Id);
                }
            }
        }

        private sealed class NodeBuilder
        {
            private readonly HashSet<int> _nextNodeIds = new HashSet<int>();

            public NodeBuilder(
                int id,
                int depth,
                ExplorationNodeType type)
            {
                Id = id;
                Depth = depth;
                Type = type;
            }

            public int Id { get; }

            public int Depth { get; }

            public ExplorationNodeType Type { get; }

            public IReadOnlyCollection<int> NextNodeIds => _nextNodeIds;

            public void AddNext(int nodeId)
            {
                _nextNodeIds.Add(nodeId);
            }

            public ExplorationNode Build()
            {
                return new ExplorationNode(Id, Depth, Type, _nextNodeIds);
            }
        }
    }
}

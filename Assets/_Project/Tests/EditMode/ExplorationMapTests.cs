using System.Collections.Generic;
using System.Linq;
using ActiveTimeBattle.Domain;
using ActiveTimeBattle.Domain.Exploration;
using NUnit.Framework;

namespace ActiveTimeBattle.Tests
{
    public sealed class ExplorationMapTests
    {
        [Test]
        public void SameSeedProducesSameNodesAndConnections()
        {
            ExplorationMapGenerator generator = new ExplorationMapGenerator();

            ExplorationMap first = generator.Generate(123456);
            ExplorationMap second = generator.Generate(123456);

            CollectionAssert.AreEqual(
                GetFingerprint(first),
                GetFingerprint(second));
        }

        [Test]
        public void DifferentSeedsChangeGeneratedMap()
        {
            ExplorationMapGenerator generator = new ExplorationMapGenerator();

            ExplorationMap first = generator.Generate(123456);
            ExplorationMap second = generator.Generate(654321);

            CollectionAssert.AreNotEqual(
                GetFingerprint(first),
                GetFingerprint(second));
        }

        [Test]
        public void EveryNonBossNodeConnectsForwardAndBossEndsRoute()
        {
            ExplorationMap map = new ExplorationMapGenerator().Generate(77);

            foreach (ExplorationNode node in map.Nodes)
            {
                if (node.Type == ExplorationNodeType.Boss)
                {
                    Assert.That(node.NextNodeIds, Is.Empty);
                    continue;
                }

                Assert.That(node.NextNodeIds, Is.Not.Empty);
                Assert.That(
                    node.NextNodeIds.All(
                        nextId => map.GetNode(nextId).Depth == node.Depth + 1),
                    Is.True);
            }
        }

        [Test]
        public void NonCombatNodeAllowsMovementBeforeCompletion()
        {
            RunSession session = new RunSession(42);
            int nextNodeId = session.ExplorationMap
                .GetNextNodes(session.CurrentNodeId)[0]
                .Id;

            Assert.That(session.IsNodeVisited(session.CurrentNodeId), Is.True);
            Assert.That(
                session.CurrentNode.Type,
                Is.EqualTo(ExplorationNodeType.Entrance));
            Assert.That(session.IsCurrentNodeCompleted, Is.True);

            Assert.That(session.TryMoveToNode(nextNodeId), Is.True);
            Assert.That(session.Rewards, Is.Empty);
            Assert.That(session.CurrentNodeId, Is.EqualTo(nextNodeId));
            Assert.That(session.PreviousNodeId, Is.EqualTo(session.ExplorationMap.StartNodeId));
            Assert.That(session.IsNodeVisited(nextNodeId), Is.True);
            Assert.That(session.IsCurrentNodeCompleted, Is.False);
        }

        [Test]
        public void CombatNodeLocksConnectedDoorsUntilCompleted()
        {
            RunSession session = CreateSessionAtNode(
                ExplorationNodeType.Combat);
            int connectedNodeId = session.ExplorationMap
                .GetConnectedNodes(session.CurrentNodeId)[0]
                .Id;

            Assert.That(session.AreCurrentNodeExitsLocked, Is.True);
            Assert.That(session.TryMoveToNode(connectedNodeId), Is.False);

            session.CompleteCurrentNode();

            Assert.That(session.AreCurrentNodeExitsLocked, Is.False);
            Assert.That(session.TryMoveToNode(connectedNodeId), Is.True);
        }

        [Test]
        public void TreasureAndRestNodesDoNotLockConnectedDoors()
        {
            RunSession treasureSession = CreateSessionAtNode(
                ExplorationNodeType.Treasure);
            RunSession restSession = CreateSessionAtNode(
                ExplorationNodeType.Rest);

            Assert.That(treasureSession.IsCurrentNodeCompleted, Is.False);
            Assert.That(treasureSession.AreCurrentNodeExitsLocked, Is.False);
            Assert.That(restSession.IsCurrentNodeCompleted, Is.False);
            Assert.That(restSession.AreCurrentNodeExitsLocked, Is.False);
        }

        [Test]
        public void ConnectedNodesIncludeForwardAndPreviousRoutes()
        {
            RunSession session = new RunSession(42);
            int startNodeId = session.CurrentNodeId;
            int nextNodeId = session.AvailableNextNodes[0].Id;

            Assert.That(session.TryMoveToNode(nextNodeId), Is.True);

            CollectionAssert.Contains(
                session.ExplorationMap
                    .GetConnectedNodes(nextNodeId)
                    .Select(node => node.Id)
                    .ToArray(),
                startNodeId);
        }

        [Test]
        public void SessionCanReturnToPreviousNode()
        {
            RunSession session = new RunSession(42);
            int startNodeId = session.CurrentNodeId;
            int nextNodeId = session.ExplorationMap
                .GetNextNodes(startNodeId)[0]
                .Id;

            session.CompleteCurrentNode();
            Assert.That(session.TryMoveToNode(nextNodeId), Is.True);

            Assert.That(session.TryReturnToPreviousNode(), Is.True);
            Assert.That(session.CurrentNodeId, Is.EqualTo(startNodeId));
            Assert.That(session.IsCurrentNodeCompleted, Is.True);
        }

        [Test]
        public void GeneratedRouteCanReachAndCompleteBoss()
        {
            RunSession session = new RunSession(20260613);
            int safety = 0;

            while (!session.IsRunComplete && safety++ < 20)
            {
                session.CompleteCurrentNode();
                if (session.AvailableNextNodes.Count > 0)
                {
                    session.TryMoveToNode(session.AvailableNextNodes[0].Id);
                }
            }

            Assert.That(session.IsRunComplete, Is.True);
            Assert.That(
                session.CurrentNode.Type,
                Is.EqualTo(ExplorationNodeType.Boss));
            Assert.That(session.Rewards, Has.Count.EqualTo(4));
        }

        [Test]
        public void TreasureGrantsResourcesOrUnlocksSkill()
        {
            RunSession session = CreateSessionAtNode(
                ExplorationNodeType.Treasure);
            int resourcesBefore = session.Resources;
            bool hadSkillBefore = session.HasUnlockedSkill("e");

            string result = session.OpenTreasure();

            Assert.That(result, Is.Not.Empty);
            Assert.That(session.IsCurrentNodeCompleted, Is.True);
            Assert.That(
                session.Resources > resourcesBefore
                || session.HasUnlockedSkill("e") != hadSkillBefore,
                Is.True);
        }

        [Test]
        public void RestNodeRestoresPlayerHealth()
        {
            RunSession session = CreateSessionAtNode(
                ExplorationNodeType.Rest);
            session.Player.ApplyDamage(60);

            string result = session.RestAtCamp(40);

            Assert.That(result, Does.Contain("40"));
            Assert.That(session.Player.CurrentHealth, Is.EqualTo(80));
            Assert.That(session.IsCurrentNodeCompleted, Is.True);
        }

        [Test]
        public void RestNodeCanGrantMoneyOrDoNothing()
        {
            RunSession moneySession = CreateSessionAtNode(
                ExplorationNodeType.Rest);
            string moneyResult = moneySession.TakeCampMoney(25);

            Assert.That(moneyResult, Does.Contain("25"));
            Assert.That(moneySession.Resources, Is.EqualTo(25));
            Assert.That(moneySession.IsCurrentNodeCompleted, Is.True);

            RunSession leaveSession = CreateSessionAtNode(
                ExplorationNodeType.Rest);
            string leaveResult = leaveSession.LeaveCamp();

            Assert.That(leaveResult, Does.Contain("沒有採取行動"));
            Assert.That(leaveSession.Resources, Is.Zero);
            Assert.That(leaveSession.IsCurrentNodeCompleted, Is.True);
        }

        private static string[] GetFingerprint(ExplorationMap map)
        {
            return map.Nodes
                .OrderBy(node => node.Id)
                .Select(
                    node =>
                        $"{node.Id}:{node.Depth}:{node.Type}:"
                        + string.Join(",", node.NextNodeIds))
                .ToArray();
        }

        private static RunSession CreateSessionAtNode(
            ExplorationNodeType targetType)
        {
            for (int seed = 1; seed <= 1000; seed++)
            {
                RunSession session = new RunSession(seed);
                List<int> path = new List<int>();
                if (!TryFindPath(
                        session.ExplorationMap,
                        session.CurrentNodeId,
                        targetType,
                        path))
                {
                    continue;
                }

                foreach (int nodeId in path)
                {
                    session.CompleteCurrentNode();
                    Assert.That(session.TryMoveToNode(nodeId), Is.True);
                }

                return session;
            }

            Assert.Fail($"No reachable {targetType} node was generated.");
            return null;
        }

        private static bool TryFindPath(
            ExplorationMap map,
            int nodeId,
            ExplorationNodeType targetType,
            List<int> path)
        {
            ExplorationNode node = map.GetNode(nodeId);
            if (node.Type == targetType)
            {
                return true;
            }

            foreach (int nextNodeId in node.NextNodeIds)
            {
                path.Add(nextNodeId);
                if (TryFindPath(map, nextNodeId, targetType, path))
                {
                    return true;
                }

                path.RemoveAt(path.Count - 1);
            }

            return false;
        }
    }
}

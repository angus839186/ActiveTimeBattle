using System;
using System.Collections.Generic;
using ActiveTimeBattle.Domain.Combat;
using ActiveTimeBattle.Domain.Exploration;

namespace ActiveTimeBattle.Domain
{
    public sealed class RunSession
    {
        private readonly List<string> _rewards = new List<string>();
        private readonly HashSet<int> _visitedNodeIds = new HashSet<int>();
        private readonly HashSet<int> _completedNodeIds = new HashSet<int>();
        private readonly HashSet<string> _unlockedSkillIds =
            new HashSet<string>(StringComparer.Ordinal)
            {
                "q",
                "w"
            };

        public RunSession(int seed)
        {
            Seed = seed;
            CreatedAtUtc = DateTime.UtcNow;
            ExplorationMap = new ExplorationMapGenerator().Generate(seed);
            CurrentNodeId = ExplorationMap.StartNodeId;
            _visitedNodeIds.Add(CurrentNodeId);
            if (CurrentNode.Type == ExplorationNodeType.Entrance)
            {
                IsCurrentNodeCompleted = true;
                _completedNodeIds.Add(CurrentNodeId);
            }

            Player = new CharacterRuntime(100);
        }

        public int Seed { get; }

        public DateTime CreatedAtUtc { get; }

        public ExplorationMap ExplorationMap { get; }

        public CharacterRuntime Player { get; }

        public int Resources { get; private set; }

        public int CurrentNodeId { get; private set; }

        public int? PreviousNodeId { get; private set; }

        public bool IsCurrentNodeCompleted { get; private set; }

        public ExplorationNode CurrentNode =>
            ExplorationMap.GetNode(CurrentNodeId);

        public IReadOnlyList<ExplorationNode> AvailableNextNodes =>
            !AreCurrentNodeExitsLocked
                ? ExplorationMap.GetNextNodes(CurrentNodeId)
                : Array.Empty<ExplorationNode>();

        public IReadOnlyList<ExplorationNode> AvailableConnectedNodes =>
            !AreCurrentNodeExitsLocked
                ? ExplorationMap.GetConnectedNodes(CurrentNodeId)
                : Array.Empty<ExplorationNode>();

        public bool AreCurrentNodeExitsLocked =>
            !IsCurrentNodeCompleted
            && (CurrentNode.Type == ExplorationNodeType.Combat
                || CurrentNode.Type == ExplorationNodeType.Boss);

        public bool IsRunComplete =>
            IsCurrentNodeCompleted && AvailableNextNodes.Count == 0;

        public IReadOnlyList<string> Rewards => _rewards;

        public bool IsNodeVisited(int nodeId)
        {
            return _visitedNodeIds.Contains(nodeId);
        }

        public bool IsNodeCompleted(int nodeId)
        {
            return _completedNodeIds.Contains(nodeId);
        }

        public bool HasUnlockedSkill(string skillId)
        {
            return !string.IsNullOrWhiteSpace(skillId)
                && _unlockedSkillIds.Contains(skillId);
        }

        public string OpenTreasure()
        {
            if (IsCurrentNodeCompleted
                || CurrentNode.Type != ExplorationNodeType.Treasure)
            {
                return string.Empty;
            }

            string lockedSkillId = GetTreasureSkillId();
            bool grantsSkill =
                lockedSkillId != null
                && ((Seed ^ CurrentNode.Id) & 1) == 0;
            string reward;
            if (grantsSkill)
            {
                _unlockedSkillIds.Add(lockedSkillId);
                reward = $"獲得新技能：{GetSkillLabel(lockedSkillId)}";
            }
            else
            {
                int amount =
                    20 + (int)Math.Abs(((long)Seed + CurrentNode.Id) % 21L);
                Resources += amount;
                reward = $"獲得資源：{amount}";
            }

            CompleteCurrentNode(reward);
            return reward;
        }

        public string RestAtCamp(int healAmount)
        {
            if (IsCurrentNodeCompleted
                || CurrentNode.Type != ExplorationNodeType.Rest)
            {
                return string.Empty;
            }

            int restored = Player.RestoreHealth(healAmount);
            string reward = restored > 0
                ? $"營地休息：回復 {restored} 生命"
                : "營地休息：生命已滿";
            CompleteCurrentNode(reward);
            return reward;
        }

        public string TakeCampMoney(int amount)
        {
            if (IsCurrentNodeCompleted
                || CurrentNode.Type != ExplorationNodeType.Rest)
            {
                return string.Empty;
            }

            Resources += amount;
            string reward = $"營地補給：獲得金錢 {amount}";
            CompleteCurrentNode(reward);
            return reward;
        }

        public string LeaveCamp()
        {
            if (IsCurrentNodeCompleted
                || CurrentNode.Type != ExplorationNodeType.Rest)
            {
                return string.Empty;
            }

            string reward = "營地休息：沒有採取行動";
            CompleteCurrentNode(reward);
            return reward;
        }

        public void CompleteCurrentNode()
        {
            CompleteCurrentNode(GetRewardLabel(CurrentNode.Type));
        }

        public void CompleteCurrentNode(string rewardLabel)
        {
            if (IsCurrentNodeCompleted)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(rewardLabel))
            {
                _rewards.Add(rewardLabel);
            }

            IsCurrentNodeCompleted = true;
            _completedNodeIds.Add(CurrentNodeId);
        }

        public bool TryMoveToNode(int nodeId)
        {
            if (AreCurrentNodeExitsLocked)
            {
                return false;
            }

            IReadOnlyList<ExplorationNode> connectedNodes =
                ExplorationMap.GetConnectedNodes(CurrentNodeId);
            bool isAvailable = false;
            for (int index = 0; index < connectedNodes.Count; index++)
            {
                if (connectedNodes[index].Id == nodeId)
                {
                    isAvailable = true;
                    break;
                }
            }

            if (!isAvailable)
            {
                return false;
            }

            MoveToNode(nodeId);
            return true;
        }

        public bool TryReturnToPreviousNode()
        {
            if (!PreviousNodeId.HasValue)
            {
                return false;
            }

            MoveToNode(PreviousNodeId.Value);
            return true;
        }

        private void MoveToNode(int nodeId)
        {
            PreviousNodeId = CurrentNodeId;
            CurrentNodeId = nodeId;
            IsCurrentNodeCompleted = _completedNodeIds.Contains(CurrentNodeId);
            _visitedNodeIds.Add(CurrentNodeId);
        }

        private string GetTreasureSkillId()
        {
            if (!_unlockedSkillIds.Contains("e"))
            {
                return "e";
            }

            return !_unlockedSkillIds.Contains("r") ? "r" : null;
        }

        private static string GetSkillLabel(string skillId)
        {
            return skillId switch
            {
                "e" => "貫穿",
                "r" => "終結技",
                _ => skillId.ToUpperInvariant()
            };
        }

        private static string GetRewardLabel(ExplorationNodeType type)
        {
            return type switch
            {
                ExplorationNodeType.Entrance => string.Empty,
                ExplorationNodeType.Combat => "戰技碎片",
                ExplorationNodeType.Treasure => "寶箱補給",
                ExplorationNodeType.Event => "記憶殘片",
                ExplorationNodeType.Shop => "商店憑證",
                ExplorationNodeType.Rest => "休息祝福",
                ExplorationNodeType.Boss => "深淵核心",
                _ => type.ToString()
            };
        }
    }
}

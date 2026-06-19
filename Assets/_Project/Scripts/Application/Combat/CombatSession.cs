using System;
using System.Collections.Generic;
using System.Linq;
using ActiveTimeBattle.Domain.Combat;

namespace ActiveTimeBattle.Application.Combat
{
    public sealed class CombatSession
    {
        private readonly Dictionary<string, SkillRuntime> _skills;
        private float _executionRemaining;
        private bool _enemyAttackPending;
        private bool _enemyIsExposed;

        public CombatSession(
            CharacterRuntime player,
            CharacterRuntime enemy,
            IEnumerable<SkillRuntime> skills,
            DirectionInputBuffer inputBuffer,
            CombatClock clock,
            int enemyDamage)
        {
            Player = player ?? throw new ArgumentNullException(nameof(player));
            Enemy = enemy ?? throw new ArgumentNullException(nameof(enemy));
            InputBuffer = inputBuffer
                ?? throw new ArgumentNullException(nameof(inputBuffer));
            Clock = clock ?? throw new ArgumentNullException(nameof(clock));
            if (enemyDamage <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(enemyDamage));
            }

            EnemyDamage = enemyDamage;
            _skills = skills?.ToDictionary(
                skill => skill.Definition.Id,
                StringComparer.Ordinal)
                ?? throw new ArgumentNullException(nameof(skills));
        }

        public CharacterRuntime Player { get; }

        public CharacterRuntime Enemy { get; }

        public DirectionInputBuffer InputBuffer { get; }

        public CombatClock Clock { get; }

        public int EnemyDamage { get; }

        public CombatPhase Phase { get; private set; } = CombatPhase.PlayerInput;

        public bool EnemyIsExposed => _enemyIsExposed;

        public IReadOnlyCollection<SkillRuntime> Skills => _skills.Values;

        public bool AddDirection(CombatDirection direction, float timestamp)
        {
            if (Phase != CombatPhase.PlayerInput)
            {
                return false;
            }

            InputBuffer.Add(direction, timestamp);
            bool matchesSkillPrefix = _skills.Values.Any(
                skill => InputBuffer.IsPrefixOf(
                    skill.Definition.Command,
                    timestamp));
            if (!matchesSkillPrefix)
            {
                InputBuffer.Clear();
            }

            return matchesSkillPrefix;
        }

        public bool TryUseSkill(string skillId, float timestamp)
        {
            if (Phase != CombatPhase.PlayerInput)
            {
                return false;
            }

            if (!_skills.TryGetValue(skillId, out SkillRuntime skill)
                || !skill.IsReady
                || !InputBuffer.EndsWith(skill.Definition.Command, timestamp))
            {
                InputBuffer.Clear();
                return false;
            }

            if (!skill.TryBeginCooldown())
            {
                return false;
            }

            InputBuffer.Clear();
            bool consumesExposure =
                _enemyIsExposed && skill.Definition.Id != "w";
            int damage = consumesExposure
                ? (int)Math.Ceiling(skill.Definition.Damage * 1.5f)
                : skill.Definition.Damage;
            if (consumesExposure)
            {
                _enemyIsExposed = false;
            }

            Enemy.ApplyDamage(damage);
            if (skill.Definition.Id == "w" && !Enemy.IsDefeated)
            {
                _enemyIsExposed = true;
            }

            if (Enemy.IsDefeated)
            {
                Phase = CombatPhase.Victory;
                Clock.IsEnemyClockPaused = true;
                return true;
            }

            _executionRemaining = skill.Definition.ExecutionDuration;
            Phase = CombatPhase.SkillExecution;
            Clock.IsEnemyClockPaused = true;
            return true;
        }

        public void Tick(float deltaTime)
        {
            if (Phase == CombatPhase.Victory || Phase == CombatPhase.Defeat)
            {
                return;
            }

            bool enemyTimerCompleted = Clock.Tick(deltaTime);

            if (Phase == CombatPhase.SkillExecution)
            {
                _executionRemaining = Math.Max(0f, _executionRemaining - deltaTime);
                if (_executionRemaining <= 0f)
                {
                    Clock.IsEnemyClockPaused = false;
                    Phase = CombatPhase.PlayerInput;
                }
            }

            if (enemyTimerCompleted && Phase == CombatPhase.PlayerInput)
            {
                BeginEnemyAttack();
            }

            if (_enemyAttackPending && !Clock.DefenseQteWindow.IsActive)
            {
                ResolveEnemyAttack(DefenseQteResult.Broken);
            }
        }

        public bool TryDefend()
        {
            if (Phase != CombatPhase.DefenseQte
                || !Clock.DefenseQteWindow.IsActive)
            {
                return false;
            }

            DefenseQteResult result = Clock.DefenseQteWindow.Resolve();
            ResolveEnemyAttack(result);
            return true;
        }

        public SkillRuntime GetSkill(string skillId)
        {
            return _skills.TryGetValue(skillId, out SkillRuntime skill)
                ? skill
                : null;
        }

        private void BeginEnemyAttack()
        {
            _enemyAttackPending = true;
            Phase = CombatPhase.DefenseQte;
            Clock.DefenseQteWindow.Open();
        }

        private void ResolveEnemyAttack(DefenseQteResult result)
        {
            if (!_enemyAttackPending)
            {
                return;
            }

            _enemyAttackPending = false;
            Clock.DefenseQteWindow.Close();
            int damage = result switch
            {
                DefenseQteResult.Perfect => 0,
                DefenseQteResult.Normal =>
                    (int)Math.Ceiling(EnemyDamage * 0.5f),
                _ => EnemyDamage
            };
            Player.ApplyDamage(damage);
            Clock.EnemyActionTimer.Reset();

            if (Player.IsDefeated)
            {
                Phase = CombatPhase.Defeat;
                Clock.IsEnemyClockPaused = true;
                return;
            }

            Phase = CombatPhase.PlayerInput;
        }
    }
}

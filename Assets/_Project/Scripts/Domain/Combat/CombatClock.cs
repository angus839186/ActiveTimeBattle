using System;
using System.Collections.Generic;

namespace ActiveTimeBattle.Domain.Combat
{
    public sealed class CombatClock
    {
        private readonly IReadOnlyList<SkillRuntime> _skills;

        public CombatClock(
            EnemyActionTimer enemyActionTimer,
            DefenseQteWindow defenseQteWindow,
            IReadOnlyList<SkillRuntime> skills)
        {
            EnemyActionTimer = enemyActionTimer
                ?? throw new ArgumentNullException(nameof(enemyActionTimer));
            DefenseQteWindow = defenseQteWindow
                ?? throw new ArgumentNullException(nameof(defenseQteWindow));
            _skills = skills
                ?? throw new ArgumentNullException(nameof(skills));
        }

        public EnemyActionTimer EnemyActionTimer { get; }

        public DefenseQteWindow DefenseQteWindow { get; }

        public bool IsEnemyClockPaused { get; set; }

        public bool Tick(float deltaTime)
        {
            if (deltaTime < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(deltaTime));
            }

            foreach (SkillRuntime skill in _skills)
            {
                skill.Tick(deltaTime);
            }

            DefenseQteWindow.Tick(deltaTime);
            return !IsEnemyClockPaused
                && !DefenseQteWindow.IsActive
                && EnemyActionTimer.Tick(deltaTime);
        }
    }
}

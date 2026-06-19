using System;
using System.Collections.Generic;
using System.Linq;

namespace ActiveTimeBattle.Domain.Combat
{
    public sealed class SkillDefinition
    {
        private readonly CombatDirection[] _command;

        public SkillDefinition(
            string id,
            string displayName,
            IEnumerable<CombatDirection> command,
            int damage,
            float cooldownDuration,
            float executionDuration)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Skill id is required.", nameof(id));
            }

            if (string.IsNullOrWhiteSpace(displayName))
            {
                throw new ArgumentException(
                    "Skill display name is required.",
                    nameof(displayName));
            }

            _command = command?.ToArray()
                ?? throw new ArgumentNullException(nameof(command));
            if (_command.Length == 0)
            {
                throw new ArgumentException(
                    "Skill command must contain at least one direction.",
                    nameof(command));
            }

            if (damage < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(damage));
            }

            if (cooldownDuration < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(cooldownDuration));
            }

            if (executionDuration < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(executionDuration));
            }

            Id = id;
            DisplayName = displayName;
            Damage = damage;
            CooldownDuration = cooldownDuration;
            ExecutionDuration = executionDuration;
        }

        public string Id { get; }

        public string DisplayName { get; }

        public IReadOnlyList<CombatDirection> Command => _command;

        public int Damage { get; }

        public float CooldownDuration { get; }

        public float ExecutionDuration { get; }
    }
}

using System;

namespace ActiveTimeBattle.Domain.Combat
{
    public sealed class SkillRuntime
    {
        public SkillRuntime(SkillDefinition definition)
        {
            Definition = definition
                ?? throw new ArgumentNullException(nameof(definition));
        }

        public SkillDefinition Definition { get; }

        public float CooldownRemaining { get; private set; }

        public bool IsReady => CooldownRemaining <= 0f;

        public bool TryBeginCooldown()
        {
            if (!IsReady)
            {
                return false;
            }

            CooldownRemaining = Definition.CooldownDuration;
            return true;
        }

        public void Tick(float deltaTime)
        {
            if (deltaTime < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(deltaTime));
            }

            CooldownRemaining = Math.Max(0f, CooldownRemaining - deltaTime);
        }
    }
}

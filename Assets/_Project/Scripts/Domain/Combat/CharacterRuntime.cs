using System;

namespace ActiveTimeBattle.Domain.Combat
{
    public sealed class CharacterRuntime
    {
        public CharacterRuntime(int maximumHealth)
        {
            if (maximumHealth <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maximumHealth));
            }

            MaximumHealth = maximumHealth;
            CurrentHealth = maximumHealth;
        }

        public int MaximumHealth { get; }

        public int CurrentHealth { get; private set; }

        public bool IsDefeated => CurrentHealth <= 0;

        public int ApplyDamage(int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount));
            }

            int previousHealth = CurrentHealth;
            CurrentHealth = Math.Max(0, CurrentHealth - amount);
            return previousHealth - CurrentHealth;
        }

        public int RestoreHealth(int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount));
            }

            int previousHealth = CurrentHealth;
            CurrentHealth = Math.Min(MaximumHealth, CurrentHealth + amount);
            return CurrentHealth - previousHealth;
        }
    }
}

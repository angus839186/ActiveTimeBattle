using System;

namespace ActiveTimeBattle.Domain.Combat
{
    public sealed class EnemyActionTimer
    {
        public EnemyActionTimer(float duration)
        {
            if (duration <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(duration));
            }

            Duration = duration;
            Remaining = duration;
        }

        public float Duration { get; }

        public float Remaining { get; private set; }

        public float NormalizedRemaining => Remaining / Duration;

        public bool Tick(float deltaTime)
        {
            if (deltaTime < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(deltaTime));
            }

            Remaining = Math.Max(0f, Remaining - deltaTime);
            return Remaining <= 0f;
        }

        public void Reset()
        {
            Remaining = Duration;
        }
    }
}

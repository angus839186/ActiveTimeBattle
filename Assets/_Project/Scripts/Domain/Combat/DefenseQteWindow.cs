using System;

namespace ActiveTimeBattle.Domain.Combat
{
    public sealed class DefenseQteWindow
    {
        public DefenseQteWindow(
            float duration,
            float normalWindow,
            float perfectWindow)
        {
            if (duration <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(duration));
            }

            if (normalWindow <= 0f || normalWindow > duration)
            {
                throw new ArgumentOutOfRangeException(nameof(normalWindow));
            }

            if (perfectWindow <= 0f || perfectWindow > normalWindow)
            {
                throw new ArgumentOutOfRangeException(nameof(perfectWindow));
            }

            Duration = duration;
            NormalWindow = normalWindow;
            PerfectWindow = perfectWindow;
        }

        public float Duration { get; }

        public float NormalWindow { get; }

        public float PerfectWindow { get; }

        public float Remaining { get; private set; }

        public bool IsActive { get; private set; }

        public void Open()
        {
            Remaining = Duration;
            IsActive = true;
        }

        public void Tick(float deltaTime)
        {
            if (deltaTime < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(deltaTime));
            }

            if (!IsActive)
            {
                return;
            }

            Remaining = Math.Max(0f, Remaining - deltaTime);
            if (Remaining <= 0f)
            {
                IsActive = false;
            }
        }

        public DefenseQteResult Resolve()
        {
            if (!IsActive)
            {
                return DefenseQteResult.Broken;
            }

            DefenseQteResult result = GetCurrentResult();
            IsActive = false;
            Remaining = 0f;
            return result;
        }

        public DefenseQteResult GetCurrentResult()
        {
            if (Remaining <= PerfectWindow)
            {
                return DefenseQteResult.Perfect;
            }

            return Remaining <= NormalWindow
                ? DefenseQteResult.Normal
                : DefenseQteResult.Broken;
        }

        public void Close()
        {
            IsActive = false;
            Remaining = 0f;
        }
    }
}

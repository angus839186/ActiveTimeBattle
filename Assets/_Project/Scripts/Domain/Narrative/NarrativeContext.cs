using System.Collections.Generic;

namespace ActiveTimeBattle.Domain.Narrative
{
    public sealed class NarrativeContext
    {
        private readonly HashSet<string> _flags = new HashSet<string>();

        public bool HasFlag(string flag)
        {
            return !string.IsNullOrWhiteSpace(flag) && _flags.Contains(flag);
        }

        public void SetFlag(string flag)
        {
            if (!string.IsNullOrWhiteSpace(flag))
            {
                _flags.Add(flag);
            }
        }

        public void Clear()
        {
            _flags.Clear();
        }
    }
}

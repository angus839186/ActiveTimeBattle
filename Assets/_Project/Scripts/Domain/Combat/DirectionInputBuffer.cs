using System;
using System.Collections.Generic;

namespace ActiveTimeBattle.Domain.Combat
{
    public sealed class DirectionInputBuffer
    {
        private readonly List<Entry> _entries = new List<Entry>();
        private readonly int _capacity;
        private readonly float _inputLifetime;

        public DirectionInputBuffer(int capacity, float inputLifetime)
        {
            if (capacity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity));
            }

            if (inputLifetime <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(inputLifetime));
            }

            _capacity = capacity;
            _inputLifetime = inputLifetime;
        }

        public int Count => _entries.Count;

        public void Add(CombatDirection direction, float timestamp)
        {
            RemoveExpired(timestamp);
            _entries.Add(new Entry(direction, timestamp));

            while (_entries.Count > _capacity)
            {
                _entries.RemoveAt(0);
            }
        }

        public bool EndsWith(
            IReadOnlyList<CombatDirection> command,
            float timestamp)
        {
            RemoveExpired(timestamp);
            if (command == null || command.Count == 0 || command.Count > _entries.Count)
            {
                return false;
            }

            int offset = _entries.Count - command.Count;
            for (int index = 0; index < command.Count; index++)
            {
                if (_entries[offset + index].Direction != command[index])
                {
                    return false;
                }
            }

            return true;
        }

        public bool IsPrefixOf(
            IReadOnlyList<CombatDirection> command,
            float timestamp)
        {
            RemoveExpired(timestamp);
            if (command == null || _entries.Count > command.Count)
            {
                return false;
            }

            for (int index = 0; index < _entries.Count; index++)
            {
                if (_entries[index].Direction != command[index])
                {
                    return false;
                }
            }

            return true;
        }

        public void Clear()
        {
            _entries.Clear();
        }

        private void RemoveExpired(float timestamp)
        {
            float oldestAllowedTimestamp = timestamp - _inputLifetime;
            _entries.RemoveAll(entry => entry.Timestamp < oldestAllowedTimestamp);
        }

        private readonly struct Entry
        {
            public Entry(CombatDirection direction, float timestamp)
            {
                Direction = direction;
                Timestamp = timestamp;
            }

            public CombatDirection Direction { get; }

            public float Timestamp { get; }
        }
    }
}

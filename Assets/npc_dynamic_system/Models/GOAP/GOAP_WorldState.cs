using System.Collections.Generic;

namespace NPCSystem
{
    /// <summary>
    /// Lightweight world-state representation for the GOAP planner.
    /// Stores named boolean facts (e.g. "EnemyVisible", "InCover").
    ///
    /// This is a plain C# class — NOT a MonoBehaviour.  It is created and
    /// managed entirely by GOAPController at runtime.
    /// </summary>
    public class GOAP_WorldState
    {
        private readonly Dictionary<string, bool> _facts = new();

        // ── Read ───────────────────────────────────────────────────────────────────

        /// <summary>Returns the value of a fact, or false if it is not set.</summary>
        public bool Get(string key) =>
            _facts.TryGetValue(key, out bool v) ? v : false;

        /// <summary>Returns true if the key exists in this world state.</summary>
        public bool Has(string key) => _facts.ContainsKey(key);

        // ── Write ──────────────────────────────────────────────────────────────────

        /// <summary>Sets a fact to the given value.</summary>
        public void Set(string key, bool value) => _facts[key] = value;

        // ── Planning helpers ───────────────────────────────────────────────────────

        /// <summary>
        /// Returns true if every fact in <paramref name="requirements"/> matches
        /// the corresponding value in this world state.
        /// </summary>
        public bool Satisfies(Dictionary<string, bool> requirements)
        {
            foreach (var kv in requirements)
                if (Get(kv.Key) != kv.Value) return false;
            return true;
        }

        /// <summary>Returns a shallow copy of this world state.</summary>
        public GOAP_WorldState Clone()
        {
            var copy = new GOAP_WorldState();
            foreach (var kv in _facts)
                copy._facts[kv.Key] = kv.Value;
            return copy;
        }

        /// <summary>
        /// Applies the given effects dictionary to this world state in-place.
        /// </summary>
        public void Apply(Dictionary<string, bool> effects)
        {
            foreach (var kv in effects)
                _facts[kv.Key] = kv.Value;
        }
    }
}

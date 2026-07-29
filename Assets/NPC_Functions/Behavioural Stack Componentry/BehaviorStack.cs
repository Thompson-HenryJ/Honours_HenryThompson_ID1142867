// ============================================================
// BehaviorStack.cs
// Priority-based behavior arbitration stack.
//
// Models the core arbitration mechanism from Half-Life:
//   "behaviors are checked starting from highest priority;
//    the first ACTIVE behavior is selected for output."
//                                   — Khoo et al. (2002)
//
// Usage
// ──────
//   1. Add one BehaviorStack component per output channel
//      (movement, pitch, fire, jump, etc.)
//   2. Call RegisterBehavior(b) for each behavior in that channel.
//   3. Call Tick() every Update from NPCController.
// ============================================================
using System.Collections.Generic;
using UnityEngine;

namespace HalfLifeNPC
{
    public class BehaviorStack
    {
        // ── Internal state ─────────────────────────────────────────────
        private readonly List<Behavior> _behaviors   = new List<Behavior>();
        private          Behavior       _activeBehavior;

        public string ChannelName { get; }

        public BehaviorStack(string channelName)
        {
            ChannelName = channelName;
        }

        // ──────────────────────────────────────────────────────────────
        // Registration
        // ──────────────────────────────────────────────────────────────

        /// <summary>Add a behavior to this stack, then re-sort by priority.</summary>
        public void RegisterBehavior(Behavior b)
        {
            if (!_behaviors.Contains(b))
            {
                _behaviors.Add(b);
                // Sort ascending: lower Priority number = higher urgency
                _behaviors.Sort((a, b2) => a.Priority.CompareTo(b2.Priority));
            }
        }

        public void UnregisterBehavior(Behavior b)
        {
            _behaviors.Remove(b);
        }

        // ──────────────────────────────────────────────────────────────
        // Arbitration — called every frame
        // ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Evaluate all behaviors top-to-bottom.
        /// First active behavior wins → Execute(); all others are skipped.
        /// </summary>
        public void Tick()
        {
            Behavior winner = null;

            foreach (var b in _behaviors)
            {
                if (b == null || !b.isActiveAndEnabled) continue;

                if (b.IsActive())
                {
                    winner = b;
                    break;              // priority arbitration: stop at first active
                }
            }

            // ── Transition callbacks ──────────────────────────────────
            if (winner != _activeBehavior)
            {
                _activeBehavior?.OnExit();
                winner?.OnEnter();
                _activeBehavior = winner;

#if UNITY_EDITOR
                Debug.Log($"[BehaviorStack:{ChannelName}] " +
                          $"→ {(winner != null ? winner.BehaviorName : "NONE")}");
#endif
            }

            // ── Execute winner ────────────────────────────────────────
            _activeBehavior?.Execute();
        }

        // ──────────────────────────────────────────────────────────────
        // Diagnostics
        // ──────────────────────────────────────────────────────────────

        public Behavior ActiveBehavior => _activeBehavior;

        public IReadOnlyList<Behavior> AllBehaviors => _behaviors;
    }
}

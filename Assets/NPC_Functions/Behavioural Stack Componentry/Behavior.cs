// ============================================================
// Behavior.cs
// Abstract base class for every individual NPC behavior.
//
// Each behavior encapsulates one "job" and exposes:
//   IsActive()  — condition that determines if this behavior
//                 *wants* to run (evaluated by the priority stack)
//   Execute()   — runs the behavior for one frame (called only
//                 when this behavior wins arbitration)
//   Priority    — integer rank; lower = higher priority
//
// Based on the behavior-based arbitration architecture used in
// Half-Life (Khoo et al., 2002).
// ============================================================
using UnityEngine;

namespace HalfLifeNPC
{
    /// <summary>
    /// Abstract base for all NPC behaviors.
    /// Attach multiple Behavior subclasses to an NPC GameObject;
    /// BehaviorStack selects the highest-priority active one each tick.
    /// </summary>
    public abstract class Behavior : MonoBehaviour
    {
        // ── Inspector ──────────────────────────────────────────────────
        [Tooltip("Lower number = higher priority (P1 beats P2)")]
        public int Priority = 10;

        [Tooltip("Human-readable name for debugging")]
        public string BehaviorName = "Unnamed Behavior";

        // ── Shared refs (populated by NPCController) ───────────────────
        [HideInInspector] public NPCBlackboard BB;

        // ──────────────────────────────────────────────────────────────
        // Subclasses MUST implement these two methods
        // ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Return true when this behavior has a valid reason to run.
        /// The BehaviorStack calls this every frame for all registered behaviors.
        /// </summary>
        public abstract bool IsActive();

        /// <summary>
        /// Called once per frame when this behavior wins arbitration.
        /// Write to motor outputs (NavMesh, Animator, etc.) here.
        /// </summary>
        public abstract void Execute();

        // ──────────────────────────────────────────────────────────────
        // Optional lifecycle hooks (override as needed)
        // ──────────────────────────────────────────────────────────────

        /// <summary>Called the first frame this behavior becomes the winner.</summary>
        public virtual void OnEnter()  { }

        /// <summary>Called the first frame this behavior is no longer the winner.</summary>
        public virtual void OnExit()   { }
    }
}

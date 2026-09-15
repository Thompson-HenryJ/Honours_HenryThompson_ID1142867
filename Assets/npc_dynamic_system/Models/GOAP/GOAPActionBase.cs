using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace NPCSystem
{
    /// <summary>
    /// Abstract base for every GOAP action.
    ///
    /// Each concrete action declares:
    ///   • Preconditions  — world-state facts that must be true before planning selects it.
    ///   • Effects        — world-state changes applied by the planner when the action succeeds.
    ///   • CheckContextPreconditions() — runtime checks on live blackboard data (default: true).
    ///
    /// The action lifecycle driven by GOAPController:
    ///   1. OnStart()  — once when the action enters the plan execution queue.
    ///   2. Execute()  — every frame; return true when the action is complete.
    ///   3. OnAbort()  — if the plan is cancelled mid-execution.
    /// </summary>
    [AddComponentMenu("NPC System/Models/GOAP/GOAP Action Base")]
    public abstract class GOAPActionBase : MonoBehaviour
    {
        [Header("Action Identity")]
        [Tooltip("Human-readable name shown in debug logs.")]
        public string ActionName = "Action";

        // ── Planning data ──────────────────────────────────────────────────────────
        // Populated by RegisterPreconditionsAndEffects() in Awake — override in concrete classes.
        public Dictionary<string, bool> Preconditions { get; } = new();
        public Dictionary<string, bool> Effects       { get; } = new();

        // ── Cached component references ────────────────────────────────────────────
        protected NPCBlackboard _blackboard;
        protected NavMeshAgent  _agent;
        protected Animator      _animator;

        protected virtual void Awake()
        {
            _blackboard = GetComponent<NPCBlackboard>();
            _agent      = GetComponent<NavMeshAgent>();
            _animator   = GetComponent<Animator>();
            RegisterPreconditionsAndEffects();
        }

        // ── Abstract / virtual API ─────────────────────────────────────────────────

        /// <summary>
        /// Override to populate Preconditions and Effects dictionaries.
        /// Called automatically from Awake.
        /// </summary>
        protected abstract void RegisterPreconditionsAndEffects();

        /// <summary>
        /// Runtime context check using live blackboard data.
        /// Called by the planner to filter otherwise valid actions.
        /// Default implementation returns true.
        /// </summary>
        public virtual bool CheckContextPreconditions() => true;

        /// <summary>Called once when the plan begins executing this action.</summary>
        public virtual void OnStart() { }

        /// <summary>
        /// Called every frame while this action is active.
        /// Return true when the action has completed its goal.
        /// </summary>
        public abstract bool Execute();

        /// <summary>Called if the current plan is aborted before this action completes.</summary>
        public virtual void OnAbort() { }
    }
}

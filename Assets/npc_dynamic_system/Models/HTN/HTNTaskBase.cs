using UnityEngine;
using UnityEngine.AI;

namespace NPCSystem
{
    /// <summary>
    /// Abstract base for every HTN task.
    ///
    /// In this implementation HTN tasks are "primitive tasks" — single, executable
    /// behaviours selected by the HTNController based on Priority and runtime conditions.
    /// (Compound decomposition is not included; the priority-selection loop serves as a
    /// lightweight approximation of top-level task selection.)
    ///
    /// Task lifecycle:
    ///   1. CheckConditions() — evaluated every frame to determine eligibility.
    ///   2. OnStart()         — called once when the task is first selected.
    ///   3. Execute()         — called every frame; return true when done.
    ///   4. OnAbort()         — called if conditions fail mid-execution.
    /// </summary>
    [AddComponentMenu("NPC System/Models/HTN/HTN Task Base")]
    public abstract class HTNTaskBase : MonoBehaviour
    {
        [Header("Task Configuration")]
        [Tooltip("Human-readable name for debug logs.")]
        public string TaskName = "Task";

        [Tooltip("Lower number = higher urgency. The HTNController selects the applicable task with the lowest Priority each frame.")]
        public int Priority = 10;

        // ── Cached component references ────────────────────────────────────────────
        protected NPCBlackboard _blackboard;
        protected NavMeshAgent  _agent;
        protected Animator      _animator;

        protected virtual void Awake()
        {
            _blackboard = GetComponent<NPCBlackboard>();
            _agent      = GetComponent<NavMeshAgent>();
            _animator   = GetComponent<Animator>();
        }

        // ── Abstract / virtual lifecycle ───────────────────────────────────────────

        /// <summary>
        /// Returns true if the runtime conditions for this task are currently satisfied.
        /// Evaluated every frame by the HTNController.
        /// </summary>
        public abstract bool CheckConditions();

        /// <summary>Called once when the HTNController selects this task.</summary>
        public virtual void OnStart() { }

        /// <summary>
        /// Called every frame while this task is active.
        /// Return true to signal completion (the controller will then re-evaluate all tasks).
        /// </summary>
        public abstract bool Execute();

        /// <summary>Called if CheckConditions() returns false mid-execution.</summary>
        public virtual void OnAbort() { }
    }
}

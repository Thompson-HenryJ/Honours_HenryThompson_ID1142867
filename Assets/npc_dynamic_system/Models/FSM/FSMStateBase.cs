using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace NPCSystem
{
    /// <summary>
    /// Abstract base for every FSM state in the priority-driven Finite State Machine model.
    /// Concrete states inherit from this class and are added as components via the inspector.
    ///
    /// Priority rules: lower number = higher urgency (e.g. Priority 1 preempts Priority 10).
    /// The FSMController evaluates CanEnter() each frame and transitions to the
    /// highest-priority eligible state.
    /// </summary>
    [AddComponentMenu("NPC System/Models/FSM/FSM State Base")]
    public abstract class FSMStateBase : MonoBehaviour
    {
        [Header("State Configuration")]
        [Tooltip("Human-readable name used in debug logs.")]
        public string StateName = "State";

        [Tooltip("Lower number = higher urgency. Evaluated every frame by FSMController.")]
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

        // ── State lifecycle ────────────────────────────────────────────────────────

        /// <summary>Returns true when this state is eligible to become the active state.</summary>
        public abstract bool CanEnter();

        /// <summary>Called once when this state becomes active (after the previous OnExit).</summary>
        public abstract void OnEnter();

        /// <summary>Called every frame while this state is active.</summary>
        public abstract void OnUpdate();

        /// <summary>Called once when a higher-priority state preempts this one.</summary>
        public abstract void OnExit();

        /// <summary>
        /// Optional: allows a state to nominate a specific next state from the
        /// available list. Return null to let FSMController choose by priority.
        /// </summary>
        public virtual FSMStateBase GetNextState(List<FSMStateBase> allStates) => null;
    }
}

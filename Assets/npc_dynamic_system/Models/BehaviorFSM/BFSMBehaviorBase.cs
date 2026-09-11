using UnityEngine;
using UnityEngine.AI;

namespace NPCSystem
{
    /// <summary>
    /// Abstract base for all Behaviour-Based FSM behaviours.
    /// Behaviours live on named priority stacks; the highest-priority active
    /// behaviour wins execution for its stack each frame (Khoo et al. 2002).
    /// </summary>
    public abstract class BFSMBehaviorBase : MonoBehaviour
    {
        [Header("Stack Configuration")]
        [Tooltip("Name of the priority stack this behaviour belongs to (e.g. Movement, Combat, Cover).")]
        public string StackName = "Default";

        [Tooltip("Lower value = higher priority. 1 is the highest possible priority.")]
        public int Priority = 10;

        // ── Protected references ──────────────────────────────────────────────
        protected NPCBlackboard _blackboard;
        protected NavMeshAgent  _agent;
        protected Animator      _animator;

        protected virtual void Awake()
        {
            _blackboard = GetComponent<NPCBlackboard>();
            _agent      = GetComponent<NavMeshAgent>();
            _animator   = GetComponent<Animator>();
        }

        // ── Abstract interface ────────────────────────────────────────────────

        /// <summary>Returns true when this behaviour's activation conditions are met.</summary>
        public abstract bool IsActive();

        /// <summary>Called every frame while this behaviour is the winning behaviour on its stack.</summary>
        public abstract void Execute();

        /// <summary>Called when a higher-priority behaviour takes over this stack.</summary>
        public virtual void OnPreempted() { }

        /// <summary>Called when this behaviour regains the top slot after being preempted.</summary>
        public virtual void OnResumed() { }
    }
}

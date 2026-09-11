using UnityEngine;
using UnityEngine.AI;

namespace NPCSystem
{
    /// <summary>
    /// Abstract base for every BDI desire.
    ///
    /// In this simplified BDI model:
    ///   • Beliefs  — held in NPCBlackboard (shared sensor data).
    ///   • Desires  — each BDIDesireBase subclass represents one candidate intention.
    ///   • Intention — the single desire with the highest utility, subject to hysteresis.
    ///
    /// Desire lifecycle (driven by BDIController):
    ///   1. EvaluateUtility() — returns a [0, 1] score each frame.
    ///   2. CanExecute()      — runtime eligibility gate; must be true for the desire
    ///                          to be considered as intention, regardless of utility.
    ///   3. OnIntend()        — called once when this desire becomes the active intention.
    ///   4. Execute()         — called every frame while intended; return true when done.
    ///   5. IsComplete()      — polled each frame; when true the intention is released.
    /// </summary>
    [AddComponentMenu("NPC System/Models/BDI/BDI Desire Base")]
    public abstract class BDIDesireBase : MonoBehaviour
    {
        [Header("Desire Identity")]
        [Tooltip("Human-readable name for debug logs.")]
        public string DesireName = "Desire";

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

        // ── Abstract / virtual API ─────────────────────────────────────────────────

        /// <summary>Returns a utility score in [0, 1]. Higher = more desirable.</summary>
        public abstract float EvaluateUtility();

        /// <summary>Returns true if this desire is currently executable (runtime gate).</summary>
        public abstract bool CanExecute();

        /// <summary>Called once when this desire is selected as the active intention.</summary>
        public virtual void OnIntend() { }

        /// <summary>
        /// Called every frame while this is the active intention.
        /// Return true when the action is complete.
        /// </summary>
        public abstract bool Execute();

        /// <summary>
        /// Returns true when this intention has been fully realised and should be released.
        /// The controller also releases the intention when Execute() returns true.
        /// </summary>
        public abstract bool IsComplete();
    }
}

using UnityEngine;

namespace NPCSystem
{
    /// <summary>
    /// FSM Idle state — the lowest-priority fallback.
    /// Always eligible (CanEnter returns true), so the NPC always has a safe default.
    /// Stops the NavMeshAgent and plays the idle animation.
    /// </summary>
    [AddComponentMenu("NPC System/Models/FSM/States/FSM Idle State")]
    public class FSM_IdleState : FSMStateBase
    {
        [Header("Idle Settings")]
        [Tooltip("Animator bool parameter to set while idle. Leave blank to skip.")]
        public string IdleAnimParam = "IsIdle";

        protected override void Awake()
        {
            base.Awake();
            StateName = "Idle";
            Priority  = 10;   // Lowest urgency — fallback only
        }

        // ── FSMStateBase ──────────────────────────────────────────────────────────

        public override bool CanEnter() => true;   // Always available as fallback

        public override void OnEnter()
        {
            _agent.ResetPath();
            _agent.isStopped = true;

            if (_animator != null && !string.IsNullOrEmpty(IdleAnimParam))
                _animator.SetBool(IdleAnimParam, true);
        }

        public override void OnUpdate()
        {
            // Nothing to do — just standing still
        }

        public override void OnExit()
        {
            _agent.isStopped = false;

            if (_animator != null && !string.IsNullOrEmpty(IdleAnimParam))
                _animator.SetBool(IdleAnimParam, false);
        }
    }
}

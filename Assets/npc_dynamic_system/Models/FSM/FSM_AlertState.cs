using UnityEngine;

namespace NPCSystem
{
    /// <summary>
    /// FSM Alert state — investigates a heard sound when no enemy is visible.
    /// Eligible when: HearingAlert is true AND enemy is not visible.
    /// On arrival the alert is cleared, allowing the NPC to resume patrolling.
    /// </summary>
    [AddComponentMenu("NPC System/Models/FSM/States/FSM Alert State")]
    public class FSM_AlertState : FSMStateBase
    {
        [Header("Alert Settings")]
        [Tooltip("Distance (metres) at which the NPC considers the investigation point reached.")]
        public float ArrivalDistance = 1.8f;

        [Tooltip("Animator bool parameter to set while on alert. Leave blank to skip.")]
        public string AlertAnimParam = "IsAlert";

        protected override void Awake()
        {
            base.Awake();
            StateName = "Alert";
            Priority  = 6;
        }

        // ── FSMStateBase ──────────────────────────────────────────────────────────

        public override bool CanEnter() =>
            _blackboard.HearingAlert && !_blackboard.EnemyVisible;

        public override void OnEnter()
        {
            _agent.SetDestination(_blackboard.LastKnownEnemyPosition);

            if (_animator != null && !string.IsNullOrEmpty(AlertAnimParam))
                _animator.SetBool(AlertAnimParam, true);
        }

        public override void OnUpdate()
        {
            // Refresh destination in case LastKnownEnemyPosition was updated
            if (!_agent.pathPending &&
                _agent.remainingDistance <= ArrivalDistance)
            {
                // Investigation complete — clear the alert
                _blackboard.HearingAlert = false;
                _agent.ResetPath();
            }
        }

        public override void OnExit()
        {
            if (_animator != null && !string.IsNullOrEmpty(AlertAnimParam))
                _animator.SetBool(AlertAnimParam, false);
        }
    }
}

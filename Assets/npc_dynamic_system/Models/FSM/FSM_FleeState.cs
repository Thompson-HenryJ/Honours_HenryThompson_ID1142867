using UnityEngine;

namespace NPCSystem
{
    /// <summary>
    /// FSM Flee state — evacuates when health is critically low and an enemy is present.
    /// Eligible when: health is below the low-health threshold AND an enemy is detected.
    ///
    /// On enter, a retreat destination is calculated directly behind the NPC relative to
    /// the enemy.  The destination is refreshed whenever the path completes so the NPC
    /// keeps running until it has covered FleeDistance metres.
    /// </summary>
    [AddComponentMenu("NPC System/Models/FSM/States/FSM Flee State")]
    public class FSM_FleeState : FSMStateBase
    {
        [Header("Flee Settings")]
        [Tooltip("Minimum distance (metres) to put between the NPC and the enemy.")]
        public float FleeDistance = 20f;

        [Tooltip("Animator bool parameter to set while fleeing. Leave blank to skip.")]
        public string FleeAnimParam = "IsFleeing";

        protected override void Awake()
        {
            base.Awake();
            StateName = "Flee";
            Priority  = 1;   // Highest urgency
        }

        // ── FSMStateBase ──────────────────────────────────────────────────────────

        public override bool CanEnter() =>
            _blackboard.IsLowHealth && _blackboard.HasEnemy;

        public override void OnEnter()
        {
            SetFleeDestination();

            if (_animator != null && !string.IsNullOrEmpty(FleeAnimParam))
                _animator.SetBool(FleeAnimParam, true);
        }

        public override void OnUpdate()
        {
            // Refresh destination once the current path is complete
            if (!_agent.pathPending && _agent.remainingDistance < 0.5f)
                SetFleeDestination();
        }

        public override void OnExit()
        {
            if (_animator != null && !string.IsNullOrEmpty(FleeAnimParam))
                _animator.SetBool(FleeAnimParam, false);
        }

        // ── Helpers ────────────────────────────────────────────────────────────────
        private void SetFleeDestination()
        {
            Vector3 awayDir;
            if (_blackboard.DetectedEnemy != null)
            {
                awayDir = (transform.position - _blackboard.DetectedEnemy.position).normalized;
            }
            else
            {
                // No enemy reference — flee away from last known position
                awayDir = (transform.position - _blackboard.LastKnownEnemyPosition).normalized;
                if (awayDir == Vector3.zero)
                    awayDir = -transform.forward;
            }

            Vector3 destination = transform.position + awayDir * FleeDistance;
            _agent.SetDestination(destination);
        }
    }
}

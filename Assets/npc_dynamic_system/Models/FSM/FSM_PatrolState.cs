using UnityEngine;

namespace NPCSystem
{
    /// <summary>
    /// FSM Patrol state — loops between a set of waypoints when the NPC has no enemy.
    /// Eligible when: no enemy detected AND at least one waypoint is configured.
    /// </summary>
    [AddComponentMenu("NPC System/Models/FSM/States/FSM Patrol State")]
    public class FSM_PatrolState : FSMStateBase
    {
        [Header("Patrol Settings")]
        [Tooltip("Distance (metres) from a waypoint at which the NPC considers it reached.")]
        public float WaypointArrivalDistance = 1.5f;

        [Tooltip("Seconds to wait at each waypoint before moving to the next.")]
        public float WaypointDwellTime = 1.5f;

        [Tooltip("Animator bool parameter to set while patrolling. Leave blank to skip.")]
        public string WalkAnimParam = "IsWalking";

        // ── Private state ─────────────────────────────────────────────────────────
        private float _dwellTimer;
        private bool  _dwelling;

        protected override void Awake()
        {
            base.Awake();
            StateName = "Patrol";
            Priority  = 8;
        }

        // ── FSMStateBase ──────────────────────────────────────────────────────────

        public override bool CanEnter() =>
            !_blackboard.HasEnemy &&
            _blackboard.PatrolWaypoints != null &&
            _blackboard.PatrolWaypoints.Length > 0;

        public override void OnEnter()
        {
            _dwelling   = false;
            _dwellTimer = 0f;
            MoveToCurrentWaypoint();

            if (_animator != null && !string.IsNullOrEmpty(WalkAnimParam))
                _animator.SetBool(WalkAnimParam, true);
        }

        public override void OnUpdate()
        {
            var waypoints = _blackboard.PatrolWaypoints;
            if (waypoints == null || waypoints.Length == 0) return;

            if (_dwelling)
            {
                _dwellTimer -= Time.deltaTime;
                if (_dwellTimer <= 0f)
                {
                    _dwelling = false;
                    _blackboard.CurrentWaypointIndex =
                        (_blackboard.CurrentWaypointIndex + 1) % waypoints.Length;
                    MoveToCurrentWaypoint();
                }
                return;
            }

            // Check arrival
            if (!_agent.pathPending &&
                _agent.remainingDistance <= WaypointArrivalDistance)
            {
                _dwelling   = true;
                _dwellTimer = WaypointDwellTime;
                _agent.ResetPath();
            }
        }

        public override void OnExit()
        {
            _dwelling = false;

            if (_animator != null && !string.IsNullOrEmpty(WalkAnimParam))
                _animator.SetBool(WalkAnimParam, false);
        }

        // ── Helpers ────────────────────────────────────────────────────────────────
        private void MoveToCurrentWaypoint()
        {
            var wp = _blackboard.PatrolWaypoints[_blackboard.CurrentWaypointIndex];
            if (wp != null)
                _agent.SetDestination(wp.position);
        }
    }
}

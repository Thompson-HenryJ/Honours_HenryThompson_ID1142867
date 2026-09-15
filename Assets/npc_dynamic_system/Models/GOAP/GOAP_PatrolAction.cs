using UnityEngine;

namespace NPCSystem
{
    /// <summary>
    /// GOAP action — patrol between waypoints when no enemy is present.
    ///
    /// Precondition:  Patrolling = true  (i.e. no enemy detected)
    /// Effect:        Patrolling = true  (self-sustaining loop action)
    ///
    /// Execute() always returns false — the patrol is an ongoing loop, not a
    /// one-shot task.  The planner will replace it if a higher-priority goal arises.
    /// </summary>
    [AddComponentMenu("NPC System/Models/GOAP/Actions/GOAP Patrol Action")]
    public class GOAP_PatrolAction : GOAPActionBase
    {
        [Header("Patrol Settings")]
        public float WaypointArrivalDistance = 1.5f;
        public float WaypointDwellTime       = 1.5f;

        private float _dwellTimer;
        private bool  _dwelling;

        protected override void Awake()
        {
            base.Awake();
            ActionName = "Patrol";
        }

        protected override void RegisterPreconditionsAndEffects()
        {
            Preconditions["Patrolling"] = true;
            Effects      ["Patrolling"] = true;
        }

        public override void OnStart()
        {
            _dwelling = false;
            MoveToCurrentWaypoint();
        }

        public override bool Execute()
        {
            var waypoints = _blackboard.PatrolWaypoints;
            if (waypoints == null || waypoints.Length == 0) return false;

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
                return false;
            }

            if (!_agent.pathPending && _agent.remainingDistance <= WaypointArrivalDistance)
            {
                _dwelling   = true;
                _dwellTimer = WaypointDwellTime;
                _agent.ResetPath();
            }

            return false;   // Patrol never self-completes
        }

        public override void OnAbort() => _agent.ResetPath();

        private void MoveToCurrentWaypoint()
        {
            var wp = _blackboard.PatrolWaypoints[_blackboard.CurrentWaypointIndex];
            if (wp != null) _agent.SetDestination(wp.position);
        }
    }
}

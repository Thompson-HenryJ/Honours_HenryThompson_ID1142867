using UnityEngine;

namespace NPCSystem
{
    /// <summary>
    /// HTN Patrol task — loops between waypoints when no enemy is present.
    /// Condition: no enemy detected AND at least one waypoint configured.
    /// Execute() always returns false (continuous loop task).
    /// </summary>
    [AddComponentMenu("NPC System/Models/HTN/Tasks/HTN Patrol Task")]
    public class HTN_PatrolTask : HTNTaskBase
    {
        [Header("Patrol Settings")]
        public float WaypointArrivalDistance = 1.5f;
        public float WaypointDwellTime       = 1.5f;

        private float _dwellTimer;
        private bool  _dwelling;

        protected override void Awake()
        {
            base.Awake();
            TaskName = "Patrol";
            Priority = 10;
        }

        public override bool CheckConditions() =>
            !_blackboard.HasEnemy &&
            _blackboard.PatrolWaypoints != null &&
            _blackboard.PatrolWaypoints.Length > 0;

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

            return false;   // Loop forever
        }

        public override void OnAbort() => _agent.ResetPath();

        private void MoveToCurrentWaypoint()
        {
            var wp = _blackboard.PatrolWaypoints[_blackboard.CurrentWaypointIndex];
            if (wp != null) _agent.SetDestination(wp.position);
        }
    }
}

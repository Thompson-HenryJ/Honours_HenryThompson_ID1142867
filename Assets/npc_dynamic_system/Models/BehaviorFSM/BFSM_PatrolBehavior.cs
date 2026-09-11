using UnityEngine;

namespace NPCSystem
{
    /// <summary>
    /// Behavior-based FSM — Movement stack (Priority 5).
    /// Loops through blackboard patrol waypoints when no enemy is visible.
    /// </summary>
    [AddComponentMenu("NPC System/Models/Behavior FSM/BFSM Patrol Behavior")]
    public class BFSM_PatrolBehavior : BFSMBehaviorBase
    {
        [Header("Patrol Settings")]
        public float WaypointArrivalDistance = 1.5f;
        public float WaypointDwellTime       = 2f;

        private bool  _dwelling;
        private float _dwellTimer;

        protected override void Awake()
        {
            base.Awake();
            StackName = "Movement";
            Priority  = 5;
        }

        public override bool IsActive()
            => !_blackboard.EnemyVisible
               && _blackboard.PatrolWaypoints != null
               && _blackboard.PatrolWaypoints.Length > 0;

        public override void Execute()
        {
            if (_blackboard.PatrolWaypoints == null || _blackboard.PatrolWaypoints.Length == 0) return;

            var target = _blackboard.PatrolWaypoints[_blackboard.CurrentWaypointIndex];
            if (target == null) return;

            // Dwell at waypoint before moving on
            if (_dwelling)
            {
                _dwellTimer -= Time.deltaTime;
                if (_dwellTimer <= 0f) _dwelling = false;
                return;
            }

            float dist = Vector3.Distance(transform.position, target.position);
            if (dist <= WaypointArrivalDistance)
            {
                _blackboard.CurrentWaypointIndex =
                    (_blackboard.CurrentWaypointIndex + 1) % _blackboard.PatrolWaypoints.Length;
                _dwelling    = true;
                _dwellTimer  = WaypointDwellTime;
                _agent.ResetPath();
            }
            else
            {
                _agent.SetDestination(target.position);
            }
        }

        public override void OnPreempted()
        {
            _agent.ResetPath();
            _dwelling = false;
        }
    }
}

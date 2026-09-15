using UnityEngine;

namespace NPCSystem
{
    /// <summary>
    /// BDI Patrol desire — lowest-utility fallback; keeps the NPC mobile when idle.
    ///
    /// Utility: constant 0.1 — always eligible but easily overridden.
    /// CanExecute: PatrolWaypoints are configured.
    /// IsComplete: always false (continuous loop).
    /// </summary>
    [AddComponentMenu("NPC System/Models/BDI/Desires/BDI Patrol Desire")]
    public class BDI_PatrolDesire : BDIDesireBase
    {
        [Header("Patrol Settings")]
        public float WaypointArrivalDistance = 1.5f;
        public float WaypointDwellTime       = 1.5f;

        private float _dwellTimer;
        private bool  _dwelling;

        protected override void Awake()
        {
            base.Awake();
            DesireName = "Patrol";
        }

        public override float EvaluateUtility() => 0.1f;   // Constant low priority

        public override bool CanExecute() =>
            _blackboard.PatrolWaypoints != null && _blackboard.PatrolWaypoints.Length > 0;

        public override void OnIntend()
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

            return false;   // Never self-completes
        }

        public override bool IsComplete() => false;

        private void MoveToCurrentWaypoint()
        {
            var wp = _blackboard.PatrolWaypoints[_blackboard.CurrentWaypointIndex];
            if (wp != null) _agent.SetDestination(wp.position);
        }
    }
}

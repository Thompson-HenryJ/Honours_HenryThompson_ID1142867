using UnityEngine;

namespace NPCSystem
{
    /// <summary>
    /// GOAP action — flee to safety when health is critically low.
    ///
    /// Precondition:  IsLowHealth = true
    /// Effect:        IsLowHealth = false  (models the expectation of escaping danger)
    /// Context check: HasEnemy (only flee if there is actually a pursuer)
    ///
    /// Execute() returns true when the NPC has put SafeDistance metres between itself
    /// and the detected enemy (or last-known position if the reference was lost).
    /// </summary>
    [AddComponentMenu("NPC System/Models/GOAP/Actions/GOAP Flee Action")]
    public class GOAP_FleeAction : GOAPActionBase
    {
        [Header("Flee Settings")]
        [Tooltip("Distance (metres) to run before considering flight successful.")]
        public float SafeDistance = 22f;

        protected override void Awake()
        {
            base.Awake();
            ActionName = "Flee";
        }

        protected override void RegisterPreconditionsAndEffects()
        {
            Preconditions["IsLowHealth"] = true;
            Effects      ["IsLowHealth"] = false;
        }

        public override bool CheckContextPreconditions() => _blackboard.HasEnemy;

        public override void OnStart() => SetFleeDestination();

        public override bool Execute()
        {
            // Refresh path if the agent has stopped
            if (!_agent.pathPending && _agent.remainingDistance < 0.5f)
                SetFleeDestination();

            float dist = _blackboard.DetectedEnemy != null
                ? Vector3.Distance(transform.position, _blackboard.DetectedEnemy.position)
                : Vector3.Distance(transform.position, _blackboard.LastKnownEnemyPosition);

            return dist >= SafeDistance;
        }

        private void SetFleeDestination()
        {
            Vector3 origin = _blackboard.DetectedEnemy != null
                ? _blackboard.DetectedEnemy.position
                : _blackboard.LastKnownEnemyPosition;

            Vector3 awayDir = (transform.position - origin).normalized;
            if (awayDir == Vector3.zero) awayDir = -transform.forward;

            _agent.SetDestination(transform.position + awayDir * SafeDistance);
        }
    }
}

using UnityEngine;

namespace NPCSystem
{
    /// <summary>
    /// BDI Flee desire — evacuates when health is critically low and an enemy is present.
    ///
    /// Utility:     (1 - HealthNormalised) × 0.95  if HasEnemy, else 0.
    ///              i.e. the lower the health, the stronger the urge to flee.
    /// CanExecute:  IsLowHealth AND HasEnemy.
    /// IsComplete:  NOT IsLowHealth OR NOT HasEnemy.
    /// </summary>
    [AddComponentMenu("NPC System/Models/BDI/Desires/BDI Flee Desire")]
    public class BDI_FleeDesire : BDIDesireBase
    {
        [Header("Flee Settings")]
        [Tooltip("Distance (metres) to put between NPC and enemy.")]
        public float FleeDistance = 20f;

        protected override void Awake()
        {
            base.Awake();
            DesireName = "Flee";
        }

        public override float EvaluateUtility() =>
            _blackboard.HasEnemy ? (1f - _blackboard.HealthNormalised) * 0.95f : 0f;

        public override bool CanExecute() =>
            _blackboard.IsLowHealth && _blackboard.HasEnemy;

        public override void OnIntend() => SetFleeDestination();

        public override bool Execute()
        {
            // Refresh destination once the agent reaches the end of its current path
            if (!_agent.pathPending && _agent.remainingDistance < 0.5f)
                SetFleeDestination();

            return IsComplete();
        }

        public override bool IsComplete() =>
            !_blackboard.IsLowHealth || !_blackboard.HasEnemy;

        private void SetFleeDestination()
        {
            Vector3 origin = _blackboard.DetectedEnemy != null
                ? _blackboard.DetectedEnemy.position
                : _blackboard.LastKnownEnemyPosition;

            Vector3 awayDir = (transform.position - origin).normalized;
            if (awayDir == Vector3.zero) awayDir = -transform.forward;

            _agent.SetDestination(transform.position + awayDir * FleeDistance);
        }
    }
}

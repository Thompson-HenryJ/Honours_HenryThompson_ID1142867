using UnityEngine;

namespace NPCSystem
{
    /// <summary>
    /// Behavior-based FSM — Cover stack (Priority 1).
    /// Moves the NPC to the nearest cover point when the enemy is visible
    /// and the NPC is not already in cover.
    /// </summary>
    [AddComponentMenu("NPC System/Models/Behavior FSM/BFSM Duck Behavior")]
    public class BFSM_DuckBehavior : BFSMBehaviorBase
    {
        [Header("Cover Settings")]
        public float CoverArrivalDistance = 1.0f;

        protected override void Awake()
        {
            base.Awake();
            StackName = "Cover";
            Priority  = 1;
        }

        public override bool IsActive()
            => _blackboard.EnemyVisible
               && _blackboard.CoverPoint != null
               && !_blackboard.InCover;

        public override void Execute()
        {
            if (_blackboard.CoverPoint == null) return;

            _agent.SetDestination(_blackboard.CoverPoint.position);

            float dist = Vector3.Distance(transform.position, _blackboard.CoverPoint.position);
            if (dist < CoverArrivalDistance)
                _blackboard.InCover = true;
        }

        public override void OnPreempted() => _agent.ResetPath();
    }
}

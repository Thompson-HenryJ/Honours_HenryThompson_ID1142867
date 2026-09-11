using UnityEngine;

namespace NPCSystem
{
    /// <summary>
    /// Behavior-based FSM — Movement stack (Priority 2).
    /// Investigates the last known enemy position when a hearing alert fires
    /// and health is above the low-health threshold.
    /// </summary>
    [AddComponentMenu("NPC System/Models/Behavior FSM/BFSM Search Behavior")]
    public class BFSM_SearchBehavior : BFSMBehaviorBase
    {
        [Header("Search Settings")]
        public float ArrivalDistance = 1.5f;

        protected override void Awake()
        {
            base.Awake();
            StackName = "Movement";
            Priority  = 2;
        }

        public override bool IsActive()
            => _blackboard.HearingAlert && !_blackboard.EnemyVisible && !_blackboard.IsLowHealth;

        public override void Execute()
        {
            _agent.SetDestination(_blackboard.LastKnownEnemyPosition);

            float dist = Vector3.Distance(transform.position, _blackboard.LastKnownEnemyPosition);
            if (dist < ArrivalDistance)
            {
                _blackboard.HearingAlert = false;
                _agent.ResetPath();
            }
        }

        public override void OnPreempted() => _agent.ResetPath();
    }
}

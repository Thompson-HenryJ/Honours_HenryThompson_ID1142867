using UnityEngine;

namespace NPCSystem
{
    /// <summary>
    /// Behavior-based FSM — Movement stack (Priority 1).
    /// Flees from the detected enemy when health is critically low.
    /// </summary>
    [AddComponentMenu("NPC System/Models/Behavior FSM/BFSM Evade Behavior")]
    public class BFSM_EvadeBehavior : BFSMBehaviorBase
    {
        [Header("Evade Settings")]
        public float FleeDistance = 15f;

        protected override void Awake()
        {
            base.Awake();
            StackName = "Movement";
            Priority  = 1;
        }

        public override bool IsActive() => _blackboard.IsLowHealth && _blackboard.HasEnemy;

        public override void Execute()
        {
            if (_blackboard.DetectedEnemy == null) return;

            Vector3 fleeDir    = (transform.position - _blackboard.DetectedEnemy.position).normalized;
            Vector3 fleeTarget = transform.position + fleeDir * FleeDistance;
            _agent.SetDestination(fleeTarget);
        }

        public override void OnPreempted() => _agent.ResetPath();
    }
}

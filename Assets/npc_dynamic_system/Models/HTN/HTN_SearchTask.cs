using UnityEngine;

namespace NPCSystem
{
    /// <summary>
    /// HTN Search task — investigates a heard sound when no enemy is visible.
    /// Condition: HearingAlert is set AND enemy is not visible.
    /// Execute() returns true on arrival, clearing the alert.
    /// </summary>
    [AddComponentMenu("NPC System/Models/HTN/Tasks/HTN Search Task")]
    public class HTN_SearchTask : HTNTaskBase
    {
        [Header("Search Settings")]
        [Tooltip("Distance (metres) at which the NPC considers the target point reached.")]
        public float ArrivalDistance = 1.8f;

        protected override void Awake()
        {
            base.Awake();
            TaskName = "Search";
            Priority = 5;
        }

        public override bool CheckConditions() =>
            _blackboard.HearingAlert && !_blackboard.EnemyVisible;

        public override void OnStart() =>
            _agent.SetDestination(_blackboard.LastKnownEnemyPosition);

        public override bool Execute()
        {
            if (!_agent.pathPending && _agent.remainingDistance <= ArrivalDistance)
            {
                _blackboard.HearingAlert = false;
                _agent.ResetPath();
                return true;
            }
            return false;
        }

        public override void OnAbort()
        {
            _blackboard.HearingAlert = false;
            _agent.ResetPath();
        }
    }
}

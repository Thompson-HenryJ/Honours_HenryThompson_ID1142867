using UnityEngine;

namespace NPCSystem
{
    /// <summary>
    /// GOAP action — investigate the source of an audible alert.
    ///
    /// Preconditions:  EnemyVisible = false, Patrolling = false
    /// Context check:  HearingAlert = true
    ///
    /// Navigates to LastKnownEnemyPosition.  On arrival (or abort) the HearingAlert
    /// flag is cleared so the planner can reassess the situation.
    ///
    /// Execute() returns true on arrival.
    /// </summary>
    [AddComponentMenu("NPC System/Models/GOAP/Actions/GOAP Investigate Action")]
    public class GOAP_InvestigateAction : GOAPActionBase
    {
        [Header("Investigate Settings")]
        [Tooltip("Distance (metres) at which the NPC considers the target point reached.")]
        public float ArrivalDistance = 1.8f;

        protected override void Awake()
        {
            base.Awake();
            ActionName = "Investigate";
        }

        protected override void RegisterPreconditionsAndEffects()
        {
            Preconditions["EnemyVisible"] = false;
            Preconditions["Patrolling"]   = false;
        }

        public override bool CheckContextPreconditions() => _blackboard.HearingAlert;

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

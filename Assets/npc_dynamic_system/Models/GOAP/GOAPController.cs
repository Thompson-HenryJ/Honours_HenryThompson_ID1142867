using System.Collections.Generic;
using UnityEngine;

namespace NPCSystem
{
    /// <summary>
    /// Goal-Oriented Action Planning (GOAP) controller — greedy forward-search planner.
    ///
    /// DESIGN NOTE: This planner uses a greedy forward-search (not full A*).
    /// It iterates available actions, applies the first that satisfies its preconditions
    /// and context checks against the current world state, and chains actions until the
    /// goal is reached.  This trades plan optimality for runtime simplicity — suitable
    /// for real-time game NPCs where low overhead matters more than globally optimal plans.
    ///
    /// The planner re-evaluates every <see cref="ReplanInterval"/> frames.
    /// The current plan is aborted (OnAbort called on active action) whenever re-planning
    /// yields a different sequence.
    ///
    /// Goal is a single boolean fact (GoalKey must equal GoalValue).
    /// </summary>
    [AddComponentMenu("NPC System/Models/GOAP/GOAP Controller")]
    public class GOAPController : MonoBehaviour, INPCModelController
    {
        [Header("Goal")]
        [Tooltip("Blackboard-derived fact key that represents success (e.g. 'EnemyDefeated').")]
        public string GoalKey   = "EnemyDefeated";

        [Tooltip("The value GoalKey must hold for the goal to be satisfied.")]
        public bool   GoalValue = true;

        [Header("Planning")]
        [Tooltip("Re-plan every N frames. Lower = more responsive; higher = cheaper.")]
        public int ReplanInterval = 30;

        [Header("Debug")]
        public bool VerboseLogging = false;

        // ── Internal state ─────────────────────────────────────────────────────────
        private List<GOAPActionBase>  _actions      = new();
        private List<GOAPActionBase>  _currentPlan  = new();
        private int                   _planIndex;
        private int                   _frameCounter;
        private bool                  _actionStarted;

        // Lazy-initialized blackboard reference
        private NPCBlackboard _blackboard;
        private NPCBlackboard Blackboard => _blackboard ??= GetComponent<NPCBlackboard>();

        // ── INPCModelController ────────────────────────────────────────────────────
        public void InitializeModel()
        {
            RebuildActions();
            _frameCounter  = 0;
            _planIndex     = 0;
            _actionStarted = false;
            _currentPlan.Clear();
        }

        // ── Unity lifecycle ────────────────────────────────────────────────────────
        private void OnEnable() => RebuildActions();

        private void Update()
        {
            _frameCounter++;
            if (_frameCounter >= ReplanInterval)
            {
                _frameCounter = 0;
                Replan();
            }

            ExecuteCurrentAction();
        }

        // ── Planning ───────────────────────────────────────────────────────────────

        /// <summary>
        /// Greedy forward-search planner.  Starts from the current world state and
        /// chains the first applicable action at each step until the goal is met.
        /// </summary>
        private List<GOAPActionBase> BuildPlan()
        {
            var plan        = new List<GOAPActionBase>();
            var worldState  = BuildCurrentWorldState();
            var goal        = new Dictionary<string, bool> { { GoalKey, GoalValue } };

            const int maxSteps = 20;
            for (int step = 0; step < maxSteps; step++)
            {
                if (worldState.Satisfies(goal)) break;

                GOAPActionBase chosen = null;
                foreach (var action in _actions)
                {
                    if (worldState.Satisfies(action.Preconditions) &&
                        action.CheckContextPreconditions())
                    {
                        chosen = action;
                        break;
                    }
                }

                if (chosen == null) break;   // No action can advance the plan

                plan.Add(chosen);
                worldState = worldState.Clone();
                worldState.Apply(chosen.Effects);
            }

            return plan;
        }

        private void Replan()
        {
            var newPlan = BuildPlan();

            // Only abort current plan if the new plan actually differs
            bool planChanged = !PlansEqual(_currentPlan, newPlan);
            if (!planChanged) return;

            // Abort currently-executing action
            if (_planIndex < _currentPlan.Count && _actionStarted)
            {
                _currentPlan[_planIndex].OnAbort();
                _actionStarted = false;
            }

            _currentPlan = newPlan;
            _planIndex   = 0;

            if (VerboseLogging)
            {
                var names = new System.Text.StringBuilder("[GOAPController] New plan: ");
                foreach (var a in _currentPlan) names.Append(a.ActionName).Append(" → ");
                Debug.Log(names.ToString(), this);
            }
        }

        private void ExecuteCurrentAction()
        {
            if (_currentPlan.Count == 0) return;
            if (_planIndex >= _currentPlan.Count) return;

            var action = _currentPlan[_planIndex];

            if (!_actionStarted)
            {
                action.OnStart();
                _actionStarted = true;
            }

            bool complete = action.Execute();
            if (complete)
            {
                _actionStarted = false;
                _planIndex++;

                if (VerboseLogging)
                    Debug.Log($"[GOAPController] Action complete: {action.ActionName}", this);
            }
        }

        // ── World state builder ────────────────────────────────────────────────────

        /// <summary>
        /// Maps live blackboard values to the six canonical world-state facts.
        /// </summary>
        private GOAP_WorldState BuildCurrentWorldState()
        {
            var bb = Blackboard;
            var ws = new GOAP_WorldState();
            ws.Set("EnemyVisible",  bb.EnemyVisible);
            ws.Set("EnemyDefeated", !bb.HasEnemy);
            ws.Set("IsLowHealth",   bb.IsLowHealth);
            ws.Set("InCover",       bb.InCover);
            ws.Set("EnemyInRange",  bb.HasEnemy && bb.DistanceToEnemy <= 20f);
            ws.Set("Patrolling",    !bb.HasEnemy);
            return ws;
        }

        // ── Helpers ────────────────────────────────────────────────────────────────
        private void RebuildActions()
        {
            _actions = new List<GOAPActionBase>(GetComponents<GOAPActionBase>());

            if (VerboseLogging)
                Debug.Log($"[GOAPController] Registered {_actions.Count} actions.", this);
        }

        private static bool PlansEqual(List<GOAPActionBase> a, List<GOAPActionBase> b)
        {
            if (a.Count != b.Count) return false;
            for (int i = 0; i < a.Count; i++)
                if (a[i] != b[i]) return false;
            return true;
        }
    }
}

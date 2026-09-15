using System.Collections.Generic;
using UnityEngine;

namespace NPCSystem
{
    /// <summary>
    /// Priority-driven Finite State Machine controller.
    ///
    /// On each frame the controller iterates the state list (sorted ascending by Priority)
    /// and transitions to the first state whose CanEnter() returns true.  The active state
    /// receives OnUpdate() every frame; transitions call OnExit() → OnEnter().
    ///
    /// Add FSMStateBase-derived components to the same GameObject via the Inspector,
    /// then select FiniteStateMachine in the NPCMasterController dropdown.
    /// </summary>
    [AddComponentMenu("NPC System/Models/FSM/FSM Controller")]
    public class FSMController : MonoBehaviour, INPCModelController
    {
        [Header("Debug")]
        [Tooltip("Log state transitions to the Unity console.")]
        public bool VerboseLogging = false;

        // ── Internal state ─────────────────────────────────────────────────────────
        private readonly List<FSMStateBase> _states = new();
        private FSMStateBase _currentState;

        // ── INPCModelController ────────────────────────────────────────────────────
        public void InitializeModel() => RebuildStates();

        // ── Unity lifecycle ────────────────────────────────────────────────────────
        private void OnEnable()  => RebuildStates();

        private void Update()
        {
            if (_states.Count == 0) return;

            // Find highest-priority (lowest Priority number) eligible state
            FSMStateBase best = null;
            foreach (var s in _states)
            {
                if (s.CanEnter()) { best = s; break; }
            }

            if (best == null) return;

            // Transition if the best candidate differs from the current state
            if (best != _currentState)
            {
                _currentState?.OnExit();
                _currentState = best;
                _currentState.OnEnter();

                if (VerboseLogging)
                    Debug.Log($"[FSMController] → {_currentState.StateName}", this);
            }

            _currentState.OnUpdate();
        }

        // ── Helpers ────────────────────────────────────────────────────────────────
        private void RebuildStates()
        {
            _states.Clear();
            _states.AddRange(GetComponents<FSMStateBase>());
            _states.Sort((a, b) => a.Priority.CompareTo(b.Priority));

            if (VerboseLogging)
                Debug.Log($"[FSMController] Rebuilt {_states.Count} states.", this);
        }
    }
}

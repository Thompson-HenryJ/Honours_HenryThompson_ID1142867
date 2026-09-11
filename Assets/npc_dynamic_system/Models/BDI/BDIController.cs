using System.Collections.Generic;
using UnityEngine;

namespace NPCSystem
{
    /// <summary>
    /// Belief-Desire-Intention (BDI) controller — utility-based intention selector.
    ///
    /// Each frame the controller evaluates all attached BDIDesireBase components and
    /// selects the one with the highest utility as the current intention, subject to:
    ///
    ///   • CanExecute() must return true.
    ///   • Hysteresis: the controller only switches intention if
    ///       bestUtility - currentUtility >= IntentionSwitchThreshold
    ///     This prevents rapid thrashing between desires with similar utility.
    ///
    /// The intention is released when:
    ///   • Execute() returns true, OR
    ///   • IsComplete() returns true, OR
    ///   • CanExecute() returns false on the current intention.
    /// </summary>
    [AddComponentMenu("NPC System/Models/BDI/BDI Controller")]
    public class BDIController : MonoBehaviour, INPCModelController
    {
        [Header("Hysteresis")]
        [Tooltip("Minimum utility advantage required before the controller switches intention. " +
                 "Prevents rapid oscillation between similar-scoring desires.")]
        public float IntentionSwitchThreshold = 0.15f;

        [Header("Debug")]
        public bool VerboseLogging = false;

        // ── Internal state ─────────────────────────────────────────────────────────
        private readonly List<BDIDesireBase> _desires = new();
        private BDIDesireBase _currentIntention;

        // ── INPCModelController ────────────────────────────────────────────────────
        public void InitializeModel() => RebuildDesires();

        // ── Unity lifecycle ────────────────────────────────────────────────────────
        private void OnEnable() => RebuildDesires();

        private void Update()
        {
            if (_desires.Count == 0) return;

            // Release intention if it is complete or no longer executable
            if (_currentIntention != null)
            {
                if (!_currentIntention.CanExecute() || _currentIntention.IsComplete())
                {
                    if (VerboseLogging)
                        Debug.Log($"[BDIController] Releasing intention: {_currentIntention.DesireName}", this);
                    _currentIntention = null;
                }
            }

            // Find the highest-utility executable desire
            BDIDesireBase best      = null;
            float         bestUtil  = -1f;
            foreach (var d in _desires)
            {
                if (!d.CanExecute()) continue;
                float u = d.EvaluateUtility();
                if (u > bestUtil)
                {
                    bestUtil = u;
                    best     = d;
                }
            }

            if (best == null) return;

            // Apply hysteresis before switching
            if (_currentIntention != null && best != _currentIntention)
            {
                float currentUtil = _currentIntention.EvaluateUtility();
                if (bestUtil - currentUtil < IntentionSwitchThreshold)
                    best = _currentIntention;   // Stay with the current intention
            }

            // Commit to new intention
            if (best != _currentIntention)
            {
                _currentIntention = best;
                _currentIntention.OnIntend();

                if (VerboseLogging)
                    Debug.Log($"[BDIController] New intention: {_currentIntention.DesireName} (utility={bestUtil:F2})", this);
            }

            // Execute the active intention
            bool done = _currentIntention.Execute();
            if (done || _currentIntention.IsComplete())
            {
                if (VerboseLogging)
                    Debug.Log($"[BDIController] Intention fulfilled: {_currentIntention.DesireName}", this);
                _currentIntention = null;
            }
        }

        // ── Helpers ────────────────────────────────────────────────────────────────
        private void RebuildDesires()
        {
            _desires.Clear();
            _desires.AddRange(GetComponents<BDIDesireBase>());
            _currentIntention = null;

            if (VerboseLogging)
                Debug.Log($"[BDIController] Registered {_desires.Count} desires.", this);
        }
    }
}

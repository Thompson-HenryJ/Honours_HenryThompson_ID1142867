// ============================================================
// NPCSensors.cs
// Perception layer — writes sensor data into the Blackboard.
// Mirrors "Vision / Hearing / Proximity / Damage" channels
// described in the Half-Life behavior-based architecture.
// ============================================================
using System.Collections.Generic;
using UnityEngine;

namespace HalfLifeNPC
{
    [RequireComponent(typeof(NPCBlackboard))]
    public class NPCSensors : MonoBehaviour
    {
        // ── Inspector settings ─────────────────────────────────────────
        [Header("Vision")]
        [Tooltip("Maximum sight range in metres")]
        public float SightRange      = 20f;
        [Tooltip("Half-angle of field of view cone (degrees)")]
        public float FOVHalfAngle    = 60f;
        [Tooltip("Layer mask for line-of-sight occlusion geometry")]
        public LayerMask ObstacleMask;

        [Header("Hearing")]
        [Tooltip("Radius within which the NPC can 'hear' loud events")]
        public float HearingRadius   = 12f;

        [Header("Cover Detection")]
        public float CoverSearchRadius = 15f;
        public LayerMask CoverMask;

        // ── Runtime refs ───────────────────────────────────────────────
        private NPCBlackboard _bb;
        private Transform     _player;          // cached player transform

        // ──────────────────────────────────────────────────────────────
        void Awake()
        {
            _bb     = GetComponent<NPCBlackboard>();
            // find player by tag; replace with your own player reference
            var playerGO = GameObject.FindGameObjectWithTag("Player");
            if (playerGO != null) _player = playerGO.transform;
        }

        void Update()
        {
            if (_player == null) return;

            UpdateTargetInfo();
            UpdateCoverInfo();
            UpdateAlertDecay();
        }

        // ──────────────────────────────────────────────────────────────
        // Vision + Enemy Info
        // ──────────────────────────────────────────────────────────────
        private void UpdateTargetInfo()
        {
            _bb.EnemyTransform  = _player;
            Vector3 toEnemy     = _player.position - transform.position;
            _bb.DistanceToEnemy = toEnemy.magnitude;

            // Angle check (field-of-view cone)
            float angle = Vector3.Angle(transform.forward, toEnemy.normalized);
            bool  inFOV = angle <= FOVHalfAngle;

            // Raycast line-of-sight check
            bool los = false;
            if (inFOV && _bb.DistanceToEnemy <= SightRange)
            {
                los = !Physics.Raycast(
                    transform.position + Vector3.up * 1.5f,
                    toEnemy.normalized,
                    _bb.DistanceToEnemy,
                    ObstacleMask);
            }

            _bb.HasLineOfSight = los;

            if (los)
            {
                _bb.EnemyIsVisible           = true;
                _bb.LastKnownEnemyPosition   = _player.position;
                _bb.TimeSinceEnemySeen       = 0f;
                _bb.AlertLevel               = Mathf.Min(1f, _bb.AlertLevel + Time.deltaTime * 2f);
            }
            else
            {
                _bb.EnemyIsVisible           = false;
                _bb.TimeSinceEnemySeen      += Time.deltaTime;
            }
        }

        // ──────────────────────────────────────────────────────────────
        // Cover detection — finds nearest valid cover point
        // ──────────────────────────────────────────────────────────────
        private void UpdateCoverInfo()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, CoverSearchRadius, CoverMask);
            float      best = Mathf.Infinity;
            Transform  bestT = null;

            foreach (var h in hits)
            {
                // A cover point is valid only if the enemy can NOT see past it
                Vector3 toPoint = h.transform.position - (_player != null ? _player.position : transform.position);
                if (Physics.Raycast(_player != null ? _player.position : transform.position,
                                    toPoint.normalized, toPoint.magnitude, ObstacleMask))
                {
                    float d = Vector3.Distance(transform.position, h.transform.position);
                    if (d < best) { best = d; bestT = h.transform; }
                }
            }
            _bb.NearestCoverPoint = bestT;
            _bb.IsInCover         = bestT != null &&
                                    Vector3.Distance(transform.position, bestT.position) < 1.2f;
        }

        // ──────────────────────────────────────────────────────────────
        // Alert level naturally decays over time
        // ──────────────────────────────────────────────────────────────
        private void UpdateAlertDecay()
        {
            if (!_bb.EnemyIsVisible)
                _bb.AlertLevel = Mathf.Max(0f, _bb.AlertLevel - Time.deltaTime * 0.2f);
        }

        // ──────────────────────────────────────────────────────────────
        // Called externally when a loud event occurs (gunshot, explosion)
        // ──────────────────────────────────────────────────────────────
        public void OnHearLoudEvent(Vector3 soundOrigin)
        {
            float dist = Vector3.Distance(transform.position, soundOrigin);
            if (dist <= HearingRadius)
            {
                _bb.AlertSignalPosition  = soundOrigin;
                _bb.ReceivedAlertSignal  = true;
                _bb.AlertLevel           = Mathf.Min(1f, _bb.AlertLevel + 0.5f);
            }
        }

        // ──────────────────────────────────────────────────────────────
        // Called by the Damage system when the NPC takes a hit
        // ──────────────────────────────────────────────────────────────
        public void OnTakeDamage(float amount, Vector3 attackerPosition)
        {
            _bb.Health                  = Mathf.Max(0f, _bb.Health - amount);
            _bb.AlertLevel              = 1f;
            _bb.LastKnownEnemyPosition  = attackerPosition;
            _bb.ReceivedAlertSignal     = true;
        }

#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, SightRange);
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, HearingRadius);
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, CoverSearchRadius);

            // FOV cone lines
            Vector3 leftDir  = Quaternion.Euler(0,  FOVHalfAngle, 0) * transform.forward;
            Vector3 rightDir = Quaternion.Euler(0, -FOVHalfAngle, 0) * transform.forward;
            Gizmos.color = Color.white;
            Gizmos.DrawRay(transform.position, leftDir  * SightRange);
            Gizmos.DrawRay(transform.position, rightDir * SightRange);
        }
#endif
    }
}

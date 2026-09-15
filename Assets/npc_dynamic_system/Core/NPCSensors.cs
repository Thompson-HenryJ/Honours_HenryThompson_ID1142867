using UnityEngine;

namespace NPCSystem
{
    [AddComponentMenu("NPC System/Core/NPC Sensors")]
    public class NPCSensors : MonoBehaviour
    {
        [Header("Vision")]
        public float VisionRange  = 15f;
        [Range(0f, 360f)]
        public float VisionAngle  = 90f;
        public LayerMask VisionBlockingLayers;

        [Header("Hearing")]
        public float HearingRange = 20f;

        [Header("Cover Detection")]
        public float    CoverSearchRadius = 8f;
        public LayerMask CoverLayers;

        [Header("Tags")]
        public string EnemyTag = "Player";

        private NPCBlackboard _blackboard;

        private void Awake()
        {
            _blackboard = GetComponent<NPCBlackboard>();
            if (_blackboard == null)
                Debug.LogError("[NPCSensors] NPCBlackboard not found on this GameObject.", this);
        }

        private void Update()
        {
            if (_blackboard == null) return;
            UpdateVision();
            UpdateHearing();
            UpdateCover();
        }

        // ── Vision ──────────────────────────────────────────────────────────────
        private void UpdateVision()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, VisionRange);
            Transform  nearestEnemy = null;
            float      nearestDist  = float.MaxValue;

            foreach (var hit in hits)
            {
                if (!hit.CompareTag(EnemyTag)) continue;

                Vector3 dir   = (hit.transform.position - transform.position).normalized;
                float   angle = Vector3.Angle(transform.forward, dir);
                if (angle > VisionAngle * 0.5f) continue;

                float dist = Vector3.Distance(transform.position, hit.transform.position);

                // Blocked by obstacles?
                if (Physics.Raycast(transform.position + Vector3.up * 0.5f, dir, dist, VisionBlockingLayers))
                    continue;

                if (dist < nearestDist)
                {
                    nearestDist  = dist;
                    nearestEnemy = hit.transform;
                }
            }

            bool visible = nearestEnemy != null;
            _blackboard.EnemyVisible    = visible;
            _blackboard.DetectedEnemy   = nearestEnemy;
            _blackboard.DistanceToEnemy = visible ? nearestDist : float.MaxValue;

            if (visible)
                _blackboard.LastKnownEnemyPosition = nearestEnemy.position;
        }

        // ── Hearing ─────────────────────────────────────────────────────────────
        private void UpdateHearing()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, HearingRange);
            foreach (var hit in hits)
            {
                if (hit.CompareTag(EnemyTag))
                {
                    _blackboard.HearingAlert = true;
                    if (!_blackboard.EnemyVisible)
                        _blackboard.LastKnownEnemyPosition = hit.transform.position;
                    return;
                }
            }
            _blackboard.HearingAlert = false;
        }

        // ── Cover detection ──────────────────────────────────────────────────────
        private void UpdateCover()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, CoverSearchRadius, CoverLayers);
            if (hits.Length == 0)
            {
                _blackboard.CoverPoint = null;
                return;
            }

            Transform closest     = null;
            float     closestDist = float.MaxValue;
            foreach (var hit in hits)
            {
                float d = Vector3.Distance(transform.position, hit.transform.position);
                if (d < closestDist) { closestDist = d; closest = hit.transform; }
            }
            _blackboard.CoverPoint = closest;
        }

        // ── Gizmos ───────────────────────────────────────────────────────────────
        private void OnDrawGizmosSelected()
        {
            // Vision cone
            Gizmos.color = new Color(1f, 1f, 0f, 0.25f);
            Gizmos.DrawWireSphere(transform.position, VisionRange);

            Gizmos.color = Color.yellow;
            Vector3 leftDir  = Quaternion.Euler(0, -VisionAngle * 0.5f, 0) * transform.forward;
            Vector3 rightDir = Quaternion.Euler(0,  VisionAngle * 0.5f, 0) * transform.forward;
            Gizmos.DrawLine(transform.position, transform.position + leftDir  * VisionRange);
            Gizmos.DrawLine(transform.position, transform.position + rightDir * VisionRange);

            // Hearing radius
            Gizmos.color = new Color(0f, 0.5f, 1f, 0.2f);
            Gizmos.DrawWireSphere(transform.position, HearingRange);

            // Cover search radius
            Gizmos.color = new Color(0f, 1f, 0.2f, 0.2f);
            Gizmos.DrawWireSphere(transform.position, CoverSearchRadius);
        }
    }
}

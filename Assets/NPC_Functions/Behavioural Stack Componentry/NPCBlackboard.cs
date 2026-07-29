// ============================================================
// NPCBlackboard.cs
// Shared read/write state store for all NPC behaviors.
// Equivalent to the "Working Memory" in Half-Life's architecture.
// ============================================================
using UnityEngine;

namespace HalfLifeNPC
{
    /// <summary>
    /// Central blackboard (working memory) that every behavior reads from
    /// and sensors write to.  One instance lives on the NPC GameObject.
    /// </summary>
    public class NPCBlackboard : MonoBehaviour
    {
        // ── Enemy / Target ─────────────────────────────────────────────
        [Header("Target Info")]
        public Transform  EnemyTransform;
        public Vector3    LastKnownEnemyPosition;
        public float      DistanceToEnemy    = Mathf.Infinity;
        public bool       HasLineOfSight     = false;
        public bool       EnemyIsVisible     = false;
        public float      TimeSinceEnemySeen = Mathf.Infinity;

        // ── Self State ─────────────────────────────────────────────────
        [Header("Self State")]
        [Range(0, 100)] public float Health       = 100f;
        [Range(0, 100)] public float Ammo         = 30f;
        public bool   WeaponLoaded               = true;
        public bool   IsCrouching                = false;
        public bool   IsInCover                  = false;

        // ── Navigation ─────────────────────────────────────────────────
        [Header("Navigation")]
        public Transform[] Waypoints;
        public int         CurrentWaypointIndex  = 0;
        public bool        HasWaypoint           = false;

        // ── Tactical ───────────────────────────────────────────────────
        [Header("Tactical")]
        public Transform   NearestCoverPoint;
        public bool        AllyNearby            = false;
        public float       AlertLevel            = 0f;   // 0=calm … 1=full alert

        // ── Team Communications ─────────────────────────────────────────
        [Header("Team")]
        public bool        ReceivedAlertSignal   = false;
        public Vector3     AlertSignalPosition;

        // ──────────────────────────────────────────────────────────────
        // Helpers used by behaviors
        // ──────────────────────────────────────────────────────────────

        public bool IsLowHealth     => Health   < 25f;
        public bool IsCriticalAmmo  => Ammo     <= 5f;
        public bool IsEnemyClose    => DistanceToEnemy < 5f;
        public bool IsEnemyInRange  => DistanceToEnemy < 20f;
        public bool IsEnemyLost     => TimeSinceEnemySeen > 5f;
    }
}

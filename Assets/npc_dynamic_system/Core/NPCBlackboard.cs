using System.Collections.Generic;
using UnityEngine;

namespace NPCSystem
{
    [AddComponentMenu("NPC System/Core/NPC Blackboard")]
    public class NPCBlackboard : MonoBehaviour
    {
        [Header("Vital Stats")]
        public float MaxHealth = 100f;
        public float Health = 100f;

        [Header("Enemy Detection")]
        public Transform DetectedEnemy;
        public bool EnemyVisible;
        public float DistanceToEnemy = float.MaxValue;
        public bool HearingAlert;
        public Vector3 LastKnownEnemyPosition;

        [Header("Cover")]
        public bool InCover;
        public Transform CoverPoint;

        [Header("Patrol")]
        public Transform[] PatrolWaypoints;
        public int CurrentWaypointIndex;

        // Generic key-value store for model-specific runtime data
        private readonly Dictionary<string, object> _data = new Dictionary<string, object>();

        // ── Computed properties ──────────────────────────────────────────────
        public float HealthNormalised => MaxHealth > 0f ? Health / MaxHealth : 0f;
        public bool IsLowHealth => HealthNormalised < 0.3f;
        public bool HasEnemy => DetectedEnemy != null;

        // ── Generic accessors ────────────────────────────────────────────────
        public void Set<T>(string key, T value) => _data[key] = value;

        public T Get<T>(string key, T defaultValue = default)
        {
            if (_data.TryGetValue(key, out object val) && val is T typed)
                return typed;
            return defaultValue;
        }

        public bool Has(string key) => _data.ContainsKey(key);
        public void Clear(string key) => _data.Remove(key);
        public void ClearAll() => _data.Clear();
    }
}

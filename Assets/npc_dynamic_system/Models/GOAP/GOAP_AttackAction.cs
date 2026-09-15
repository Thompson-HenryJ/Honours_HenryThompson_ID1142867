using UnityEngine;

namespace NPCSystem
{
    /// <summary>
    /// GOAP action — engage and attempt to defeat a visible, in-range enemy.
    ///
    /// Preconditions:  EnemyInRange = true, IsLowHealth = false
    /// Effect:         EnemyDefeated = true
    /// Context check:  HasEnemy AND EnemyVisible AND NOT IsLowHealth
    ///
    /// Execute() returns true (action complete) when the enemy is gone from the blackboard.
    /// </summary>
    [AddComponentMenu("NPC System/Models/GOAP/Actions/GOAP Attack Action")]
    public class GOAP_AttackAction : GOAPActionBase
    {
        [Header("Combat Settings")]
        public float EngageRange     = 20f;
        public float FireRate        = 1.5f;
        public float ProjectileSpeed = 18f;

        [Tooltip("Projectile prefab (must have a Rigidbody component).")]
        public GameObject ProjectilePrefab;

        [Tooltip("Child Transform from which projectiles spawn.")]
        public Transform FirePoint;

        private float _fireTimer;

        protected override void Awake()
        {
            base.Awake();
            ActionName = "Attack";
        }

        protected override void RegisterPreconditionsAndEffects()
        {
            Preconditions["EnemyInRange"] = true;
            Preconditions["IsLowHealth"]  = false;
            Effects      ["EnemyDefeated"] = true;
        }

        public override bool CheckContextPreconditions() =>
            _blackboard.HasEnemy && _blackboard.EnemyVisible && !_blackboard.IsLowHealth;

        public override void OnStart() => _fireTimer = 0f;

        public override bool Execute()
        {
            if (_blackboard.DetectedEnemy != null)
            {
                // Face enemy
                Vector3 dir = (_blackboard.DetectedEnemy.position - transform.position).normalized;
                dir.y = 0f;
                if (dir != Vector3.zero)
                    transform.rotation = Quaternion.LookRotation(dir);
            }

            _fireTimer -= Time.deltaTime;
            if (_fireTimer <= 0f)
            {
                FireProjectile();
                _fireTimer = 1f / Mathf.Max(FireRate, 0.01f);
            }

            // Complete when enemy is gone
            return !_blackboard.HasEnemy;
        }

        private void FireProjectile()
        {
            /*if (ProjectilePrefab == null || FirePoint == null) return;
            var proj = Object.Instantiate(ProjectilePrefab, FirePoint.position, FirePoint.rotation);
            if (proj.TryGetComponent<Rigidbody>(out var rb))
                rb.linearVelocity = FirePoint.forward * ProjectileSpeed;*/
        }
    }
}

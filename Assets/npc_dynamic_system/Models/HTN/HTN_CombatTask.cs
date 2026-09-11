using UnityEngine;

namespace NPCSystem
{
    /// <summary>
    /// HTN Combat task — continuously fires at a visible, in-range enemy.
    /// Condition: enemy is visible AND within EngageRange.
    /// Execute() returns false (continuous task); completes when conditions fail.
    ///
    /// Assign a ProjectilePrefab (Rigidbody + Collider) and FirePoint child Transform
    /// before entering play mode.
    /// </summary>
    [AddComponentMenu("NPC System/Models/HTN/Tasks/HTN Combat Task")]
    public class HTN_CombatTask : HTNTaskBase
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
            TaskName = "Combat";
            Priority = 2;
        }

        public override bool CheckConditions() =>
            _blackboard.EnemyVisible && _blackboard.DistanceToEnemy <= EngageRange;

        public override void OnStart()
        {
            _fireTimer = 0f;
            _agent.ResetPath();
            _agent.isStopped = true;
        }

        public override bool Execute()
        {
            if (_blackboard.DetectedEnemy != null)
            {
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

            return false;   // Continuous — conditions failure drives abort
        }

        public override void OnAbort() => _agent.isStopped = false;

        private void FireProjectile()
        {
            /*if (ProjectilePrefab == null || FirePoint == null) return;
            var proj = Instantiate(ProjectilePrefab, FirePoint.position, FirePoint.rotation);
            if (proj.TryGetComponent<Rigidbody>(out var rb))
                rb.linearVelocity = FirePoint.forward * ProjectileSpeed;*/
        }
    }
}

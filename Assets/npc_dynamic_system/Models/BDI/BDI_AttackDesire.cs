using UnityEngine;

namespace NPCSystem
{
    /*
    <summary>
    /// BDI Attack desire - engages a visible enemy.
    ///
    /// Utility:     HealthNormalised x (1 - Clamp01(DistanceToEnemy / EngageRange)) x 0.9
    ///              i.e. higher health + closer enemy -> higher urgency to attack.
    /// CanExecute:  HasEnemy AND EnemyVisible.
    /// IsComplete:  no enemy detected.
    ///
    /// Assign a ProjectilePrefab and FirePoint Transform in the Inspector.
    </summary>
    */
    [AddComponentMenu("NPC System/Models/BDI/Desires/BDI Attack Desire")]
    public class BDI_AttackDesire : BDIDesireBase
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
            DesireName = "Attack";
        }

        public override float EvaluateUtility()
        {
            if (!_blackboard.HasEnemy) return 0f;
            float proximityScore = 1f - Mathf.Clamp01(_blackboard.DistanceToEnemy / EngageRange);
            return _blackboard.HealthNormalised * proximityScore * 0.9f;
        }

        public override bool CanExecute() =>
            _blackboard.HasEnemy && _blackboard.EnemyVisible;

        public override void OnIntend()
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

            return false;   // IsComplete() drives termination
        }

        public override bool IsComplete() => !_blackboard.HasEnemy;

        private void FireProjectile()
        {
            /*if (ProjectilePrefab == null || FirePoint == null) return;
            var proj = Instantiate(ProjectilePrefab, FirePoint.position, FirePoint.rotation);
            if (proj.TryGetComponent<Rigidbody>(out var rb))
                rb.linearVelocity = FirePoint.forward * ProjectileSpeed;*/
        }
    }
}

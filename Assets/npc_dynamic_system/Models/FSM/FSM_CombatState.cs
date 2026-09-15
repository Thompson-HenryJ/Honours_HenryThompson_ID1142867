using UnityEngine;

namespace NPCSystem
{
    /// <summary>
    /// FSM Combat state — engages a visible enemy at range.
    /// Eligible when: enemy is visible, within EngageRange, and the NPC is NOT low health.
    ///
    /// The NPC faces the enemy each frame and fires a projectile at FireRate intervals.
    /// Assign a ProjectilePrefab (Rigidbody + Collider) and a FirePoint child Transform
    /// in the inspector before entering play mode.
    /// </summary>
    [AddComponentMenu("NPC System/Models/FSM/States/FSM Combat State")]
    public class FSM_CombatState : FSMStateBase
    {
        [Header("Combat Settings")]
        [Tooltip("Maximum distance at which this NPC will engage an enemy.")]
        public float EngageRange = 20f;

        [Tooltip("Rounds fired per second.")]
        public float FireRate = 1.5f;

        [Tooltip("Projectile prefab (must have a Rigidbody component).")]
        public GameObject ProjectilePrefab;

        [Tooltip("Child Transform from which projectiles are spawned.")]
        public Transform FirePoint;

        [Tooltip("Launch speed of the projectile in metres per second.")]
        public float ProjectileSpeed = 18f;

        [Tooltip("Animator bool parameter to set while in combat. Leave blank to skip.")]
        public string CombatAnimParam = "IsCombat";

        // ── Private state ─────────────────────────────────────────────────────────
        private float _fireTimer;

        protected override void Awake()
        {
            base.Awake();
            StateName = "Combat";
            Priority  = 2;
        }

        // ── FSMStateBase ──────────────────────────────────────────────────────────

        public override bool CanEnter() =>
            _blackboard.EnemyVisible &&
            _blackboard.DistanceToEnemy <= EngageRange &&
            !_blackboard.IsLowHealth;

        public override void OnEnter()
        {
            _agent.ResetPath();
            _agent.isStopped = true;
            _fireTimer = 0f;

            if (_animator != null && !string.IsNullOrEmpty(CombatAnimParam))
                _animator.SetBool(CombatAnimParam, true);
        }

        public override void OnUpdate()
        {
            if (_blackboard.DetectedEnemy == null) return;

            // Face the enemy
            Vector3 dir = (_blackboard.DetectedEnemy.position - transform.position).normalized;
            dir.y = 0f;
            if (dir != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(dir);

            // Fire
            _fireTimer -= Time.deltaTime;
            if (_fireTimer <= 0f)
            {
                FireProjectile();
                _fireTimer = 1f / Mathf.Max(FireRate, 0.01f);
            }
        }

        public override void OnExit()
        {
            _agent.isStopped = false;

            if (_animator != null && !string.IsNullOrEmpty(CombatAnimParam))
                _animator.SetBool(CombatAnimParam, false);
        }

        // ── Helpers ────────────────────────────────────────────────────────────────
        private void FireProjectile()
        {
            /*if (ProjectilePrefab == null || FirePoint == null) return;

            GameObject proj = Instantiate(ProjectilePrefab, FirePoint.position, FirePoint.rotation);
            if (proj.TryGetComponent<Rigidbody>(out var rb))
                rb.linearVelocity = FirePoint.forward * ProjectileSpeed;*/
        }
    }
}

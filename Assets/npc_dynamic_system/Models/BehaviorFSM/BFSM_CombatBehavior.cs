using UnityEngine;

namespace NPCSystem
{
    /// <summary>
    /// Behavior-based FSM — Combat stack (Priority 1).
    /// Faces and fires at a visible enemy while health is above the low-health threshold.
    /// </summary>
    [AddComponentMenu("NPC System/Models/Behavior FSM/BFSM Combat Behavior")]
    public class BFSM_CombatBehavior : BFSMBehaviorBase
    {
        [Header("Combat Settings")]
        public GameObject ProjectilePrefab;
        public Transform  FirePoint;
        public float      ProjectileSpeed = 15f;
        public float      FireRate        = 1f;
        public float      EngageRange     = 20f;

        private float _nextFireTime;

        protected override void Awake()
        {
            base.Awake();
            StackName = "Combat";
            Priority  = 1;
        }

        public override bool IsActive()
            => _blackboard.EnemyVisible
               && _blackboard.DistanceToEnemy <= EngageRange
               && !_blackboard.IsLowHealth;

        public override void Execute()
        {
            if (_blackboard.DetectedEnemy == null) return;

            // Face the enemy (horizontal only)
            Vector3 dir = (_blackboard.DetectedEnemy.position - transform.position);
            dir.y = 0f;
            if (dir != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(dir);

            _agent.ResetPath();

            // Fire at the set rate
            if (Time.time >= _nextFireTime && ProjectilePrefab != null && FirePoint != null)
            {
                /*_nextFireTime = Time.time + 1f / FireRate;
                var proj = Instantiate(ProjectilePrefab, FirePoint.position, FirePoint.rotation);
                if (proj.TryGetComponent<Rigidbody>(out var rb))
                    rb.linearVelocity = FirePoint.forward * ProjectileSpeed;*/
            }
        }

        public override void OnPreempted() { }
    }
}

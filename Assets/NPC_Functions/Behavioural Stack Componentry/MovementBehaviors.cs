// ============================================================
// MovementBehaviors.cs
// Movement actuator channel — three behaviors on one stack:
//
//   P1  EvadeImmediateBehavior   — enemy in melee range  → strafe away
//   P2  TacticalMoveBehavior     — enemy seen/known      → approach / circle-strafe
//   P3  PatrolBehavior           — no threat             → follow waypoints
//
// These map directly to the Groo / Ledgewalker architecture
// (rotate, translate, strafe outputs via NavMeshAgent).
// ============================================================
using UnityEngine;
using UnityEngine.AI;

namespace HalfLifeNPC
{
    // ─────────────────────────────────────────────────────────────────────────
    // P1  Evade Immediate  (highest priority movement behavior)
    // ─────────────────────────────────────────────────────────────────────────
    [DisallowMultipleComponent]
    public class EvadeImmediateBehavior : Behavior
    {
        [Header("Evade Settings")]
        [Tooltip("Distance threshold for triggering evasion")]
        public float MeleeRange       = 4.0f;
        [Tooltip("How far to strafe away per frame")]
        public float StrafeSpeed      = 5.0f;
        public float StrafeDistance   = 8.0f;

        private NavMeshAgent _agent;
        private Animator     _anim;

        void Awake()
        {
            _agent         = GetComponent<NavMeshAgent>();
            _anim          = GetComponent<Animator>();
            BehaviorName   = "EvadeImmediate (P1)";
            Priority       = 1;
        }

        public override bool IsActive()
            => BB != null && BB.EnemyIsVisible && BB.IsEnemyClose;

        public override void OnEnter()
        {
            _anim?.SetBool("IsCrouching", false);
            _anim?.SetTrigger("Strafe");
        }

        public override void Execute()
        {
            if (BB.EnemyTransform == null) return;

            // Strafe perpendicular to the enemy direction
            Vector3 toEnemy  = (BB.EnemyTransform.position - transform.position).normalized;
            Vector3 strafe   = Vector3.Cross(toEnemy, Vector3.up);     // right strafe
            Vector3 target   = transform.position + strafe * StrafeDistance
                               - toEnemy * StrafeDistance;             // also back off

            if (NavMesh.SamplePosition(target, out NavMeshHit hit, 5f, NavMesh.AllAreas))
                _agent.SetDestination(hit.position);

            // Face the enemy while strafing
            transform.rotation = Quaternion.LookRotation(toEnemy, Vector3.up);

            _anim?.SetFloat("Speed", _agent.velocity.magnitude);
        }

        public override void OnExit()
            => _anim?.ResetTrigger("Strafe");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // P2  Tactical Move  (approach / circle-strafe when enemy is known)
    // ─────────────────────────────────────────────────────────────────────────
    [DisallowMultipleComponent]
    public class TacticalMoveBehavior : Behavior
    {
        [Header("Tactical Settings")]
        [Tooltip("Preferred engagement distance")]
        public float DesiredRange        = 10f;
        [Tooltip("Radius of circle-strafe orbit")]
        public float StrafeOrbitRadius   = 6f;
        [Tooltip("Degrees per second of circle-strafing")]
        public float StrafeAngleSpeed    = 45f;

        private NavMeshAgent _agent;
        private Animator     _anim;
        private float        _strafeAngle;

        void Awake()
        {
            _agent       = GetComponent<NavMeshAgent>();
            _anim        = GetComponent<Animator>();
            BehaviorName = "TacticalMove (P2)";
            Priority     = 2;
        }

        public override bool IsActive()
            => BB != null && (BB.EnemyIsVisible || !BB.IsEnemyLost);

        public override void Execute()
        {
            if (BB.EnemyTransform == null) return;

            float dist = BB.DistanceToEnemy;

            if (dist > DesiredRange + 1f)
            {
                // Move closer
                _agent.SetDestination(BB.EnemyTransform.position);
            }
            else if (dist < DesiredRange - 1f)
            {
                // Back off to keep preferred range
                Vector3 awayDir = (transform.position - BB.EnemyTransform.position).normalized;
                Vector3 target  = transform.position + awayDir * 3f;
                if (NavMesh.SamplePosition(target, out NavMeshHit h, 4f, NavMesh.AllAreas))
                    _agent.SetDestination(h.position);
            }
            else
            {
                // Circle-strafe at preferred range
                _strafeAngle += StrafeAngleSpeed * Time.deltaTime;
                Vector3 orbit = new Vector3(
                    Mathf.Sin(_strafeAngle * Mathf.Deg2Rad),
                    0f,
                    Mathf.Cos(_strafeAngle * Mathf.Deg2Rad)) * StrafeOrbitRadius;
                Vector3 orbitPos = BB.EnemyTransform.position + orbit;

                if (NavMesh.SamplePosition(orbitPos, out NavMeshHit hit, 3f, NavMesh.AllAreas))
                    _agent.SetDestination(hit.position);
            }

            // Always face the enemy
            Vector3 look = BB.EnemyTransform.position - transform.position;
            look.y = 0f;
            if (look.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.LookRotation(look);

            _anim?.SetFloat("Speed", _agent.velocity.magnitude);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // P3  Patrol  (lowest priority — default movement when no threat)
    // ─────────────────────────────────────────────────────────────────────────
    [DisallowMultipleComponent]
    public class PatrolBehavior : Behavior
    {
        [Tooltip("How close to a waypoint counts as 'arrived'")]
        public float WaypointStopDistance = 1.5f;
        [Tooltip("Time to idle at each waypoint before moving on")]
        public float IdleTimeAtWaypoint   = 2.0f;

        private NavMeshAgent _agent;
        private Animator     _anim;
        private float        _idleTimer;

        void Awake()
        {
            _agent       = GetComponent<NavMeshAgent>();
            _anim        = GetComponent<Animator>();
            BehaviorName = "Patrol (P3)";
            Priority     = 3;
        }

        public override bool IsActive()
        {
            // Always active as a fallback; valid only when we have waypoints
            return BB != null && BB.Waypoints != null && BB.Waypoints.Length > 0;
        }

        public override void OnEnter()
        {
            BB.HasWaypoint = BB.Waypoints != null && BB.Waypoints.Length > 0;
            if (BB.HasWaypoint) MoveToCurrentWaypoint();
        }

        public override void Execute()
        {
            if (BB.Waypoints == null || BB.Waypoints.Length == 0) return;

            float distToWP = Vector3.Distance(
                transform.position,
                BB.Waypoints[BB.CurrentWaypointIndex].position);

            if (distToWP <= WaypointStopDistance)
            {
                //Currently Disabled due to lack of Animator on NPC
                //_anim?.SetFloat("Speed", 0f);
                _idleTimer += Time.deltaTime;
                if (_idleTimer >= IdleTimeAtWaypoint)
                {
                    _idleTimer = 0f;
                    BB.CurrentWaypointIndex = (BB.CurrentWaypointIndex + 1) % BB.Waypoints.Length;
                    MoveToCurrentWaypoint();
                }
            }
            else
            {
                //Currently Disabled due to lack of Animator on NPC
                // _anim?.SetFloat("Speed", _agent.velocity.magnitude);
            }
        }

        private void MoveToCurrentWaypoint()
        {
            if (BB.Waypoints == null || BB.Waypoints.Length == 0) return;
            _agent.SetDestination(BB.Waypoints[BB.CurrentWaypointIndex].position);
        }
    }
}

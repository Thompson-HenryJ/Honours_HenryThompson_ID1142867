// ============================================================
// UtilityBehaviors.cs
// Independent actuator stacks for utility / body outputs:
//
//   JumpBehavior    — jumps over obstacles or gaps
//   DuckBehavior    — crouches when in cover or under fire
//   UseBehavior     — interacts with doors, buttons, items
//
// Each is an independent stack entry in the Half-Life sense:
// they run concurrently with movement and combat because they
// control entirely different motor outputs.
// ============================================================
using UnityEngine;
using UnityEngine.AI;

namespace HalfLifeNPC
{
    // ─────────────────────────────────────────────────────────────────────────
    // Jump Behavior
    // ─────────────────────────────────────────────────────────────────────────
    public class JumpBehavior : Behavior
    {
        [Header("Jump Settings")]
        public float JumpForce         = 6f;
        public float ObstacleCheckDist = 1.5f;
        public LayerMask GroundMask;

        private Rigidbody _rb;
        private bool      _isGrounded;
        private float     _jumpCooldown;

        void Awake()
        {
            _rb          = GetComponent<Rigidbody>();
            BehaviorName = "Jump";
            Priority     = 1;
        }

        void Update()
        {
            // Ground check via raycast
            _isGrounded  = Physics.Raycast(transform.position, Vector3.down, 1.1f, GroundMask);
            if (_jumpCooldown > 0f) _jumpCooldown -= Time.deltaTime;
        }

        public override bool IsActive()
        {
            if (BB == null || !_isGrounded || _jumpCooldown > 0f) return false;

            // Check for a forward obstacle at nav level
            return Physics.Raycast(
                transform.position + Vector3.up * 0.5f,
                transform.forward,
                ObstacleCheckDist,
                ~GroundMask);
        }

        public override void Execute()
        {
            if (_rb != null)
            {
                _rb.AddForce(Vector3.up * JumpForce, ForceMode.VelocityChange);
                _jumpCooldown = 1.5f;
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Duck / Crouch Behavior — take cover behind low obstacles
    // ─────────────────────────────────────────────────────────────────────────
    public class DuckBehavior : Behavior
    {
        [Header("Duck Settings")]
        public float CrouchHeight       = 0.8f;   // local scale Y when crouching
        public float StandHeight        = 1.8f;
        public float DuckTransitionSpeed= 5f;

        private bool _shouldDuck;

        void Awake() { BehaviorName = "Duck"; Priority = 1; }

        public override bool IsActive()
        {
            if (BB == null) return false;
            // Duck when in cover and enemy has LOS pressure, or when reloading
            _shouldDuck = BB.IsInCover && (BB.EnemyIsVisible || BB.IsCriticalAmmo);
            return _shouldDuck;
        }

        public override void Execute()
        {
            // Smoothly scale to crouch height (simple approach; use Animator in production)
            float targetH = _shouldDuck ? CrouchHeight : StandHeight;
            Vector3 s     = transform.localScale;
            s.y           = Mathf.Lerp(s.y, targetH, DuckTransitionSpeed * Time.deltaTime);
            transform.localScale = s;
            BB.IsCrouching = _shouldDuck;
        }

        public override void OnExit()
        {
            // Snap back to standing
            Vector3 s = transform.localScale; s.y = StandHeight;
            transform.localScale = s;
            BB.IsCrouching = false;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Use / Interact Behavior — opens doors, presses buttons, picks up items
    // ─────────────────────────────────────────────────────────────────────────
    public interface IInteractable
    {
        void Interact(GameObject interactor);
    }

    public class UseBehavior : Behavior
    {
        [Header("Use Settings")]
        public float UseRange       = 1.8f;
        public LayerMask UseMask;

        private IInteractable _target;
        private float         _useCooldown;

        void Awake() { BehaviorName = "Use"; Priority = 2; }

        void Update() { if (_useCooldown > 0f) _useCooldown -= Time.deltaTime; }

        public override bool IsActive()
        {
            if (BB == null || _useCooldown > 0f) return false;

            // Sphere-cast for nearby interactables
            Collider[] hits = Physics.OverlapSphere(transform.position, UseRange, UseMask);
            foreach (var h in hits)
            {
                var interactable = h.GetComponent<IInteractable>();
                if (interactable != null)
                {
                    _target = interactable;
                    return true;
                }
            }
            _target = null;
            return false;
        }

        public override void Execute()
        {
            _target?.Interact(gameObject);
            _useCooldown = 2.0f;
        }
    }
}

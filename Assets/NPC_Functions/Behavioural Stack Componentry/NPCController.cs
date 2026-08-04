// ============================================================
// NPCController.cs
// Master orchestrator for the Half-Life behavior-based FSM.
//
// Wiring mirrors the exact architecture from Khoo et al. (2002):
//
//   MOVEMENT stack  (one shared priority stack)
//     P1  EvadeImmediateBehavior
//     P2  TacticalMoveBehavior
//     P3  PatrolBehavior
//
//   INDEPENDENT actuator stacks (one per output)
//     PITCH          → PitchBehavior
//     SHOOT          → ShootBehavior
//     FIRE_SECONDARY → FireSecondaryBehavior
//     JUMP           → JumpBehavior
//     SWITCH_WEAPON  → SwitchWeaponBehavior
//     RELOAD         → ReloadBehavior
//     DUCK           → DuckBehavior
//     USE            → UseBehavior
//
// All stacks are ticked every frame via Update().
// The NPCBlackboard and NPCSensors must also be on this GameObject.
// ============================================================
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace HalfLifeNPC
{
    [RequireComponent(typeof(NPCBlackboard))]
    [RequireComponent(typeof(NPCSensors))]
    [RequireComponent(typeof(NavMeshAgent))]
    public class NPCController : MonoBehaviour
    {
        // ── Inspector ──────────────────────────────────────────────────
        [Header("Behavior Stack Diagnostics")]
        [Tooltip("Print stack transitions to console")]
        public bool VerboseLogging = false;

        // ── Runtime refs ───────────────────────────────────────────────
        private NPCBlackboard _bb;
        private NPCSensors    _sensors;

        // ── The eleven behavior stacks ─────────────────────────────────
        private BehaviorStack _movementStack;
        private BehaviorStack _pitchStack;
        private BehaviorStack _shootStack;
        private BehaviorStack _fireSecondaryStack;
        private BehaviorStack _jumpStack;
        private BehaviorStack _switchWeaponStack;
        private BehaviorStack _reloadStack;
        private BehaviorStack _duckStack;
        private BehaviorStack _useStack;

        // Keep all stacks in a list for easy ticking
        private readonly List<BehaviorStack> _allStacks = new List<BehaviorStack>();

        // ──────────────────────────────────────────────────────────────
        // Initialisation
        // ──────────────────────────────────────────────────────────────
        void Awake()
        {
            _bb      = GetComponent<NPCBlackboard>();
            _sensors = GetComponent<NPCSensors>();

            BuildStacks();
        }

        private void BuildStacks()
        {
            // ── Movement stack ────────────────────────────────────────
            _movementStack = CreateStack("Movement");
            RegisterInStack<EvadeImmediateBehavior>(_movementStack);
            RegisterInStack<TacticalMoveBehavior>  (_movementStack);
            RegisterInStack<PatrolBehavior>        (_movementStack);

            // ── Individual actuator stacks ────────────────────────────
            _pitchStack         = CreateStack("Pitch");
            RegisterInStack<PitchBehavior>(_pitchStack);

            _shootStack         = CreateStack("Shoot");
            RegisterInStack<ShootBehavior>(_shootStack);

            _fireSecondaryStack = CreateStack("FireSecondary");
            RegisterInStack<FireSecondaryBehavior>(_fireSecondaryStack);

            _jumpStack          = CreateStack("Jump");
            RegisterInStack<JumpBehavior>(_jumpStack);

            _switchWeaponStack  = CreateStack("SwitchWeapon");
            RegisterInStack<SwitchWeaponBehavior>(_switchWeaponStack);

            _reloadStack        = CreateStack("Reload");
            RegisterInStack<ReloadBehavior>(_reloadStack);

            _duckStack          = CreateStack("Duck");
            RegisterInStack<DuckBehavior>(_duckStack);

            _useStack           = CreateStack("Use");
            RegisterInStack<UseBehavior>(_useStack);
        }

        // ──────────────────────────────────────────────────────────────
        // Per-frame tick — all stacks run independently
        // ──────────────────────────────────────────────────────────────
        void Update()
        {
            foreach (var stack in _allStacks)
                stack.Tick();
        }

        // ──────────────────────────────────────────────────────────────
        // Helpers
        // ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Create a named BehaviorStack and add it to the master tick list.
        /// </summary>
        private BehaviorStack CreateStack(string name)
        {
            var stack = new BehaviorStack(name);
            _allStacks.Add(stack);
            return stack;
        }

        /// <summary>
        /// Get or add a Behavior component of type T, inject the blackboard,
        /// and register it on the given stack.
        /// </summary>
        private T RegisterInStack<T>(BehaviorStack stack) where T : Behavior
        {
            var b = GetComponent<T>() ?? gameObject.AddComponent<T>();
            b.BB = _bb;
            stack.RegisterBehavior(b);
            return b;
        }

        // ──────────────────────────────────────────────────────────────
        // Damage forwarding (call from Health system)
        // ──────────────────────────────────────────────────────────────
        public void TakeDamage(float amount, Vector3 attackerPos)
            => _sensors.OnTakeDamage(amount, attackerPos);

        // ──────────────────────────────────────────────────────────────
        // Alert forwarding (call from game event system)
        // ──────────────────────────────────────────────────────────────
        public void ReceiveAlertSignal(Vector3 alertPos)
            => _sensors.OnHearLoudEvent(alertPos);

        // ──────────────────────────────────────────────────────────────
        // Debug GUI — shows which behavior is winning on each stack
        // ──────────────────────────────────────────────────────────────
#if UNITY_EDITOR
        void OnGUI()
        {
            if (!VerboseLogging) return;

            int y = 10;
            GUI.color = Color.cyan;
            GUI.Label(new Rect(10, y, 400, 20), "=== NPC Behavior Stacks ===");
            y += 22;

            GUI.color = Color.white;
            foreach (var stack in _allStacks)
            {
                string active = stack.ActiveBehavior != null
                              ? stack.ActiveBehavior.BehaviorName
                              : "— none —";
                GUI.Label(new Rect(10, y, 450, 20),
                          $"[{stack.ChannelName,-16}]  Active: {active}");
                y += 18;
            }
        }

        void OnDrawGizmosSelected()
        {
            var bb = GetComponent<NPCBlackboard>();
            if (bb == null) return;

            // Last known enemy position
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(bb.LastKnownEnemyPosition, 0.3f);

            // Alert level indicator (vertical bar)
            Gizmos.color = Color.Lerp(Color.green, Color.red, bb.AlertLevel);
            Gizmos.DrawCube(transform.position + Vector3.up * (2.5f + bb.AlertLevel),
                            new Vector3(0.15f, bb.AlertLevel, 0.15f));
        }
#endif
    }
}

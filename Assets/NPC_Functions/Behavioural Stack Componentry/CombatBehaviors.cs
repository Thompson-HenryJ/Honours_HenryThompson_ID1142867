// ============================================================
// CombatBehaviors.cs
// Independent actuator stacks for combat outputs:
//
//   PitchBehavior         — rotates weapon up/down toward target
//   ShootBehavior         — fires primary weapon
//   FireSecondaryBehavior — fires secondary / grenade
//   SwitchWeaponBehavior  — selects optimal weapon for situation
//   ReloadBehavior        — reloads when ammo is low and safe
//
// In the Half-Life architecture each of these is a *separate*
// actuator with its own independent priority stack, allowing
// e.g., pitching and shooting to run concurrently.
// ============================================================
using UnityEngine;

namespace HalfLifeNPC
{
    // ─────────────────────────────────────────────────────────────────────────
    // Pitch Behavior — aims weapon barrel vertically toward target
    // ─────────────────────────────────────────────────────────────────────────
    public class PitchBehavior : Behavior
    {
        [Header("Pitch Settings")]
        public Transform WeaponPivot;          // bone / empty that rotates up-down
        public float     AimSpeed      = 90f;  // degrees per second
        public float     MaxPitchAngle = 45f;

        void Awake() { BehaviorName = "Pitch"; Priority = 1; }

        public override bool IsActive()
            => BB != null && BB.EnemyIsVisible && WeaponPivot != null;

        public override void Execute()
        {
            if (BB.EnemyTransform == null) return;

            Vector3 dirToTarget  = BB.EnemyTransform.position
                                 + Vector3.up * 1.5f        // aim at chest
                                 - WeaponPivot.position;
            Quaternion targetRot = Quaternion.LookRotation(dirToTarget);

            // Clamp pitch
            float pitch = targetRot.eulerAngles.x;
            pitch = pitch > 180f ? pitch - 360f : pitch;
            pitch = Mathf.Clamp(pitch, -MaxPitchAngle, MaxPitchAngle);

            Quaternion clamped = Quaternion.Euler(pitch, WeaponPivot.eulerAngles.y, 0f);
            WeaponPivot.rotation = Quaternion.RotateTowards(
                WeaponPivot.rotation, clamped, AimSpeed * Time.deltaTime);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Shoot Behavior — fires primary weapon when LOS is established
    // ─────────────────────────────────────────────────────────────────────────
    public class ShootBehavior : Behavior
    {
        [Header("Shoot Settings")]
        public float FireRate          = 0.2f;     // seconds between shots
        public float AccuracyRange     = 15f;      // max range for full accuracy
        public int   BurstCount        = 3;        // shots per burst
        public float BurstCooldown     = 1.0f;
        public GameObject ProjectilePrefab;
        public Transform  MuzzlePoint;

        private float _fireTimer;
        private float _burstTimer;
        private int   _shotsFired;

        void Awake() { BehaviorName = "Shoot (Primary)"; Priority = 1; }

        public override bool IsActive()
            => BB != null
            && BB.HasLineOfSight
            && BB.WeaponLoaded
            && BB.Ammo > 0
            && !BB.IsLowHealth;           // don't shoot when fleeing

        public override void Execute()
        {
            _fireTimer  += Time.deltaTime;
            _burstTimer += Time.deltaTime;

            if (_burstTimer < BurstCooldown) return;  // between bursts

            if (_shotsFired >= BurstCount)
            {
                _burstTimer = 0f;
                _shotsFired = 0;
                return;
            }

            if (_fireTimer >= FireRate)
            {
                Fire();
                _fireTimer = 0f;
                _shotsFired++;
            }
        }

        private void Fire()
        {
            if (MuzzlePoint == null || ProjectilePrefab == null) return;

            // Accuracy spread — increases with distance
            float spread = Mathf.Lerp(0f, 4f,
                           BB.DistanceToEnemy / AccuracyRange) * Mathf.Deg2Rad;
            Vector3 dir  = MuzzlePoint.forward;
            dir += new Vector3(
                Random.Range(-spread, spread),
                Random.Range(-spread, spread),
                0f);
            dir.Normalize();

            Instantiate(ProjectilePrefab,
                        MuzzlePoint.position,
                        Quaternion.LookRotation(dir));

            BB.Ammo         = Mathf.Max(0f, BB.Ammo - 1f);
            BB.WeaponLoaded = BB.Ammo > 0;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Fire Secondary Behavior — grenade / alt-fire when enemy is behind cover
    // ─────────────────────────────────────────────────────────────────────────
    public class FireSecondaryBehavior : Behavior
    {
        [Header("Secondary Fire")]
        public float       Cooldown         = 5f;
        public GameObject  GrenadePrefab;
        public Transform   ThrowPoint;
        public float       ThrowForce       = 12f;

        private float _timer;

        void Awake() { BehaviorName = "FireSecondary"; Priority = 2; }

        public override bool IsActive()
            => BB != null
            && BB.IsEnemyInRange
            && !BB.HasLineOfSight          // enemy is behind cover → lob grenade
            && _timer <= 0f;

        public override void Execute()
        {
            if (ThrowPoint == null || GrenadePrefab == null) return;

            Vector3 toTarget = BB.LastKnownEnemyPosition - ThrowPoint.position;
            var go  = Instantiate(GrenadePrefab, ThrowPoint.position, Quaternion.identity);
            var rb  = go.GetComponent<Rigidbody>();
            if (rb != null)
                rb.velocity = toTarget.normalized * ThrowForce + Vector3.up * 5f;

            _timer = Cooldown;
        }

        void Update() { if (_timer > 0f) _timer -= Time.deltaTime; }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Reload Behavior — reload when ammo is critically low and in cover
    // ─────────────────────────────────────────────────────────────────────────
    public class ReloadBehavior : Behavior
    {
        [Header("Reload Settings")]
        public float ReloadDuration  = 2.5f;
        public float MaxAmmo         = 30f;

        private bool  _reloading;
        private float _reloadTimer;

        void Awake() { BehaviorName = "Reload"; Priority = 1; }

        public override bool IsActive()
            => BB != null
            && BB.IsCriticalAmmo
            && (BB.IsInCover || !BB.EnemyIsVisible);   // reload only when safe

        public override void OnEnter()
        {
            _reloading   = true;
            _reloadTimer = 0f;
        }

        public override void Execute()
        {
            _reloadTimer += Time.deltaTime;
            if (_reloadTimer >= ReloadDuration)
            {
                BB.Ammo         = MaxAmmo;
                BB.WeaponLoaded = true;
                _reloading      = false;
            }
        }

        public override void OnExit() => _reloading = false;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Switch Weapon Behavior — change weapon when current is empty
    // ─────────────────────────────────────────────────────────────────────────
    public class SwitchWeaponBehavior : Behavior
    {
        [Header("Weapons")]
        public GameObject[] WeaponObjects;     // array of weapon child GameObjects
        private int _currentWeapon = 0;

        void Awake() { BehaviorName = "SwitchWeapon"; Priority = 2; }

        public override bool IsActive()
            => BB != null && BB.Ammo <= 0 && WeaponObjects != null && WeaponObjects.Length > 1;

        public override void OnEnter()
        {
            _currentWeapon = (_currentWeapon + 1) % WeaponObjects.Length;
            for (int i = 0; i < WeaponObjects.Length; i++)
                if (WeaponObjects[i] != null)
                    WeaponObjects[i].SetActive(i == _currentWeapon);

            // Restore some ammo for the new weapon
            BB.Ammo         = 15f;
            BB.WeaponLoaded = true;
        }

        public override void Execute() { }   // one-shot — OnEnter does all the work
    }
}

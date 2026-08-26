using UnityEngine;
using Game.Core;

namespace Game.Player
{
    public class PlayerWeapon : MonoBehaviour
    {
        [Header("Weapon")]
        [SerializeField] private WeaponConfig config;
        [SerializeField] private Bullet bulletPrefab;
        [SerializeField] private Transform muzzlePoint;

        [Header("Aiming")]
        [SerializeField] private LayerMask enemyLayer;
        [Tooltip("Max angle between barrel and target to allow firing")]
        [Range(0.5f, 15f)] [SerializeField] private float aimToleranceDegrees = 5f;

        private float _fireTimer;

        public IDamageable CurrentTarget { get; private set; }

        private void Update()
        {
            _fireTimer -= Time.deltaTime;
            CurrentTarget = FindNearestEnemy();

            if (_fireTimer > 0f || CurrentTarget == null) return;
            if (!IsAimedAt(CurrentTarget)) return;

            Fire(CurrentTarget);
            _fireTimer = config.fireRate;
        }

        // where the gun visually points: body forward
        private Vector3 AimDirection => transform.forward;

        // ponytail: angle-only gate — enemy colliders are small ground-level triggers a barrel-height ray overflies;
        // bring back a Physics.Raycast gate if enemy colliders ever cover their bodies
        private bool IsAimedAt(IDamageable target)
        {
            Vector3 origin = muzzlePoint != null ? muzzlePoint.position : transform.position;
            Vector3 flatDir = Vector3.ProjectOnPlane(AimDirection, Vector3.up).normalized;

            Vector3 flatToTarget = Vector3.ProjectOnPlane(
                ((MonoBehaviour)target).transform.position - origin, Vector3.up);

            return Vector3.Angle(flatDir, flatToTarget) <= aimToleranceDegrees;
        }

        private void Fire(IDamageable target)
        {
            Vector3 spawnPos = muzzlePoint != null ? muzzlePoint.position : transform.position;
            Bullet bullet = Instantiate(bulletPrefab, spawnPos, Quaternion.LookRotation(AimDirection));
            bullet.Init(((MonoBehaviour)target).transform, target, config.damage, config.bulletSpeed);
        }

        private IDamageable FindNearestEnemy()
        {
            int mask = enemyLayer == 0 ? ~0 : enemyLayer;
            Collider[] hits = Physics.OverlapSphere(transform.position, config.range, mask, QueryTriggerInteraction.Collide);
            IDamageable closest = null;
            float closestDist = float.MaxValue;

            foreach (var hit in hits)
            {
                var damageable = hit.GetComponent<IDamageable>();
                if (damageable == null || !damageable.IsAlive) continue;

                float dist = (hit.transform.position - transform.position).sqrMagnitude;
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = damageable;
                }
            }

            return closest;
        }

        private void OnDrawGizmosSelected()
        {
            if (muzzlePoint == null || config == null) return;

            Vector3 origin = muzzlePoint.position;
            Vector3 dir = Vector3.ProjectOnPlane(AimDirection, Vector3.up).normalized;

            bool firing = Application.isPlaying && CurrentTarget != null && IsAimedAt(CurrentTarget);
            Gizmos.color = firing ? Color.green : Color.red;
            Gizmos.DrawRay(origin, dir * config.range);
        }
    }
}

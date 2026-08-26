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
        private readonly Collider[] _hitBuffer = new Collider[32];
        private ObjectPool<Bullet> _pool;

        public IDamageable CurrentTarget { get; private set; }

        private void Awake()
        {
            _pool = new ObjectPool<Bullet>(bulletPrefab, transform, 4);
        }

        private void Update()
        {
            _fireTimer -= Time.deltaTime;
            CurrentTarget = FindNearestEnemy();

            if (_fireTimer > 0f || CurrentTarget == null) return;
            if (!IsAimedAt(CurrentTarget)) return;

            Fire(CurrentTarget);
            _fireTimer = config.fireRate;
        }

        private Vector3 AimDirection => transform.forward;

        private bool IsAimedAt(IDamageable target)
        {
            if (target is not MonoBehaviour mb) return false;

            Vector3 origin = muzzlePoint != null ? muzzlePoint.position : transform.position;
            Vector3 flatDir = Vector3.ProjectOnPlane(AimDirection, Vector3.up).normalized;

            Vector3 flatToTarget = Vector3.ProjectOnPlane(
                mb.transform.position - origin, Vector3.up);

            return Vector3.Angle(flatDir, flatToTarget) <= aimToleranceDegrees;
        }

        private void Fire(IDamageable target)
        {
            if (target is not MonoBehaviour mb) return;

            Vector3 spawnPos = muzzlePoint != null ? muzzlePoint.position : transform.position;
            Bullet bullet = _pool.Get(spawnPos, Quaternion.LookRotation(AimDirection));
            bullet.Init(mb.transform, target, config.damage, config.bulletSpeed, _pool);
        }

        private IDamageable FindNearestEnemy()
        {
            int mask = enemyLayer == 0 ? ~0 : enemyLayer;
            int count = Physics.OverlapSphereNonAlloc(
                transform.position, config.range, _hitBuffer, mask, QueryTriggerInteraction.Collide);

            IDamageable closest = null;
            float closestDist = float.MaxValue;

            for (int i = 0; i < count; i++)
            {
                var damageable = _hitBuffer[i].GetComponent<IDamageable>();
                if (damageable == null || !damageable.IsAlive) continue;

                float dist = (_hitBuffer[i].transform.position - transform.position).sqrMagnitude;
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

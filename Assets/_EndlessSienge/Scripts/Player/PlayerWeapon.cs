using UnityEngine;
using Game.Core;

namespace Game.Player
{
    public class PlayerWeapon : MonoBehaviour
    {
        [SerializeField] private WeaponConfig config;
        [SerializeField] private Bullet bulletPrefab;
        [SerializeField] private LayerMask enemyLayer;
        [SerializeField] private Transform muzzlePoint;

        private float _fireTimer;
        private ObjectPool<Bullet> _bulletPool;

        private void Awake()
        {
            _bulletPool = new ObjectPool<Bullet>(bulletPrefab, transform);
        }

        private void Update()
        {
            _fireTimer -= Time.deltaTime;
            if (_fireTimer > 0f) return;

            IDamageable nearest = FindNearestEnemy();
            if (nearest != null)
            {
                Fire(nearest);
                _fireTimer = config.fireRate;
            }
        }

        private void Fire(IDamageable target)
        {
            Vector3 spawnPos = muzzlePoint != null ? muzzlePoint.position : transform.position;
            Bullet bullet = _bulletPool.Get(spawnPos, Quaternion.identity);
            bullet.Init(((MonoBehaviour)target).transform, target, config.damage, config.bulletSpeed);
            bullet.transform.SetParent(null);
        }

        private IDamageable FindNearestEnemy()
        {
            int mask = enemyLayer == 0 ? ~0 : enemyLayer;
            Collider[] hits = Physics.OverlapSphere(transform.position, config.range, mask);
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
    }
}

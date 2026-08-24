using UnityEngine;
using Game.Core;

namespace Game.Player
{
    public class PlayerWeapon : MonoBehaviour
    {
        [SerializeField] private WeaponConfig config;
        [SerializeField] private LayerMask enemyLayer;

        private float _fireTimer;

        private void Update()
        {
            _fireTimer -= Time.deltaTime;
            if (_fireTimer > 0f) return;

            IDamageable nearest = FindNearestEnemy();
            if (nearest != null)
            {
                nearest.TakeDamage(config.damage);
                _fireTimer = config.fireRate;
            }
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

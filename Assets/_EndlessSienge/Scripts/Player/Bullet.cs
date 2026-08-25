using UnityEngine;
using Game.Core;

namespace Game.Player
{
    public class Bullet : MonoBehaviour, IPoolable
    {
        private Transform _target;
        private IDamageable _targetDamageable;
        private float _damage;
        private float _speed;

        public void Init(Transform target, IDamageable targetDamageable, float damage, float speed)
        {
            _target = target;
            _targetDamageable = targetDamageable;
            _damage = damage;
            _speed = speed;
        }

        private void Update()
        {
            if (_target == null || !_targetDamageable.IsAlive)
            {
                ReturnToPool();
                return;
            }

            Vector3 direction = _target.position - transform.position;
            float distanceThisFrame = _speed * Time.deltaTime;

            if (direction.magnitude <= distanceThisFrame)
            {
                _targetDamageable.TakeDamage(_damage);
                ReturnToPool();
                return;
            }

            transform.position += direction.normalized * distanceThisFrame;
            transform.LookAt(_target);
        }

        private void ReturnToPool()
        {
            gameObject.SetActive(false);
        }

        public void OnSpawn() { }

        public void OnDespawn()
        {
            _target = null;
            _targetDamageable = null;
        }
    }
}

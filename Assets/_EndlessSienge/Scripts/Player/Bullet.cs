using UnityEngine;
using Game.Core;
using Game.Utils;

namespace Game.Player
{
    public class Bullet : MonoBehaviour, IPoolable
    {
        private Transform _target;
        private IDamageable _targetDamageable;
        private ObjectPool<Bullet> _pool;
        private float _damage;
        private float _speed;

        [SerializeField] private AudioClip hitSfx;

        public void Init(Transform target, IDamageable targetDamageable, float damage, float speed,
            ObjectPool<Bullet> pool)
        {
            _target = target;
            _targetDamageable = targetDamageable;
            _damage = damage;
            _speed = speed;
            _pool = pool;
        }

        public void OnSpawn() { }

        public void OnDespawn()
        {
            _target = null;
            _targetDamageable = null;
        }

        private void Update()
        {
            if (_target == null || !_targetDamageable.IsAlive)
            {
                Release();
                return;
            }

            Vector3 direction = _target.position - transform.position;
            float distanceThisFrame = _speed * Time.deltaTime;

            if (direction.magnitude <= distanceThisFrame)
            {
                _targetDamageable.TakeDamage(_damage);
                SfxPlayer.Play(hitSfx, transform.position);
                Release();
                return;
            }

            transform.position += direction.normalized * distanceThisFrame;
            transform.LookAt(_target);
        }

        private void Release()
        {
            if (_pool != null)
                _pool.Release(this);
            else
                Destroy(gameObject);
        }
    }
}

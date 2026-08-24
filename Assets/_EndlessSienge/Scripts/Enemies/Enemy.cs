using System;
using UnityEngine;
using Game.Core;

namespace Game.Enemies
{
    [RequireComponent(typeof(SphereCollider))]
    public class Enemy : MonoBehaviour, IPoolable, IDamageable
    {
        public event Action<float, float> OnHealthChanged;
        public event Action<Enemy> OnDeath;

        public bool IsAlive => _currentHealth > 0f;

        private EnemyConfig _config;
        private Transform _target;
        private IDamageable _targetDamageable;
        private float _currentHealth;
        private float _attackTimer;
        private GameObject _visualInstance;

        public void Init(EnemyConfig config, Transform target, IDamageable targetDamageable)
        {
            _config = config;
            _target = target;
            _targetDamageable = targetDamageable;

            RebuildVisual();

            _currentHealth = _config.maxHealth;
            _attackTimer = 0f;

            OnHealthChanged?.Invoke(_currentHealth, _config.maxHealth);
        }

        private void Update()
        {
            if (!IsAlive || _target == null) return;

            MoveTowardsTarget();
            TryAttackTarget();
        }

        public void TakeDamage(float amount)
        {
            if (!IsAlive) return;

            _currentHealth = Mathf.Max(0f, _currentHealth - amount);
            OnHealthChanged?.Invoke(_currentHealth, _config.maxHealth);

            if (_currentHealth <= 0f)
                OnDeath?.Invoke(this);
        }

        public void OnSpawn()
        {
            _attackTimer = 0f;
        }

        public void OnDespawn()
        {
            _target = null;
            _targetDamageable = null;

            if (_visualInstance != null)
            {
                Destroy(_visualInstance);
                _visualInstance = null;
            }
        }

        private void MoveTowardsTarget()
        {
            Vector3 toTarget = _target.position - transform.position;
            toTarget.y = 0f;

            if (toTarget.magnitude <= _config.attackRange) return;

            Vector3 direction = toTarget.normalized;
            transform.position += direction * (_config.moveSpeed * Time.deltaTime);

            if (direction.sqrMagnitude > 0.0001f)
                transform.rotation = Quaternion.LookRotation(direction);
        }

        private void TryAttackTarget()
        {
            Vector3 toTarget = _target.position - transform.position;
            toTarget.y = 0f;

            if (toTarget.magnitude > _config.attackRange) return;

            _attackTimer -= Time.deltaTime;
            if (_attackTimer > 0f) return;

            _attackTimer = _config.attackInterval;
            _targetDamageable?.TakeDamage(_config.damage);
        }

        private void RebuildVisual()
        {
            if (_visualInstance != null)
            {
                Destroy(_visualInstance);
                _visualInstance = null;
            }

            if (_config.visualPrefab != null)
            {
                _visualInstance = Instantiate(_config.visualPrefab, transform);
                _visualInstance.transform.localPosition = Vector3.zero;
                _visualInstance.transform.localRotation = Quaternion.identity;
            }
        }
    }
}

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
        public event Action<Enemy> OnDespawned;

        public bool IsAlive => _currentHealth > 0f;

        private EnemyConfig _config;
        private Transform _target;
        private IDamageable _targetDamageable;
        private float _currentHealth;
        private float _attackTimer;
        private GameObject _visualInstance;
        private Rigidbody _rb;

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

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            if (_rb == null) _rb = gameObject.AddComponent<Rigidbody>();
            _rb.useGravity = false;
            _rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;

            if (GetComponent<CapsuleCollider>() == null)
            {
                CapsuleCollider body = gameObject.AddComponent<CapsuleCollider>();
                body.radius = 0.35f;
                body.height = 1.0f;
                body.center = new Vector3(0f, 0.5f, 0f);
            }
        }

        private void Update()
        {
            if (!IsAlive || _target == null) return;

            float distSqr = (_target.position - transform.position).sqrMagnitude;
            if (distSqr > _config.despawnDistance * _config.despawnDistance)
            {
                OnDespawned?.Invoke(this);
                return;
            }

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
            _rb.linearVelocity = Vector3.zero;

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

            if (toTarget.magnitude <= _config.attackRange)
            {
                _rb.linearVelocity = Vector3.zero;
                return;
            }

            Vector3 direction = toTarget.normalized;
            _rb.linearVelocity = direction * _config.moveSpeed;

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

using System;
using UnityEngine;
using Game.Core;

namespace Game.Player
{
    public class PlayerHealth : MonoBehaviour, IDamageable
    {
        [Min(1f)] [SerializeField] private float maxHealth = 100f;

        public event Action<float, float> OnHealthChanged;
        public event Action OnDeath;

        public bool IsAlive => _currentHealth > 0f;

        private float _currentHealth;
        private bool _deathFired;

        private void Awake()
        {
            _currentHealth = maxHealth;
            _deathFired = false;
        }

        private void Start()
        {
            OnHealthChanged?.Invoke(_currentHealth, maxHealth);
        }

        public void TakeDamage(float amount)
        {
            if (!IsAlive) return;

            _currentHealth = Mathf.Max(0f, _currentHealth - amount);
            OnHealthChanged?.Invoke(_currentHealth, maxHealth);

            if (_currentHealth <= 0f && !_deathFired)
            {
                _deathFired = true;
                OnDeath?.Invoke();
            }
        }
    }
}

using UnityEngine;
using UnityEngine.UI;
using Game.Core;

namespace Game.UI
{
    public class HealthBar : MonoBehaviour
    {
        [SerializeField] private Image fill;

        private IDamageable _damageable;

        private void Start()
        {
            _damageable = GetComponentInParent<IDamageable>();
            if (_damageable != null)
                _damageable.OnHealthChanged += SetHealth;
        }

        private void OnDestroy()
        {
            if (_damageable != null)
                _damageable.OnHealthChanged -= SetHealth;
        }

        public void SetHealth(float current, float max)
        {
            fill.fillAmount = max > 0f ? current / max : 0f;
        }
    }
}

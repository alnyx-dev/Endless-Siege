using System;

namespace Game.Core
{
    public interface IDamageable
    {
        bool IsAlive { get; }
        event Action<float, float> OnHealthChanged;
        void TakeDamage(float amount);
    }
}
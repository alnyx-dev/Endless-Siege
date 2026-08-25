using UnityEngine;

namespace Game.Player
{
    [CreateAssetMenu(menuName = "Game/Weapon Config")]
    public class WeaponConfig : ScriptableObject
    {
        [Min(0f)] public float damage = 5f;
        [Min(0.05f)] public float fireRate = 0.5f;
        [Min(1f)] public float range = 15f;
        [Min(1f)] public float bulletSpeed = 20f;
    }
}

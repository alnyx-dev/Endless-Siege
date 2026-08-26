using UnityEngine;

namespace Game.Enemies
{
    [CreateAssetMenu(menuName = "Game/Enemy Config", fileName = "NewEnemyConfig")]
    public class EnemyConfig : ScriptableObject
    {
        public string enemyName = "New Enemy";
        public GameObject visualPrefab;

        [Min(1f)] public float maxHealth = 10f;
        [Min(0f)] public float moveSpeed = 2f;
        [Min(0f)] public float damage = 5f;

        [Min(0.1f)] public float attackRange = 1.2f;
        [Min(0.05f)] public float attackInterval = 1f;

        [Min(5f)] public float despawnDistance = 25f;
    }
}

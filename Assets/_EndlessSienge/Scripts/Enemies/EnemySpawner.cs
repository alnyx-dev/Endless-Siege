using System.Collections.Generic;
using UnityEngine;
using Game.Core;
using Game.Player;

namespace Game.Enemies
{
    public class EnemySpawner : MonoBehaviour
    {
        [Header("Enemy Prefab & Types")]
        [SerializeField] private Enemy enemyPrefab;
        [SerializeField] private EnemyConfig[] enemyConfigs;

        [Header("Player (target for enemies)")]
        [SerializeField] private Transform playerTransform;
        [SerializeField] private PlayerHealth playerHealth;

        [Header("Spawn Settings")]
        [Min(1)] [SerializeField] private int minEnemiesOnField = 5;
        [SerializeField] private float minSpawnRadius = 6f;
        [SerializeField] private float maxSpawnRadius = 10f;

        private IDamageable _playerDamageable;
        private ObjectPool<Enemy> _pool;
        private readonly List<Enemy> _activeEnemies = new List<Enemy>();

        private void Awake()
        {
            _playerDamageable = playerHealth;
            _pool = new ObjectPool<Enemy>(enemyPrefab, transform, minEnemiesOnField);
        }

        private void Update()
        {
            while (_activeEnemies.Count < minEnemiesOnField)
                SpawnEnemy();
        }

        private void SpawnEnemy()
        {
            if (enemyConfigs == null || enemyConfigs.Length == 0) return;

            EnemyConfig config = enemyConfigs[Random.Range(0, enemyConfigs.Length)];
            Vector3 spawnPosition = GetSpawnPosition();

            Enemy enemy = _pool.Get(spawnPosition, Quaternion.identity);
            enemy.Init(config, playerTransform, _playerDamageable);
            enemy.OnDeath += HandleEnemyDeath;

            _activeEnemies.Add(enemy);
        }

        private void HandleEnemyDeath(Enemy enemy)
        {
            enemy.OnDeath -= HandleEnemyDeath;
            _activeEnemies.Remove(enemy);
            _pool.Release(enemy);
        }

        private Vector3 GetSpawnPosition()
        {
            Vector2 randomDir = Random.insideUnitCircle.normalized;
            float distance = Random.Range(minSpawnRadius, maxSpawnRadius);
            Vector3 offset = new Vector3(randomDir.x, 0f, randomDir.y) * distance;

            return (playerTransform != null ? playerTransform.position : Vector3.zero) + offset;
        }

        private void OnDestroy()
        {
            foreach (Enemy enemy in _activeEnemies)
                enemy.OnDeath -= HandleEnemyDeath;
        }
    }
}

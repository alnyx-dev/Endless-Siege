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
            if (_activeEnemies.Count < minEnemiesOnField)
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
            enemy.OnDespawned += HandleEnemyDespawned;

            _activeEnemies.Add(enemy);
        }

        private void HandleEnemyDeath(Enemy enemy) => ReleaseEnemy(enemy);

        private void HandleEnemyDespawned(Enemy enemy) => ReleaseEnemy(enemy);

        private void ReleaseEnemy(Enemy enemy)
        {
            enemy.OnDeath -= HandleEnemyDeath;
            enemy.OnDespawned -= HandleEnemyDespawned;
            _activeEnemies.Remove(enemy);
            _pool.Release(enemy);
        }

        private Vector3 GetSpawnPosition()
        {
            Camera cam = Camera.main;
            Vector3 center = playerTransform != null ? playerTransform.position : Vector3.zero;

            for (int attempt = 0; attempt < 10; attempt++)
            {
                Vector2 randomDir = Random.insideUnitCircle.normalized;
                float distance = Random.Range(minSpawnRadius, maxSpawnRadius);
                Vector3 pos = center + new Vector3(randomDir.x, 0f, randomDir.y) * distance;

                if (cam == null || IsOffScreen(cam, pos))
                    return pos;
            }

            Vector2 fallbackDir = Random.insideUnitCircle.normalized;
            return center + new Vector3(fallbackDir.x, 0f, fallbackDir.y) * maxSpawnRadius;
        }

        private static bool IsOffScreen(Camera cam, Vector3 worldPos)
        {
            Vector3 vp = cam.WorldToViewportPoint(worldPos);
            return vp.x < 0f || vp.x > 1f || vp.y < 0f || vp.y > 1f;
        }

        private void OnDestroy()
        {
            var snapshot = new List<Enemy>(_activeEnemies);
            foreach (Enemy enemy in snapshot)
            {
                enemy.OnDeath -= HandleEnemyDeath;
                enemy.OnDespawned -= HandleEnemyDespawned;
            }
        }
    }
}

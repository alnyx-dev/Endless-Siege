using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Game.Player;

namespace Game.Gameplay
{
    public class GameState : MonoBehaviour
    {
        public const string BestTimeKey = "BestTime";

        [SerializeField] private PlayerHealth playerHealth;

        public float ElapsedTime { get; private set; }
        public bool IsRunning { get; private set; }

        public event Action OnDeath;
        public event Action<float> OnTimeUpdated;

        private void Awake()
        {
            if (playerHealth == null) playerHealth = FindFirstObjectByType<PlayerHealth>();
            if (playerHealth == null)
            {
                Debug.LogError("GameState: no PlayerHealth found in scene", this);
                enabled = false;
                return;
            }

            playerHealth.OnDeath += HandleDeath;
            IsRunning = true;
        }

        private void Update()
        {
            if (!IsRunning) return;

            ElapsedTime += Time.deltaTime;
            OnTimeUpdated?.Invoke(ElapsedTime);
        }

        private void OnDestroy()
        {
            if (playerHealth != null) playerHealth.OnDeath -= HandleDeath;
        }

        private void HandleDeath()
        {
            IsRunning = false;

            float best = PlayerPrefs.GetFloat(BestTimeKey, 0f);
            if (ElapsedTime > best)
                PlayerPrefs.SetFloat(BestTimeKey, ElapsedTime);
            PlayerPrefs.Save();

            Time.timeScale = 0f;

            OnDeath?.Invoke();
        }

        public void RequestMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("MenuScene");
        }
    }
}

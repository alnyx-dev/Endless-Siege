using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Gameplay
{
    public class MenuState : MonoBehaviour
    {
        private const string GameSceneName = "GameScene";

        public void StartGame()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(GameSceneName);
        }
    }
}

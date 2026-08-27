using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Game.Gameplay;

namespace Game.UI
{
    public class SurvivalUI : MonoBehaviour
    {
        [SerializeField] private GameState state;
        [SerializeField] private TMP_Text timerLabel;
        [SerializeField] private GameObject gameOverPanel;
        private const string MenuSceneName = "MenuScene";

        private void Awake()
        {
            if (state == null) state = FindFirstObjectByType<GameState>();
            if (state == null)
            {
                Debug.LogError("SurvivalUI: no GameState found in scene", this);
                enabled = false;
                return;
            }

            state.OnTimeUpdated += UpdateTimer;
            state.OnDeath += ShowGameOver;

            if (gameOverPanel != null)
                gameOverPanel.SetActive(false);
        }

        private void OnDestroy()
        {
            if (state == null) return;
            state.OnTimeUpdated -= UpdateTimer;
            state.OnDeath -= ShowGameOver;
        }

        private void UpdateTimer(float elapsed)
        {
            timerLabel.text = RunUi.Format(elapsed);
        }

        private void ShowGameOver()
        {
            if (gameOverPanel != null)
                gameOverPanel.SetActive(true);
        }
    }

    public static class RunUi
    {
        public static string Format(float seconds)
        {
            int t = Mathf.FloorToInt(seconds);
            return $"{t / 60}:{t % 60:D2}";
        }

        public static Transform CanvasRoot()
        {
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas != null) return canvas.transform;

            canvas = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster))
                .GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            return canvas.transform;
        }

        public static TMP_Text CreateText(Transform parent, string name, int fontSize, Vector2 anchor, Vector2 position)
        {
            GameObject go = new GameObject(name, typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);

            TMP_Text text = go.GetComponent<TextMeshProUGUI>();
            text.fontSize = fontSize;
            text.alignment = TextAlignmentOptions.Center;
            text.raycastTarget = false;

            RectTransform rect = text.rectTransform;
            rect.anchorMin = rect.anchorMax = anchor;
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(600f, fontSize * 1.4f);
            return text;
        }
    }
}

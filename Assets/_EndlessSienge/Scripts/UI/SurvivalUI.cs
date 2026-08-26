using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Game.Gameplay;

namespace Game.UI
{
    public class SurvivalUI : MonoBehaviour
    {
        [SerializeField] private GameState state;
        [Tooltip("Optional: wire your own label, otherwise one is created under the scene Canvas")]
        [SerializeField] private TMP_Text timerLabel;
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

            if (timerLabel == null)
                timerLabel = RunUi.CreateText(
                    RunUi.CanvasRoot(), "Survival Timer", 40,
                    new Vector2(0.5f, 1f), new Vector2(0f, -120f));
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
            CreateGameOverPanel();
        }

        private void CreateGameOverPanel()
        {
            Transform canvas = RunUi.CanvasRoot();

            RectTransform panel =
                new GameObject("Game Over Panel", typeof(Image)).GetComponent<RectTransform>();
            panel.SetParent(canvas, false);
            panel.anchorMin = Vector2.zero;
            panel.anchorMax = Vector2.one;
            panel.offsetMin = Vector2.zero;
            panel.offsetMax = Vector2.zero;
            panel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.65f);

            TMP_Text title = RunUi.CreateText(panel, "Game Over Title", 72,
                new Vector2(0.5f, 0.5f), new Vector2(0f, 80f));
            title.text = "Game Over";

            Button menuButton = CreateButton(panel, "Menu Button", "Menu", new Vector2(0f, -60f));
            menuButton.onClick.AddListener(LoadMenu);
        }

        private static Button CreateButton(RectTransform parent, string name, string label, Vector2 position)
        {
            RectTransform rect =
                new GameObject(name, typeof(Image), typeof(Button)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(240f, 70f);
            rect.GetComponent<Image>().color = new Color(0.25f, 0.25f, 0.25f);

            TMP_Text text = RunUi.CreateText(rect, "Label", 32,
                new Vector2(0.5f, 0.5f), Vector2.zero);
            text.rectTransform.sizeDelta = rect.sizeDelta;
            text.text = label;

            return rect.GetComponent<Button>();
        }

        private void LoadMenu()
        {
            state.RequestMenu();
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

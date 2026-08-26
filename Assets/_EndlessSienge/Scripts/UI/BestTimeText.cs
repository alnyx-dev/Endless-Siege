using TMPro;
using UnityEngine;
using Game.Gameplay;

namespace Game.UI
{
    public class BestTimeText : MonoBehaviour
    {
        [Tooltip("Optional: wire your own label, otherwise one is created under the scene Canvas")]
        [SerializeField] private TMP_Text label;

        private void Start()
        {
            if (label == null)
                label = RunUi.CreateText(
                    RunUi.CanvasRoot(), "Best Time", 40,
                    new Vector2(0.5f, 1f), new Vector2(0f, -60f));

            float best = PlayerPrefs.GetFloat(GameState.BestTimeKey, 0f);
            label.text = best > 0f ? $"Record: {RunUi.Format(best)}" : "Record: --";
        }
    }
}

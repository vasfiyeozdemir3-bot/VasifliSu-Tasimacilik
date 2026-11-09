using UnityEngine;
using UnityEngine.UI;

namespace VasifliSu
{
    /// <summary>Oynanış arayüzü: hız, kalan su, yol ilerlemesi, süre, geri sayım.</summary>
    public class HUDController : MonoBehaviour
    {
        public LevelManager level;
        public Text levelNameText;
        public Text speedText;
        public Text waterText;
        public Image waterFill;
        public RectTransform requiredMarker;
        public Image progressFill;
        public RectTransform progressTruck;
        public Text distanceText;
        public Text timerText;
        public Text countdownText;
        public Button pauseButton;

        float _shownWater = 1f;

        void Start()
        {
            levelNameText.text = (level.levelIndex + 1) + Loc.T(". BÖLÜM  •  ") + Loc.T(level.Data.levelName).ToUpper();
            pauseButton.onClick.AddListener(level.TogglePause);
            if (requiredMarker != null)
            {
                float r = level.Data.requiredRatio;
                requiredMarker.anchorMin = new Vector2(r, 0f);
                requiredMarker.anchorMax = new Vector2(r, 1f);
                requiredMarker.anchoredPosition = Vector2.zero;
            }
        }

        void Update()
        {
            var truck = level.truck;
            var cargo = level.cargo;
            var terrain = level.terrain;

            speedText.text = Mathf.RoundToInt(truck.SpeedKmh) + "<size=26> " + Loc.T("km/s") + "</size>";

            float ratio = cargo.RemainingRatio;
            _shownWater = Mathf.Lerp(_shownWater, ratio, Time.unscaledDeltaTime * 6f);
            waterFill.fillAmount = _shownWater;
            waterFill.color = ratio < level.Data.requiredRatio + 0.15f ? Brand.Orange : Brand.Aqua;
            waterText.text = cargo.RemainingLitres + " / " + cargo.TotalLitres + " L";

            float start = terrain.SpawnX, end = terrain.FinishX;
            float p = Mathf.Clamp01((truck.transform.position.x - start) / (end - start));
            progressFill.fillAmount = p;
            if (progressTruck != null)
            {
                progressTruck.anchorMin = progressTruck.anchorMax = new Vector2(p, 0.5f);
                progressTruck.anchoredPosition = Vector2.zero;
            }
            distanceText.text = Mathf.Max(0, Mathf.RoundToInt(end - truck.transform.position.x)) + " m";

            float t = level.ElapsedTime;
            timerText.text = string.Format("{0}:{1:00}", (int)(t / 60), (int)(t % 60));
        }

        public void ShowCountdown(string s)
        {
            countdownText.gameObject.SetActive(true);
            countdownText.text = s;
            countdownText.transform.localScale = Vector3.one * 1.4f;
        }

        public void HideCountdown() => countdownText.gameObject.SetActive(false);

        void LateUpdate()
        {
            if (countdownText.gameObject.activeSelf)
                countdownText.transform.localScale = Vector3.Lerp(countdownText.transform.localScale, Vector3.one, Time.unscaledDeltaTime * 8f);
        }
    }
}

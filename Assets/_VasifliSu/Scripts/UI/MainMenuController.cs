using UnityEngine;
using UnityEngine.UI;

namespace VasifliSu
{
    /// <summary>Ana menü: Oyna, Ayarlar, GDD, Çıkış + dağıtıcı rütbesi (oyunlaştırma).</summary>
    public class MainMenuController : MonoBehaviour
    {
        public Button playButton, settingsButton, gddButton, quitButton;
        public SettingsPanel settings;
        public GddPanel gdd;
        public RectTransform logo;
        public Text rankText, statsText;
        public Image rankFill;

        Vector2 _logoPos;

        void Start()
        {
            Time.timeScale = 1f;
            AudioManager.I.PlayMusic();
            playButton.onClick.AddListener(SceneFlow.LoadLevelSelect);
            settingsButton.onClick.AddListener(() => settings.Show(true));
            gddButton.onClick.AddListener(() => gdd.Show(true));
            quitButton.onClick.AddListener(Quit);
#if UNITY_WEBGL && !UNITY_EDITOR
            quitButton.gameObject.SetActive(false);
#endif
            if (logo != null) _logoPos = logo.anchoredPosition;
            RefreshStats();
            Loc.Changed += RefreshStats;
        }

        void OnDestroy() { Loc.Changed -= RefreshStats; }

        public void RefreshStats()
        {
            string rank = SaveSystem.RankName(out float prog, out int next);
            rankText.text = Loc.T("Rütbe: ") + "<color=#FF9F1C>" + rank + "</color>";
            rankFill.fillAmount = prog;
            statsText.text = Loc.T("Toplam teslim: ") + SaveSystem.TotalLitres + " L   •   ★ " + SaveSystem.TotalStars() + " / 15"
                             + (prog < 1f ? "   •   " + Loc.T("Sonraki rütbe: ") + next + " L" : "");
        }

        void Update()
        {
            if (logo != null) logo.anchoredPosition = _logoPos + new Vector2(0f, Mathf.Sin(Time.unscaledTime * 1.6f) * 8f);
        }

        void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}

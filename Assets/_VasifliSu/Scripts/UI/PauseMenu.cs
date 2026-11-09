using UnityEngine;
using UnityEngine.UI;

namespace VasifliSu
{
    /// <summary>
    /// Duraklatma menüsü: bölüm bilgisi, anlık teslimat durumu (su, süre, mesafe),
    /// hareketli kamyon ve su damlaları, değişen ipuçları, ses ayarları ve gezinme butonları.
    /// </summary>
    public class PauseMenu : MonoBehaviour
    {
        public LevelManager level;
        public GameObject root;
        public Button resumeButton, restartButton, levelSelectButton, mainMenuButton;
        public Slider musicSlider, sfxSlider;
        public Text levelText, waterText, timeText, distanceText, tipText;
        public Image waterFill;
        public RectTransform truck;
        public RectTransform panel;
        public RectTransform[] drops;

        static readonly string[] Tips =
        {
            "Yokuş aşağı inerken frene bas, damacanalar kaymasın.",
            "Havadayken gaz burnu kaldırır, fren indirir.",
            "Yumuşak gaz = daha az dökülen su!",
            "Hedef sürede bitirirsen 3. yıldız senin.",
            "Her teslim edilen litre rütbeni yükseltir.",
        };

        Vector2[] _dropStart;
        Vector2 _truckPos;
        float _open;
        int _tip;
        float _tipTimer;

        void Start()
        {
            resumeButton.onClick.AddListener(level.TogglePause);
            restartButton.onClick.AddListener(level.Restart);
            levelSelectButton.onClick.AddListener(level.GoLevelSelect);
            mainMenuButton.onClick.AddListener(level.GoMainMenu);
            musicSlider.value = SaveSystem.MusicVolume;
            sfxSlider.value = SaveSystem.SfxVolume;
            musicSlider.onValueChanged.AddListener(v => { SaveSystem.MusicVolume = v; AudioManager.I.ApplyVolumes(); });
            sfxSlider.onValueChanged.AddListener(v => { SaveSystem.SfxVolume = v; AudioManager.I.ApplyVolumes(); });
            if (drops != null)
            {
                _dropStart = new Vector2[drops.Length];
                for (int i = 0; i < drops.Length; i++) _dropStart[i] = drops[i].anchoredPosition;
            }
            if (truck != null) _truckPos = truck.anchoredPosition;
            root.SetActive(false);
        }

        public void Show(bool show)
        {
            root.SetActive(show);
            if (!show) return;
            _open = 0f;
            var d = level.Data;
            levelText.text = (level.levelIndex + 1) + Loc.T(". BÖLÜM  •  ") + Loc.T(d.levelName).ToUpper() + "   —   " + Loc.T("Müşteri: ") + Loc.T(d.customer);
            var c = level.cargo;
            waterText.text = c.RemainingLitres + " / " + c.TotalLitres + " L";
            if (waterFill != null) waterFill.fillAmount = c.RemainingRatio;
            float t = level.ElapsedTime;
            timeText.text = string.Format("{0}:{1:00}", (int)(t / 60), (int)(t % 60));
            float left = Mathf.Max(0f, level.terrain.FinishX - level.truck.transform.position.x);
            distanceText.text = Mathf.RoundToInt(left) + " m";
            _tip = Random.Range(0, Tips.Length);
            _tipTimer = 0f;
            tipText.text = Loc.T(Tips[_tip]);
        }

        void Update()
        {
            if (!root.activeSelf) return;
            float ut = Time.unscaledTime;
            float dt = Time.unscaledDeltaTime;

            // Panel açılış animasyonu (büyüyerek gelir)
            _open = Mathf.MoveTowards(_open, 1f, dt * 5f);
            float e = 1f - Mathf.Pow(1f - _open, 3f);
            if (panel != null) panel.localScale = Vector3.one * Mathf.Lerp(0.85f, 1f, e);

            // Kamyon hafifçe zıplar
            if (truck != null) truck.anchoredPosition = _truckPos + new Vector2(Mathf.Sin(ut * 1.3f) * 8f, Mathf.Abs(Mathf.Sin(ut * 4f)) * 6f);

            // Su damlaları yukarı süzülür
            if (drops != null)
                for (int i = 0; i < drops.Length; i++)
                {
                    float speed = 25f + (i % 4) * 12f;
                    float y = Mathf.Repeat(_dropStart[i].y + 600f + ut * speed, 1300f) - 650f;
                    drops[i].anchoredPosition = new Vector2(_dropStart[i].x + Mathf.Sin(ut * 0.8f + i) * 20f, y);
                    drops[i].localRotation = Quaternion.Euler(0, 0, Mathf.Sin(ut + i) * 12f);
                }

            // İpucu her 4 sn'de değişir
            _tipTimer += dt;
            if (_tipTimer > 4f)
            {
                _tipTimer = 0f;
                _tip = (_tip + 1) % Tips.Length;
                tipText.text = Loc.T(Tips[_tip]);
            }
        }
    }
}

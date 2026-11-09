using UnityEngine;
using UnityEngine.UI;

namespace VasifliSu
{
    /// <summary>Bölüm sonu ekranı: başarı/başarısızlık, teslim edilen su oranı, süre, skor ve yıldızlar.</summary>
    public class ResultPanel : MonoBehaviour
    {
        public struct Result
        {
            public bool success;
            public string title, reason;
            public float ratio, time, parTime;
            public int litres, totalLitres, score, best, stars;
            public bool starRatio, starTime, newBest;
        }

        public LevelManager level;
        public GameObject root;
        public Image header;
        public Text titleText, reasonText, ratioText, litresText, timeText, scoreText, bestText, criteriaText;
        public Image ratioFill;
        public Image[] stars;
        public Button retryButton, nextButton, levelSelectButton, menuButton;

        float _anim;
        Result _r;

        void Start()
        {
            retryButton.onClick.AddListener(level.Restart);
            nextButton.onClick.AddListener(level.NextLevel);
            levelSelectButton.onClick.AddListener(level.GoLevelSelect);
            menuButton.onClick.AddListener(level.GoMainMenu);
            root.SetActive(false);
        }

        public void Show(Result r)
        {
            _r = r;
            _anim = 0f;
            root.SetActive(true);
            header.color = r.success ? Brand.Green : Brand.Red;
            titleText.text = r.title;
            reasonText.text = r.reason;
            litresText.text = r.litres + " / " + r.totalLitres + " L";
            timeText.text = string.Format("{0}:{1:00}  <size=26>({4} {2}:{3:00})</size>", (int)(r.time / 60), (int)(r.time % 60), (int)(r.parTime / 60), (int)(r.parTime % 60), Loc.T("hedef"));
            bestText.text = Loc.T("En iyi: ") + r.best + (r.newBest ? "  <color=#FF9F1C>" + Loc.T("YENİ REKOR!") + "</color>" : "");
            if (r.success)
            {
                criteriaText.text =
                    "★ " + Loc.T("Teslimat tamamlandı") + "\n" +
                    (r.starRatio ? "★ " : "☆ ") + Loc.T("Suyun en az %85'i teslim") + "\n" +
                    (r.starTime ? "★ " : "☆ ") + Loc.T("Hedef sürede teslim");
            }
            else criteriaText.text = Loc.T("İpucu: Yokuş inerken frene (◄) bas,\nhavadayken gaz/frenle dengele.");

            nextButton.gameObject.SetActive(r.success && !level.IsLastLevel);
            for (int i = 0; i < stars.Length; i++)
            {
                stars[i].sprite = GameAssets.I.iconStarEmpty;
                stars[i].transform.localScale = Vector3.one;
            }
        }

        void Update()
        {
            if (!root.activeSelf) return;
            _anim += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(_anim / 1.2f);
            float ratio = Mathf.Lerp(0f, _r.ratio, k);
            ratioFill.fillAmount = ratio;
            ratioText.text = Loc.English ? Mathf.RoundToInt(ratio * 100f) + "%" : "%" + Mathf.RoundToInt(ratio * 100f);
            scoreText.text = Mathf.RoundToInt(Mathf.Lerp(0, _r.score, k)).ToString("N0");
            for (int i = 0; i < stars.Length; i++)
            {
                bool on = _r.success && i < _r.stars && _anim > 0.4f + i * 0.35f;
                if (on && stars[i].sprite != GameAssets.I.iconStar)
                {
                    stars[i].sprite = GameAssets.I.iconStar;
                    stars[i].transform.localScale = Vector3.one * 1.5f;
                    AudioManager.I.Play(GameAssets.I.beep, 0.6f);
                }
                stars[i].transform.localScale = Vector3.Lerp(stars[i].transform.localScale, Vector3.one, Time.unscaledDeltaTime * 8f);
            }
        }
    }
}

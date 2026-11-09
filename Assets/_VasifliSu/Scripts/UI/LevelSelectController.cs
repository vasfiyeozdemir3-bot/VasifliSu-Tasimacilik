using UnityEngine;
using UnityEngine.UI;

namespace VasifliSu
{
    /// <summary>
    /// Bölüm seçimi: Vasıflı Su deposunun 5 kepenk kapısı. Seçilen kapının kepengi yukarı kalkar,
    /// içeride bölümün manzarası ve kamyon görünür. Kilitli kapılarda kilit + zincir vardır.
    /// Alttaki bilgi şeridinde sipariş detayları ve "Teslimata Çık" butonu bulunur.
    /// </summary>
    public class LevelSelectController : MonoBehaviour
    {
        [System.Serializable]
        public class Door
        {
            public Button button;
            public Image shutter;        // Filled (dikey, üstten) kepenk
            public RectTransform truck;  // kapı içindeki kamyon
            public GameObject lockGroup; // kilit + zincir
            public Image[] stars;
            public Image lamp;
        }

        public Door[] doors;
        public Text titleText, subtitleText, customerText, descText, infoText, bestText, playLabel, totalText;
        public Image[] detailStars;
        public Button playButton, backButton;

        int _selected = -1;
        int _unlocked;
        float _t;

        void Start()
        {
            Time.timeScale = 1f;
            AudioManager.I.PlayMusic();
            _unlocked = SaveSystem.UnlockedCount;
            backButton.onClick.AddListener(SceneFlow.LoadMainMenu);
            playButton.onClick.AddListener(() => { if (_selected >= 0 && _selected < _unlocked) SceneFlow.LoadLevel(_selected); });
            var a = GameAssets.I;
            for (int i = 0; i < doors.Length; i++)
            {
                int idx = i;
                var d = doors[i];
                bool open = i < _unlocked;
                d.lockGroup.SetActive(!open);
                int s = SaveSystem.GetStars(i);
                for (int k = 0; k < d.stars.Length; k++) d.stars[k].sprite = k < s ? a.iconStar : a.iconStarEmpty;
                d.button.onClick.AddListener(() => Select(idx));
                d.shutter.fillAmount = 1f;
            }
            totalText.text = "★ " + SaveSystem.TotalStars() + " / 15     " + Loc.T("Toplam teslim: ") + SaveSystem.TotalLitres + " L";
            Select(Mathf.Clamp(_unlocked - 1, 0, doors.Length - 1));
        }

        public int SelectedIndex => _selected;

        public void Select(int i)
        {
            if (i == _selected) { if (i < _unlocked) SceneFlow.LoadLevel(i); return; }   // açık kapıya tekrar tıklamak = başla
            _selected = i;
            var ld = LevelDatabase.I.levels[i];
            bool open = i < _unlocked;
            if (GameAssets.I) AudioManager.I.Play(GameAssets.I.click, 0.6f);
            for (int k = 0; k < doors.Length; k++)
                if (doors[k].truck) doors[k].truck.anchoredPosition = new Vector2(-360f, doors[k].truck.anchoredPosition.y);

            titleText.text = (i + 1) + ". " + Loc.T(ld.levelName);
            subtitleText.text = Loc.T(ld.subtitle).ToUpper();
            customerText.text = Loc.T("Müşteri: ") + Loc.T(ld.customer);
            descText.text = Loc.T(ld.description);
            string pct = Loc.English ? Mathf.RoundToInt(ld.requiredRatio * 100) + "%" : "%" + Mathf.RoundToInt(ld.requiredRatio * 100);
            infoText.text = Loc.T("Mesafe") + ": <b>" + Mathf.RoundToInt(ld.length) + " m</b>     " +
                            Loc.T("Kargo") + ": <b>" + ld.damacanaCount + " " + Loc.T("damacana") + "</b>     " +
                            Loc.T("Min. teslim") + ": <b>" + pct + "</b>     " +
                            Loc.T("Hedef süre") + ": <b>" + Mathf.RoundToInt(ld.parTime) + (Loc.English ? " s" : " sn") + "</b>";
            int s = SaveSystem.GetStars(i);
            for (int k = 0; k < detailStars.Length; k++) detailStars[k].sprite = k < s ? GameAssets.I.iconStar : GameAssets.I.iconStarEmpty;
            int best = SaveSystem.GetBest(i);
            bestText.text = open ? (best > 0 ? Loc.T("En iyi skor: ") + best : Loc.T("Henüz teslim edilmedi")) : Loc.T("Önceki siparişi tamamla");
            playButton.interactable = open;
            playLabel.text = Loc.T(open ? "TESLİMATA ÇIK" : "KİLİTLİ");
        }

        void Update()
        {
            var kb = UnityEngine.InputSystem.Keyboard.current;
            if (kb != null)
            {
                if (kb.rightArrowKey.wasPressedThisFrame || kb.dKey.wasPressedThisFrame) Select((_selected + 1) % doors.Length);
                if (kb.leftArrowKey.wasPressedThisFrame || kb.aKey.wasPressedThisFrame) Select((_selected + doors.Length - 1) % doors.Length);
                if ((kb.enterKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame) && _selected < _unlocked) SceneFlow.LoadLevel(_selected);
                if (kb.escapeKey.wasPressedThisFrame) SceneFlow.LoadMainMenu();
            }

            _t += Time.unscaledDeltaTime;
            float dt = Time.unscaledDeltaTime;
            for (int k = 0; k < doors.Length; k++)
            {
                var d = doors[k];
                bool sel = k == _selected;
                // Seçilen her kapının kepengi kalkar (kilitliler de görüntülenebilir ama başlatılamaz)
                bool canOpen = sel;
                float target = canOpen ? 0.12f : 1f;
                d.shutter.fillAmount = Mathf.MoveTowards(d.shutter.fillAmount, target, dt * 1.8f);
                // Kilit/zincir yalnızca kapı kapalıyken görünür
                if (k >= _unlocked) d.lockGroup.SetActive(d.shutter.fillAmount > 0.6f);
                var rt = (RectTransform)d.button.transform;
                rt.localRotation = Quaternion.identity;
                rt.localScale = Vector3.Lerp(rt.localScale, Vector3.one * (sel ? 1.06f : 1f), dt * 10f);
                if (d.lamp) d.lamp.color = sel ? new Color(1f, 0.85f, 0.4f, 0.9f) : new Color(1f, 0.85f, 0.4f, 0.25f);
                // Kepenk açılınca kamyon içeriden öne doğru gelir
                if (d.truck)
                {
                    float tx = canOpen && d.shutter.fillAmount < 0.5f ? 0f : -360f;
                    var p = d.truck.anchoredPosition;
                    p.x = Mathf.Lerp(p.x, tx, dt * 3f);
                    p.y = d.truck.anchoredPosition.y;
                    d.truck.anchoredPosition = p;
                    d.truck.localRotation = Quaternion.Euler(0, 0, canOpen ? Mathf.Sin(_t * 9f) * 0.8f : 0f);
                }
            }
        }
    }
}

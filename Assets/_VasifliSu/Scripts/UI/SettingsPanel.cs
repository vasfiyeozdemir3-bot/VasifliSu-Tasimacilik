using UnityEngine;
using UnityEngine.UI;

namespace VasifliSu
{
    /// <summary>Ayarlar: ana ses, müzik, efekt, dil (Türkçe/English), tam ekran, ilerlemeyi sıfırla.</summary>
    public class SettingsPanel : MonoBehaviour
    {
        public GameObject root;
        public Slider masterSlider, musicSlider, sfxSlider;
        public Text masterValue, musicValue, sfxValue;
        public Button turkishButton, englishButton;
        public Toggle fullscreenToggle;
        public Button resetButton, closeButton;
        public Text resetText;
        public MainMenuController menu;

        bool _confirm;

        void Start()
        {
            masterSlider.value = SaveSystem.MasterVolume;
            musicSlider.value = SaveSystem.MusicVolume;
            sfxSlider.value = SaveSystem.SfxVolume;
            fullscreenToggle.isOn = Screen.fullScreen;
            masterSlider.onValueChanged.AddListener(v => { SaveSystem.MasterVolume = v; AudioManager.I.ApplyVolumes(); UpdateValues(); });
            musicSlider.onValueChanged.AddListener(v => { SaveSystem.MusicVolume = v; AudioManager.I.ApplyVolumes(); UpdateValues(); });
            sfxSlider.onValueChanged.AddListener(v => { SaveSystem.SfxVolume = v; AudioManager.I.ApplyVolumes(); UpdateValues(); });
            fullscreenToggle.onValueChanged.AddListener(v => Screen.fullScreen = v);
            turkishButton.onClick.AddListener(() => { Loc.SetEnglish(false); UpdateLang(); });
            englishButton.onClick.AddListener(() => { Loc.SetEnglish(true); UpdateLang(); });
            resetButton.onClick.AddListener(OnReset);
            closeButton.onClick.AddListener(() => Show(false));
            UpdateValues();
            UpdateLang();
            root.SetActive(false);
        }

        void UpdateValues()
        {
            masterValue.text = "%" + Mathf.RoundToInt(masterSlider.value * 100);
            musicValue.text = "%" + Mathf.RoundToInt(musicSlider.value * 100);
            sfxValue.text = "%" + Mathf.RoundToInt(sfxSlider.value * 100);
        }

        void UpdateLang()
        {
            bool en = Loc.English;
            turkishButton.image.color = en ? new Color(1f, 1f, 1f, 0.55f) : Color.white;
            englishButton.image.color = en ? Color.white : new Color(1f, 1f, 1f, 0.55f);
            resetText.text = Loc.T("İlerlemeyi Sıfırla");
            _confirm = false;
        }

        public void Show(bool s)
        {
            root.SetActive(s);
            _confirm = false;
            resetText.text = Loc.T("İlerlemeyi Sıfırla");
        }

        void OnReset()
        {
            if (!_confirm) { _confirm = true; resetText.text = Loc.T("Emin misin? Tekrar bas"); return; }
            SaveSystem.ResetProgress();
            _confirm = false;
            resetText.text = Loc.T("Sıfırlandı ✓");
            if (menu != null) menu.RefreshStats();
        }
    }
}

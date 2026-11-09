using UnityEngine;

namespace VasifliSu
{
    /// <summary>
    /// Oyunun ortak görsel/ses varlıklarını tutan merkezi ScriptableObject.
    /// Resources/GameAssets.asset olarak durur; her yerden GameAssets.I ile erişilir.
    /// </summary>
    [CreateAssetMenu(menuName = "Vasıflı Su/Game Assets", fileName = "GameAssets")]
    public class GameAssets : ScriptableObject
    {
        static GameAssets _instance;
        public static GameAssets I
        {
            get
            {
                if (_instance == null) _instance = Resources.Load<GameAssets>("GameAssets");
                return _instance;
            }
        }

        [Header("Fontlar")]
        public Font fontBold;
        public Font fontRegular;

        [Header("Arayüz")]
        public Sprite logo;
        public Sprite menuBackground;
        public Sprite button;
        public Sprite buttonAlt;
        public Sprite buttonRound;
        public Sprite panel;
        public Sprite panelDark;
        public Sprite barBg;
        public Sprite barFill;
        public Sprite card;

        [Header("İkonlar")]
        public Sprite iconWater;
        public Sprite iconSpeed;
        public Sprite iconTimer;
        public Sprite iconStar;
        public Sprite iconStarEmpty;
        public Sprite iconLock;
        public Sprite iconPause;
        public Sprite iconGas;
        public Sprite iconBrake;
        public Sprite iconSettings;
        public Sprite iconHome;
        public Sprite iconRetry;
        public Sprite iconNext;
        public Sprite iconPlay;
        public Sprite iconDoc;
        public Sprite iconExit;
        public Sprite iconFlag;

        [Header("Dünya")]
        public Sprite rock;
        public Sprite depot;
        public Sprite deliveryPoint;
        public Sprite splash;

        [Header("Sesler")]
        public AudioClip music;
        public AudioClip click;
        public AudioClip splashSfx;
        public AudioClip engine;
        public AudioClip win;
        public AudioClip lose;
        public AudioClip beep;
        public AudioClip go;
    }

    /// <summary>Vasıflı Su marka renkleri.</summary>
    public static class Brand
    {
        public static readonly Color Blue = new Color32(0x0E, 0x6B, 0xC9, 0xFF);
        public static readonly Color DeepBlue = new Color32(0x0A, 0x2E, 0x5C, 0xFF);
        public static readonly Color Aqua = new Color32(0x3F, 0xD0, 0xE8, 0xFF);
        public static readonly Color Foam = new Color32(0xEA, 0xF8, 0xFF, 0xFF);
        public static readonly Color Orange = new Color32(0xFF, 0x9F, 0x1C, 0xFF);
        public static readonly Color Green = new Color32(0x3C, 0xC2, 0x6B, 0xFF);
        public static readonly Color Red = new Color32(0xE8, 0x4A, 0x4A, 0xFF);
        public static readonly Color TextDark = new Color32(0x0A, 0x2E, 0x5C, 0xFF);
        public static readonly Color White = Color.white;
    }
}

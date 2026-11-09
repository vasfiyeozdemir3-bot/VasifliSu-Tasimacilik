using UnityEngine;

namespace VasifliSu
{
    /// <summary>
    /// Bir bölümün tüm tasarım parametreleri: arazi, zorluk, kargo ve görsel tema.
    /// </summary>
    [CreateAssetMenu(menuName = "Vasıflı Su/Level Data", fileName = "Level_X")]
    public class LevelData : ScriptableObject
    {
        [Header("Kimlik")]
        public string levelName = "Bölüm";
        public string subtitle = "";
        public string customer = "";
        [TextArea(2, 4)] public string description = "";

        [Header("Arazi")]
        [Tooltip("Başlangıçtan teslimat noktasına kadar yol uzunluğu (birim = metre)")]
        public float length = 200f;
        [Tooltip("Büyük tepelerin yüksekliği")]
        public float amplitude = 3f;
        [Tooltip("Tepelerin sıklığı")]
        public float frequency = 0.06f;
        [Tooltip("Küçük tümsek/engebe miktarı")]
        public float roughness = 0.2f;
        [Tooltip("Genel eğim (pozitif = yokuş yukarı)")]
        public float climb = 0f;
        public int seed = 1;
        [Tooltip("Zemin sürtünmesi (kar < 0.5)")]
        public float groundFriction = 1f;
        public int rockCount = 3;

        [Header("Kargo ve Hedef")]
        public int damacanaCount = 6;
        [Range(0f, 1f)] public float requiredRatio = 0.5f;
        [Tooltip("3. yıldız için hedef süre (saniye)")]
        public float parTime = 60f;

        [Header("Tema")]
        public Color skyColor = new Color(0.55f, 0.82f, 1f);
        public Color grassColor = new Color(0.35f, 0.75f, 0.3f);
        public Material groundMaterial;
        public Material grassMaterial;
        public Sprite backgroundFar;
        public Sprite backgroundNear;
        public Sprite[] decorSprites;
        [Tooltip("Her kaç metrede bir dekor objesi")]
        public float decorSpacing = 9f;
        [Tooltip("Yol kenarındaki ev / bina sprite'ları")]
        public Sprite[] buildingSprites;
        public float buildingSpacing = 28f;
        [Tooltip("Yolda kayalarla karışık çıkan engeller (kütük, sandık, koni, varil...)")]
        public Sprite[] obstacleSprites;
        [Tooltip("Yol kenarı tabelaları")]
        public Sprite[] signSprites;
        [Tooltip("Kar yağışı efekti")]
        public bool snowfall;
    }
}

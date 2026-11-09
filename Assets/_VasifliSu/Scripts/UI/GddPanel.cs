using UnityEngine;
using UnityEngine.UI;

namespace VasifliSu
{
    /// <summary>Oyun içinden GDD erişimi: özet metin + tam PDF'i açma.</summary>
    public class GddPanel : MonoBehaviour
    {
        public GameObject root;
        public Button openPdfButton, closeButton;
        public Text summaryText;

        void Start()
        {
            openPdfButton.onClick.AddListener(OpenPdf);
            closeButton.onClick.AddListener(() => Show(false));
            root.SetActive(false);
        }

        public void Show(bool s)
        {
            root.SetActive(s);
            if (s && summaryText != null) summaryText.text = Loc.English ? SummaryEn : Summary;
        }

        void OpenPdf()
        {
            string path = Application.streamingAssetsPath + "/VasifliSu_GDD.pdf";
#if UNITY_WEBGL && !UNITY_EDITOR
            Application.OpenURL(path);
#else
            Application.OpenURL("file:///" + path.Replace("\\", "/"));
#endif
        }

        public const string SummaryEn =
"<b><size=34>Vasıflı Su Transport</size></b>\n" +
"<i>Game Design Document – Summary</i>\n\n" +
"<b>Story</b>\n" +
"Vasıflı Su Transport is the town's brand-new water delivery company and you are its first driver. Deliver 19-litre water jugs to mountain villages, forests, snowy peaks and canyons without tipping over, and make the brand the most trusted water company in the region.\n\n" +
"<b>Core Mechanic</b>\n" +
"Both wheels are attached to the body with <b>WheelJoint2D</b>. The motor (JointMotor2D) spins the wheels and the suspension (JointSuspension2D) absorbs bumps. The jugs in the bed are real physics objects; rough driving makes them fall out.\n\n" +
"<b>Controls</b>\n" +
"SHIFT / ► / D : Gas        CTRL / SPACE / ◄ / A : Brake - Reverse\n" +
"In the air: Gas = nose up, Brake = nose down\n" +
"ESC / P : Pause     R : Restart\n" +
"\n" +
"<b>Win / Lose</b>\n" +
"Reach the delivery point with the required share of water. Flipping over, leaving the road or dropping below the required share fails the delivery.\n\n" +
"<b>Score and Stars</b>\n" +
"Score = litres delivered × 10 + water ratio × 1000 + time bonus\n" +
"★ Delivery   ★ 85%+ water   ★ Target time\n\n" +
"<b>Gamification – Brand Power</b>\n" +
"Every litre you deliver adds to the brand power and raises your rank: Trainee Courier → Neighborhood Courier → Regional Manager → Logistics Chief → Vasıflı Su Legend.\n\n" +
"<b>Levels</b>\n" +
"1. Volcano Foothills – volcano and lava, tutorial\n" +
"2. City Center – night, roadworks\n" +
"3. Sunset Valley – steep slopes and rocks\n" +
"4. Snowball Town – snowy town, slippery ground\n" +
"5. Blue Harbor – port/industry, the longest, toughest route\n\n" +
"Use \"OPEN PDF\" for the full document.";

        public const string Summary =
"<b><size=34>Vasıflı Su Taşımacılık</size></b>\n" +
"<i>Game Design Document – Özet</i>\n\n" +
"<b>Hikaye</b>\n" +
"Vasıflı Su Taşımacılık, kasabanın yeni kurulan su dağıtım firmasıdır. Sen firmanın ilk sürücüsüsün. Volkan eteğine, şehir merkezine, vadilere, karlı kasabalara ve limana 19 litrelik damacanaları devirmeden ulaştırarak markayı bölgenin en güvenilir su firması yapmalısın.\n\n" +
"<b>Temel Mekanik</b>\n" +
"Kamyonun iki tekerleği gövdeye <b>WheelJoint2D</b> ile bağlıdır. Motor (JointMotor2D) tekerlekleri döndürür, süspansiyon (JointSuspension2D) engebeleri emer. Kasadaki damacanalar gerçek fizik nesneleridir; sert sürüşte kasadan düşer ve kaybolur.\n\n" +
"<b>Kontroller</b>\n" +
"SHIFT / ► / D : Gaz        CTRL / BOŞLUK / ◄ / A : Fren - Geri\n" +
"Havada: Gaz = burun yukarı, Fren = burun aşağı\n" +
"ESC / P : Duraklat     R : Yeniden başla\n" +
"\n" +
"<b>Kazanma / Kaybetme</b>\n" +
"Teslimat noktasına, bölümün istediği oranda suyla ulaş. Kamyon devrilirse, yoldan çıkarsa ya da su gereken oranın altına düşerse teslimat başarısız olur.\n\n" +
"<b>Skor ve Yıldızlar</b>\n" +
"Skor = teslim edilen litre × 10 + su oranı × 1000 + zaman bonusu\n" +
"★ Teslimat   ★ %85+ su   ★ Hedef süre\n\n" +
"<b>Oyunlaştırma – Marka Gücü</b>\n" +
"Teslim ettiğin her litre firmanın marka gücüne eklenir ve rütben yükselir: Stajyer Dağıtıcı → Mahalle Dağıtıcısı → Bölge Sorumlusu → Lojistik Şefi → Vasıflı Su Efsanesi.\n\n" +
"<b>Bölümler</b>\n" +
"1. Volkan Eteği – volkan ve lav, öğretici\n" +
"2. Şehir Merkezi – gece, şantiye engelleri\n" +
"3. Gün Batımı Vadisi – dik yokuşlar ve kayalar\n" +
"4. Kartopu Kasabası – karlı kasaba, kaygan zemin\n" +
"5. Mavi Liman – liman/sanayi, en uzun ve en sert parkur\n\n" +
"Tam doküman için \"PDF'i Aç\" butonunu kullan.";
    }
}

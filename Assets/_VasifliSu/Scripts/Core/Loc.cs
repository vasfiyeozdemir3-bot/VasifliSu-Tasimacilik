using System.Collections.Generic;
using UnityEngine;

namespace VasifliSu
{
    /// <summary>
    /// Basit iki dilli yerelleştirme. Anahtar = Türkçe metin; İngilizce seçiliyse sözlükten karşılığı döner.
    /// </summary>
    public static class Loc
    {
        const string Key = "vs_lang";
        public static event System.Action Changed;

        public static bool English => PlayerPrefs.GetInt(Key, 0) == 1;

        public static void SetEnglish(bool en)
        {
            PlayerPrefs.SetInt(Key, en ? 1 : 0);
            PlayerPrefs.Save();
            Changed?.Invoke();
        }

        public static string T(string tr)
        {
            if (!English || string.IsNullOrEmpty(tr)) return tr;
            return En.TryGetValue(tr, out var e) ? e : tr;
        }

        static readonly Dictionary<string, string> En = new Dictionary<string, string>
        {
            // Ana menü
            { "OYNA", "PLAY" }, { "AYARLAR", "SETTINGS" }, { "GDD", "GDD" }, { "ÇIKIŞ", "QUIT" },
            { "Rütbe: ", "Rank: " }, { "Toplam teslim: ", "Total delivered: " }, { "Sonraki rütbe: ", "Next rank: " },
            { "v1.0  •  Vasıflı Su Taşımacılık (kurgusal marka)", "v1.0  •  Vasıflı Su Transport (fictional brand)" },
            { "Stajyer Dağıtıcı", "Trainee Courier" }, { "Mahalle Dağıtıcısı", "Neighborhood Courier" },
            { "Bölge Sorumlusu", "Regional Manager" }, { "Lojistik Şefi", "Logistics Chief" }, { "Vasıflı Su Efsanesi", "Vasıflı Su Legend" },
            // Ayarlar
            { "Ana Ses", "Master" }, { "Müzik", "Music" }, { "Efektler", "Effects" }, { "Tam Ekran", "Fullscreen" }, { "Dil", "Language" },
            { "İlerlemeyi Sıfırla", "Reset Progress" }, { "Emin misin? Tekrar bas", "Sure? Press again" }, { "Sıfırlandı ✓", "Reset ✓" },
            { "TAMAM", "OK" }, { "KAPAT", "CLOSE" }, { "PDF'İ AÇ", "OPEN PDF" },
            { "OYUN TASARIM DOKÜMANI (GDD)", "GAME DESIGN DOCUMENT (GDD)" },
            // Sipariş ekranı
            { "TESLİMAT SİPARİŞLERİ", "DELIVERY ORDERS" }, { "Bir sipariş seç ve teslimata çık", "Pick an order and hit the road" },
            { "SİPARİŞ", "ORDER" }, { "Müşteri: ", "Customer: " }, { "TESLİMATA ÇIK", "START DELIVERY" }, { "KİLİTLİ", "LOCKED" },
            { "ANA MENÜ", "MAIN MENU" }, { "TESLİM EDİLDİ", "DELIVERED" }, { "Henüz teslim edilmedi", "Not delivered yet" },
            { "En iyi skor: ", "Best score: " }, { "Önceki siparişi tamamla", "Complete the previous order" },
            { "Mesafe", "Distance" }, { "Kargo", "Cargo" }, { "Min. teslim", "Min. delivery" }, { "Hedef süre", "Target time" },
            { "damacana", "jugs" }, { "Dikkat: kaygan zemin!", "Careful: slippery ground!" },
            // Bölümler
            { "Volkan Eteği", "Volcano Foothills" }, { "Şehir Merkezi", "City Center" }, { "Gün Batımı Vadisi", "Sunset Valley" },
            { "Kartopu Kasabası", "Snowball Town" }, { "Mavi Liman", "Blue Harbor" },
            { "Öğretici teslimat", "Tutorial delivery" }, { "Gece teslimatı", "Night delivery" }, { "Dik yokuşlar", "Steep slopes" },
            { "Kaygan zemin", "Slippery ground" }, { "Final teslimatı", "Final delivery" },
            { "Volkan Gözlemevi", "Volcano Observatory" }, { "Merkez Hastanesi", "Central Hospital" }, { "Vadi Çiftliği", "Valley Farm" },
            { "Kayak Oteli", "Ski Hotel" }, { "Liman Fabrikası", "Harbor Factory" },
            { "Volkan eteğindeki gözlemevine ilk teslimat. Kontrolleri öğren, lav çatlaklarına dikkat!", "First delivery to the volcano observatory. Learn the controls and mind the lava cracks!" },
            { "Gece vardiyasındaki şehir hastanesine acil su! Şantiye engellerine dikkat.", "Urgent water for the city hospital's night shift! Watch out for roadworks." },
            { "Vadideki çiftliklere giden yol dik ve kayalarla dolu. Ağırlık merkezine dikkat!", "The road to the valley farms is steep and rocky. Mind the center of mass!" },
            { "Karlı kasabanın kayak oteline kışlık su. Kar kaygandır, gazı yavaş ver!", "Winter water for the snowy town's ski hotel. Snow is slippery, ease on the gas!" },
            { "Limandaki fabrika işçileri susuz! Konteynerler ve vinçler arasında en uzun ve en sert rota.", "The harbor factory workers are out of water! The longest, toughest route among containers and cranes." },
            // HUD
            { ". BÖLÜM  •  ", ". LEVEL  •  " }, { "km/s", "km/h" }, { "YOLA ÇIK!", "GO!" },
            // Duraklatma
            { "DURAKLATILDI", "PAUSED" }, { "DEVAM ET", "RESUME" }, { "YENİDEN BAŞLAT", "RESTART" }, { "SİPARİŞLER", "ORDERS" },
            { "ESC: devam  •  R: yeniden başlat", "ESC: resume  •  R: restart" },
            { "Kalan yol", "Distance left" },
            { "Yokuş aşağı inerken frene bas, damacanalar kaymasın.", "Brake when going downhill so the jugs don't slide." },
            { "Havadayken gaz burnu kaldırır, fren indirir.", "In the air, gas lifts the nose and brake lowers it." },
            { "Yumuşak gaz = daha az dökülen su!", "Gentle throttle = less spilled water!" },
            { "Hedef sürede bitirirsen 3. yıldız senin.", "Finish within the target time for the 3rd star." },
            { "Her teslim edilen litre rütbeni yükseltir.", "Every litre delivered raises your rank." },
            // Sonuç
            { "TESLİMAT TAMAM!", "DELIVERED!" }, { "TESLİMAT BAŞARISIZ", "DELIVERY FAILED" },
            { " müşterileri suyuna kavuştu.", " customers got their water." },
            { "Kamyon devrildi!", "The truck flipped over!" }, { "Kamyon yoldan çıktı!", "The truck left the road!" },
            { "Yeterli su kalmadı!", "Not enough water left!" }, { "En az %{0} teslim edilmeliydi.", "At least {0}% had to be delivered." },
            { "Teslim edilen su", "Water delivered" }, { "Su miktarı", "Water amount" }, { "Süre", "Time" }, { "SKOR", "SCORE" },
            { "En iyi: ", "Best: " }, { "YENİ REKOR!", "NEW RECORD!" }, { "hedef", "target" },
            { "Teslimat tamamlandı", "Delivery completed" }, { "Suyun en az %85'i teslim", "At least 85% of the water delivered" },
            { "Hedef sürede teslim", "Delivered within target time" },
            { "Toplanan coin: ", "Coins collected: " },
            { "İpucu: Yokuş inerken frene (◄) bas,\nhavadayken gaz/frenle dengele.", "Tip: Brake (◄) when going downhill,\nbalance with gas/brake in the air." },
            { "TEKRAR", "RETRY" }, { "MENÜ", "MENU" }, { "SONRAKİ", "NEXT" },
        };
    }
}

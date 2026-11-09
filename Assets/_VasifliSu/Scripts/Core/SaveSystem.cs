using UnityEngine;

namespace VasifliSu
{
    /// <summary>
    /// PlayerPrefs tabanlı kayıt: açılan bölümler, yıldızlar, en iyi skor,
    /// toplam teslim edilen su (marka gücü) ve ses ayarları.
    /// </summary>
    public static class SaveSystem
    {
        const string KeyUnlocked = "hy_unlocked";
        const string KeyTotalLitres = "hy_total_litres";
        const string KeyMusic = "hy_music";
        const string KeySfx = "hy_sfx";
        const string KeyMaster = "hy_master";

        static string StarsKey(int i) => "hy_stars_" + i;
        static string BestKey(int i) => "hy_best_" + i;

        /// <summary>Açık olan en yüksek bölüm sayısı (1 = sadece ilk bölüm).</summary>
        public static int UnlockedCount
        {
            get => Mathf.Max(1, PlayerPrefs.GetInt(KeyUnlocked, 1));
            set { PlayerPrefs.SetInt(KeyUnlocked, value); PlayerPrefs.Save(); }
        }

        public static int TotalLitres => PlayerPrefs.GetInt(KeyTotalLitres, 0);

        public static int GetStars(int levelIndex) => PlayerPrefs.GetInt(StarsKey(levelIndex), 0);
        public static int GetBest(int levelIndex) => PlayerPrefs.GetInt(BestKey(levelIndex), 0);

        public static float MusicVolume
        {
            get => PlayerPrefs.GetFloat(KeyMusic, 0.6f);
            set { PlayerPrefs.SetFloat(KeyMusic, value); PlayerPrefs.Save(); }
        }

        public static float MasterVolume
        {
            get => PlayerPrefs.GetFloat(KeyMaster, 1f);
            set { PlayerPrefs.SetFloat(KeyMaster, value); PlayerPrefs.Save(); }
        }

        public static float SfxVolume
        {
            get => PlayerPrefs.GetFloat(KeySfx, 0.8f);
            set { PlayerPrefs.SetFloat(KeySfx, value); PlayerPrefs.Save(); }
        }

        /// <summary>Başarılı bir teslimatı kaydeder. Yeni rekorsa true döner.</summary>
        public static bool SaveResult(int levelIndex, int stars, int score, int litres)
        {
            bool newBest = score > GetBest(levelIndex);
            if (newBest) PlayerPrefs.SetInt(BestKey(levelIndex), score);
            if (stars > GetStars(levelIndex)) PlayerPrefs.SetInt(StarsKey(levelIndex), stars);
            if (levelIndex + 2 > UnlockedCount) PlayerPrefs.SetInt(KeyUnlocked, levelIndex + 2);
            PlayerPrefs.SetInt(KeyTotalLitres, TotalLitres + litres);
            PlayerPrefs.Save();
            return newBest;
        }

        public static int TotalStars()
        {
            int n = LevelDatabase.I != null ? LevelDatabase.I.Count : 5;
            int s = 0;
            for (int i = 0; i < n; i++) s += GetStars(i);
            return s;
        }

        /// <summary>Oyunlaştırma: toplam teslim edilen suya göre dağıtıcı rütbesi.</summary>
        public static string RankName(out float progressToNext, out int nextAt)
        {
            int[] limits = { 0, 200, 600, 1200, 2000 };
            string[] names = { "Stajyer Dağıtıcı", "Mahalle Dağıtıcısı", "Bölge Sorumlusu", "Lojistik Şefi", "Vasıflı Su Efsanesi" };
            int l = TotalLitres;
            int idx = 0;
            for (int i = 0; i < limits.Length; i++) if (l >= limits[i]) idx = i;
            if (idx == limits.Length - 1) { progressToNext = 1f; nextAt = l; }
            else
            {
                nextAt = limits[idx + 1];
                progressToNext = Mathf.InverseLerp(limits[idx], limits[idx + 1], l);
            }
            return Loc.T(names[idx]);
        }

        public static void ResetProgress()
        {
            float m = MusicVolume, s = SfxVolume, ma = MasterVolume;
            bool en = Loc.English;
            PlayerPrefs.DeleteAll();
            MusicVolume = m; SfxVolume = s; MasterVolume = ma;
            PlayerPrefs.SetInt("vs_lang", en ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
}

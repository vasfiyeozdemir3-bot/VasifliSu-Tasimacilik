using UnityEngine;

namespace VasifliSu
{
    /// <summary>Tüm bölümlerin sıralı listesi. Resources/LevelDatabase.asset</summary>
    [CreateAssetMenu(menuName = "Vasıflı Su/Level Database", fileName = "LevelDatabase")]
    public class LevelDatabase : ScriptableObject
    {
        static LevelDatabase _instance;
        public static LevelDatabase I
        {
            get
            {
                if (_instance == null) _instance = Resources.Load<LevelDatabase>("LevelDatabase");
                return _instance;
            }
        }

        public LevelData[] levels;

        public int Count => levels == null ? 0 : levels.Length;
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;

namespace VasifliSu
{
    /// <summary>Sahne isimleri ve geçişleri tek yerde.</summary>
    public static class SceneFlow
    {
        public const string MainMenu = "MainMenu";
        public const string LevelSelect = "LevelSelect";
        public static string LevelScene(int index) => "Level_" + (index + 1);

        public static void LoadMainMenu() { Time.timeScale = 1f; SceneManager.LoadScene(MainMenu); }
        public static void LoadLevelSelect() { Time.timeScale = 1f; SceneManager.LoadScene(LevelSelect); }
        public static void LoadLevel(int index) { Time.timeScale = 1f; SceneManager.LoadScene(LevelScene(index)); }
        public static void Reload() { Time.timeScale = 1f; SceneManager.LoadScene(SceneManager.GetActiveScene().name); }
    }
}

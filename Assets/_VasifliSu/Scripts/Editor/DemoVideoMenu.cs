using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace VasifliSu.EditorTools
{
    /// <summary>Tanıtım videosunu otomatik kaydeder: Recording/demo.mjpeg</summary>
    public static class DemoVideoMenu
    {
        [MenuItem("Vasıflı Su/5) 5 Bölümü Otomatik Test Et", priority = 41)]
        static void Test() => Start(DemoRecorder.TestMode);

        [MenuItem("Vasıflı Su/4) Tanıtım Videosu Kaydet", priority = 40)]
        static void Record() => Start(1);

        static void Start(int mode)
        {
            if (EditorApplication.isPlaying) { EditorApplication.isPlaying = false; return; }
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            EditorSceneManager.OpenScene("Assets/_VasifliSu/Scenes/MainMenu.unity");
            PlayerPrefs.SetInt(DemoRecorder.PrefKey, mode);
            PlayerPrefs.Save();
            var gvType = System.Type.GetType("UnityEditor.GameView,UnityEditor");
            if (gvType != null)
            {
                var gv = EditorWindow.GetWindow(gvType);
                gv.Focus();
                gv.maximized = true;
            }
            EditorApplication.isPlaying = true;
        }
    }
}

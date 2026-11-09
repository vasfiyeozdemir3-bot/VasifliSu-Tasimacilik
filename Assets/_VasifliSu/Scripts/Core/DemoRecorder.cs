using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace VasifliSu
{
    /// <summary>
    /// Tanıtım videosu kaydedici: oyunu otomatik oynatır (menü → depo kapıları → 5 bölüm),
    /// her kareyi JPG olarak tek bir .mjpeg dosyasına yazar. Editör menüsünden başlatılır
    /// ("Vasıflı Su/4) Tanıtım Videosu Kaydet"). Normal oyunda hiçbir etkisi yoktur.
    /// </summary>
    public class DemoRecorder : MonoBehaviour
    {
        public const string PrefKey = "vs_demo_record";
        const int Fps = 30;

        FileStream _out;
        string _dir;
        int _part;
        RenderTexture _rt;
        Texture2D _small;

        void OpenPart()
        {
            if (_out != null) { _out.Flush(); _out.Close(); }
            _out = new FileStream(Path.Combine(_dir, "demo_" + _part.ToString("00") + ".mjpeg"), FileMode.Create, FileAccess.Write);
            _part++;
        }
        int _frames;
        bool _recording;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            if (PlayerPrefs.GetInt(PrefKey, 0) != 1) return;
            PlayerPrefs.SetInt(PrefKey, 0);
            PlayerPrefs.Save();
            var go = new GameObject("DemoRecorder");
            DontDestroyOnLoad(go);
            go.AddComponent<DemoRecorder>();
        }

        void Start()
        {
            string dir = Path.Combine(Application.dataPath, "..", "Recording");
            Directory.CreateDirectory(dir);
            _dir = dir;
            foreach (var f in Directory.GetFiles(dir, "demo*.mjpeg")) File.Delete(f);
            OpenPart();
            Time.captureFramerate = Fps;
            InputBridge.Demo = true;
            _recording = true;
            StartCoroutine(Capture());
            StartCoroutine(Script());
        }

        IEnumerator Capture()
        {
            var eof = new WaitForEndOfFrame();
            while (_recording)
            {
                yield return eof;
                var tex = ScreenCapture.CaptureScreenshotAsTexture();
                if (_rt == null) { _rt = new RenderTexture(1280, 720, 0); _small = new Texture2D(1280, 720, TextureFormat.RGB24, false); }
                Graphics.Blit(tex, _rt);
                var prev = RenderTexture.active;
                RenderTexture.active = _rt;
                _small.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
                RenderTexture.active = prev;
                byte[] jpg = _small.EncodeToJPG(80);
                Destroy(tex);
                if (_frames > 0 && _frames % 900 == 0) OpenPart();
                _out.Write(jpg, 0, jpg.Length);
                _frames++;
            }
        }

        IEnumerator Wait(float seconds)
        {
            int target = _frames + Mathf.RoundToInt(seconds * Fps);
            while (_frames < target) yield return null;
        }

        IEnumerator WaitScene(string name)
        {
            while (SceneManager.GetActiveScene().name != name) yield return null;
            yield return null;
        }

        IEnumerator Script()
        {
            // 1) Ana menü
            yield return WaitScene(SceneFlow.MainMenu);
            yield return Wait(4f);

            // 2) Depo kapıları — kapılar sırayla seçilir
            SceneFlow.LoadLevelSelect();
            yield return WaitScene(SceneFlow.LevelSelect);
            yield return Wait(1.5f);
            var ls = FindFirstObjectByType<LevelSelectController>();
            if (ls != null)
            {
                for (int i = 0; i < ls.doors.Length; i++) { if (i != ls.SelectedIndex) ls.Select(i); yield return Wait(1.3f); }
                if (ls.SelectedIndex != 0) ls.Select(0); yield return Wait(1.5f);
            }

            // 3) Bölüm 1 — sonuna kadar sürülür, sonuç ekranı gösterilir
            yield return PlayLevel(0, 60f, false, true);
            // 4) Bölüm 2 — yolda duraklatma ekranı gösterilir
            yield return PlayLevel(1, 15f, true, false);
            // 5) Diğer bölümlerden kesitler
            yield return PlayLevel(2, 14f, false, false);
            yield return PlayLevel(3, 14f, false, false);
            yield return PlayLevel(4, 14f, false, false);

            // 6) Kapanış: ana menü
            InputBridge.DemoThrottle = 0f;
            SceneFlow.LoadMainMenu();
            yield return WaitScene(SceneFlow.MainMenu);
            yield return Wait(3.5f);

            _recording = false;
            yield return null;
            _out.Flush(); _out.Close();
            Debug.Log("[DemoRecorder] Kayıt bitti: " + _frames + " kare (" + (_frames / (float)Fps).ToString("0.0") + " sn)");
            Time.captureFramerate = 0;
            InputBridge.Demo = false;
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        IEnumerator PlayLevel(int index, float driveSeconds, bool showPause, bool untilFinish)
        {
            InputBridge.DemoThrottle = 0f;
            SceneFlow.LoadLevel(index);
            yield return WaitScene(SceneFlow.LevelScene(index));
            LevelManager lm = null;
            while ((lm = LevelManager.I) == null) yield return null;
            while (lm.State == GameState.Countdown) yield return null;

            float driven = 0f;
            bool paused = false;
            while (lm != null && lm.State == GameState.Playing)
            {
                Drive(lm);
                yield return null;
                driven += 1f / Fps;
                if (showPause && !paused && driven > 7f)
                {
                    paused = true;
                    lm.TogglePause();
                    yield return Wait(4.5f);
                    lm.TogglePause();
                }
                if (!untilFinish && driven > driveSeconds) break;
                if (untilFinish && driven > driveSeconds) break;
            }
            InputBridge.DemoThrottle = 0f;
            if (lm != null && (lm.State == GameState.Won || lm.State == GameState.Lost))
                yield return Wait(5f);      // sonuç ekranı
        }

        static float _stuck, _reverse;

        static void Drive(LevelManager lm)
        {
            var t = lm.truck;
            if (t == null || t.Body == null) return;
            float dt = 1f / Fps;
            // Takılırsa kısa bir geri manevra yapıp yeniden hızlanır
            if (_reverse > 0f) { _reverse -= dt; InputBridge.DemoThrottle = -1f; return; }
            if (t.IsGrounded && t.SpeedKmh < 2f) _stuck += dt; else _stuck = 0f;
            if (_stuck > 1.2f) { _stuck = 0f; _reverse = 0.9f; }
            float ang = Mathf.DeltaAngle(0f, t.Body.rotation);   // + burun yukarı
            float thr;
            if (t.IsGrounded)
            {
                thr = 1f;
                if (ang < -14f && t.SpeedKmh > 18f) thr = 0f;      // sert inişte yavaşla
                if (ang > 38f) thr = 0.2f;                          // şaha kalkmayı önle
            }
            else
            {
                thr = ang > 8f ? -1f : (ang < -8f ? 1f : 0.2f);     // havada dengede tut
            }
            InputBridge.DemoThrottle = thr;
        }

        void OnDestroy()
        {
            if (_out != null) { try { _out.Close(); } catch { } }
            InputBridge.Demo = false;
            Time.captureFramerate = 0;
        }
    }
}

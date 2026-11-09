using System.Collections;
using UnityEngine;

namespace VasifliSu
{
    public enum GameState { Countdown, Playing, Paused, Won, Lost }

    /// <summary>
    /// Bölüm akışı: geri sayım, oynanış, duraklatma, kazanma/kaybetme koşulları ve skor.
    /// </summary>
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager I { get; private set; }

        public int levelIndex;
        public TruckController truck;
        public WaterCargo cargo;
        public TerrainGenerator terrain;
        public HUDController hud;
        public PauseMenu pauseMenu;
        public ResultPanel resultPanel;

        public GameState State { get; private set; }
        public float ElapsedTime { get; private set; }
        public LevelData Data => LevelDatabase.I.levels[levelIndex];
        public bool IsLastLevel => levelIndex >= LevelDatabase.I.Count - 1;

        float _upsideTimer;

        void Awake()
        {
            I = this;
            Time.timeScale = 1f;
            InputBridge.Reset();
        }

        void OnDestroy() { if (I == this) I = null; }

        IEnumerator Start()
        {
            AudioManager.I.PlayMusic();
            State = GameState.Countdown;
            truck.ControlsEnabled = false;
            var a = GameAssets.I;
            for (int n = 3; n >= 1; n--)
            {
                hud.ShowCountdown(n.ToString());
                if (a) AudioManager.I.Play(a.beep, 0.7f);
                yield return new WaitForSeconds(0.65f);
            }
            hud.ShowCountdown(Loc.T("YOLA ÇIK!"));
            if (a) AudioManager.I.Play(a.go, 0.8f);
            State = GameState.Playing;
            truck.ControlsEnabled = true;
            yield return new WaitForSeconds(0.8f);
            hud.HideCountdown();
        }

        void Update()
        {
            if (InputBridge.PausePressed() && (State == GameState.Playing || State == GameState.Paused)) TogglePause();
            if (InputBridge.RestartPressed() && State != GameState.Paused) SceneFlow.Reload();
            if (State != GameState.Playing) return;

            ElapsedTime += Time.deltaTime;

            // Devrilme: ters durumda 1.5 sn kalırsa
            if (truck.transform.up.y < -0.15f) _upsideTimer += Time.deltaTime; else _upsideTimer = 0f;
            if (_upsideTimer > 1.5f) { Fail(Loc.T("Kamyon devrildi!")); return; }

            // Uçurumdan düşme
            if (truck.transform.position.y < terrain.MinHeight - 15f) { Fail(Loc.T("Kamyon yoldan çıktı!")); return; }

            // Yetersiz su
            if (cargo.RemainingRatio + 0.0001f < Data.requiredRatio)
            {
                Fail(Loc.T("Yeterli su kalmadı!") + "\n" + string.Format(Loc.T("En az %{0} teslim edilmeliydi."), Mathf.RoundToInt(Data.requiredRatio * 100)));
            }
        }

        public void TogglePause()
        {
            if (State == GameState.Playing)
            {
                State = GameState.Paused;
                Time.timeScale = 0f;
                pauseMenu.Show(true);
            }
            else if (State == GameState.Paused)
            {
                State = GameState.Playing;
                Time.timeScale = 1f;
                pauseMenu.Show(false);
            }
        }

        public void Finish()
        {
            if (State != GameState.Playing) return;
            State = GameState.Won;
            truck.ControlsEnabled = false;
            truck.Body.linearDamping = 2.5f;   // teslimat noktasında yavaşça dur
            InputBridge.Reset();

            float ratio = cargo.RemainingRatio;
            int litres = cargo.RemainingLitres;
            float t = ElapsedTime;
            int timeBonus = Mathf.Max(0, Mathf.RoundToInt((Data.parTime * 1.5f - t) * 10f));
            int score = litres * 10 + Mathf.RoundToInt(ratio * 1000f) + timeBonus;

            bool starRatio = ratio >= 0.85f;
            bool starTime = t <= Data.parTime;
            int stars = 1 + (starRatio ? 1 : 0) + (starTime ? 1 : 0);

            bool newBest = SaveSystem.SaveResult(levelIndex, stars, score, litres);
            if (GameAssets.I) AudioManager.I.Play(GameAssets.I.win, 0.9f);

            var r = new ResultPanel.Result
            {
                success = true,
                title = Loc.T("TESLİMAT TAMAM!"),
                reason = Loc.T(Data.customer) + Loc.T(" müşterileri suyuna kavuştu."),
                ratio = ratio, litres = litres, totalLitres = cargo.TotalLitres,
                time = t, parTime = Data.parTime, score = score, best = SaveSystem.GetBest(levelIndex),
                stars = stars, starRatio = starRatio, starTime = starTime, newBest = newBest
            };
            StartCoroutine(ShowResultAfter(r, 1.2f));
        }

        public void Fail(string reason)
        {
            if (State != GameState.Playing) return;
            State = GameState.Lost;
            truck.StopEngine();
            InputBridge.Reset();
            if (GameAssets.I) AudioManager.I.Play(GameAssets.I.lose, 0.9f);

            var r = new ResultPanel.Result
            {
                success = false,
                title = Loc.T("TESLİMAT BAŞARISIZ"),
                reason = reason,
                ratio = cargo.RemainingRatio, litres = cargo.RemainingLitres, totalLitres = cargo.TotalLitres,
                time = ElapsedTime, parTime = Data.parTime, score = 0, best = SaveSystem.GetBest(levelIndex)
            };
            StartCoroutine(ShowResultAfter(r, 1.4f));
        }

        IEnumerator ShowResultAfter(ResultPanel.Result r, float delay)
        {
            yield return new WaitForSeconds(delay);
            truck.StopEngine();
            resultPanel.Show(r);
        }

        public void Restart() => SceneFlow.Reload();
        public void NextLevel() { if (!IsLastLevel) SceneFlow.LoadLevel(levelIndex + 1); else SceneFlow.LoadLevelSelect(); }
        public void GoLevelSelect() => SceneFlow.LoadLevelSelect();
        public void GoMainMenu() => SceneFlow.LoadMainMenu();
    }
}

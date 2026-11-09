using System.Collections;
using UnityEngine;

namespace VasifliSu
{
    /// <summary>
    /// Kasadaki 19 L'lik damacana. Yere (GroundSurface) değerse veya kamyondan çok uzaklaşırsa
    /// "kaybedilmiş" sayılır, su sıçrar ve yavaşça kaybolur.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class Damacana : MonoBehaviour
    {
        public const int Litres = 19;

        public bool Lost { get; private set; }
        public System.Action<Damacana> onLost;

        SpriteRenderer _sr;

        void Awake() { _sr = GetComponent<SpriteRenderer>(); }

        void OnCollisionEnter2D(Collision2D c)
        {
            if (Lost) return;
            if (c.collider.GetComponent<GroundSurface>() != null) MarkLost();
        }

        public void MarkLost()
        {
            if (Lost) return;
            // Teslimat bittikten sonra (kazanma/kaybetme) kargo artık sayılmaz
            if (LevelManager.I != null && LevelManager.I.State != GameState.Playing) return;
            Lost = true;
            onLost?.Invoke(this);
            if (GameAssets.I != null) AudioManager.I.Play(GameAssets.I.splashSfx, 0.8f);
            SpawnSplash();
            StartCoroutine(FadeOut());
        }

        void SpawnSplash()
        {
            if (GameAssets.I == null || GameAssets.I.splash == null) return;
            var go = new GameObject("Splash");
            go.transform.position = transform.position + Vector3.up * 0.2f;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = GameAssets.I.splash;
            sr.sortingOrder = 30;
            go.AddComponent<SplashFx>();
        }

        IEnumerator FadeOut()
        {
            float t = 0f;
            Color c0 = _sr ? _sr.color : Color.white;
            while (t < 1.5f)
            {
                t += Time.deltaTime;
                if (_sr) _sr.color = new Color(c0.r, c0.g, c0.b, Mathf.Lerp(1f, 0f, t / 1.5f));
                yield return null;
            }
            gameObject.SetActive(false);
        }
    }
}

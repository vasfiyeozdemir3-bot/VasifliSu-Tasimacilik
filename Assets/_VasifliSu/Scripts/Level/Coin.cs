using UnityEngine;

namespace VasifliSu
{
    /// <summary>Yol üzerindeki toplanabilir Vasıflı Su coin'i. Kamyon değince toplanır, skora bonus ekler.</summary>
    [RequireComponent(typeof(CircleCollider2D))]
    public class Coin : MonoBehaviour
    {
        public const int ScoreValue = 50;

        Vector3 _basePos;
        Vector3 _baseScale;
        float _phase;
        bool _taken;
        float _popT;

        void Start()
        {
            _basePos = transform.position;
            _baseScale = transform.localScale;
            _phase = _basePos.x * 0.7f;
        }

        void Update()
        {
            if (_taken)
            {
                _popT += Time.deltaTime;
                transform.position += Vector3.up * Time.deltaTime * 4f;
                transform.localScale = _baseScale * (1f + _popT * 2f);
                var sr = GetComponent<SpriteRenderer>();
                if (sr) sr.color = new Color(1f, 1f, 1f, 1f - _popT * 3f);
                if (_popT > 0.33f) Destroy(gameObject);
                return;
            }
            float t = Time.time * 3f + _phase;
            transform.localScale = new Vector3(_baseScale.x * Mathf.Abs(Mathf.Cos(t)) + _baseScale.x * 0.08f, _baseScale.y, 1f);
            transform.position = _basePos + Vector3.up * Mathf.Sin(t * 0.7f) * 0.08f;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (_taken) return;
            if (other.GetComponentInParent<TruckController>() == null) return;
            var lm = LevelManager.I;
            if (lm == null || lm.State != GameState.Playing) return;
            _taken = true;
            GetComponent<Collider2D>().enabled = false;
            lm.AddCoin();
            if (GameAssets.I && GameAssets.I.coinSfx) AudioManager.I.Play(GameAssets.I.coinSfx, 0.7f);
        }
    }
}

using UnityEngine;

namespace VasifliSu
{
    /// <summary>Basit su sıçrama efekti: büyür ve kaybolur.</summary>
    public class SplashFx : MonoBehaviour
    {
        float _t;
        SpriteRenderer _sr;
        void Awake() { _sr = GetComponent<SpriteRenderer>(); transform.localScale = Vector3.one * 0.3f; }
        void Update()
        {
            _t += Time.deltaTime;
            float k = _t / 0.6f;
            transform.localScale = Vector3.one * Mathf.Lerp(0.3f, 1.2f, k);
            if (_sr) _sr.color = new Color(1f, 1f, 1f, 1f - k);
            if (k >= 1f) Destroy(gameObject);
        }
    }
}

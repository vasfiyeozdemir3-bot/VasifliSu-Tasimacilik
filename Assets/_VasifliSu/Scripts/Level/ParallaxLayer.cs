using UnityEngine;

namespace VasifliSu
{
    /// <summary>
    /// Sonsuz yatay parallax arka plan. SpriteRenderer "Tiled" modunda, sprite genişliğinin 3 katı olmalı.
    /// factor 1 = kamerayla birlikte hareket (çok uzak), 0 = dünyaya sabit.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class ParallaxLayer : MonoBehaviour
    {
        public Transform cam;
        [Range(0f, 1f)] public float factor = 0.8f;
        [Range(0f, 1f)] public float yFollow = 0.9f;
        public float yOffset = 0f;

        float _startX, _length;

        void Start()
        {
            if (cam == null && Camera.main != null) cam = Camera.main.transform;
            var sr = GetComponent<SpriteRenderer>();
            _length = sr.sprite != null ? sr.sprite.bounds.size.x * transform.localScale.x : 20f;
            _startX = cam != null ? cam.position.x : transform.position.x;
        }

        void LateUpdate()
        {
            if (cam == null) return;
            float temp = cam.position.x * (1f - factor);
            float dist = cam.position.x * factor;
            transform.position = new Vector3(_startX + dist, cam.position.y * yFollow + yOffset, transform.position.z);
            if (temp > _startX + _length) _startX += _length;
            else if (temp < _startX - _length) _startX -= _length;
        }
    }
}

using UnityEngine;

namespace VasifliSu
{
    /// <summary>Kamyonu yumuşakça takip eden, hızlanınca hafifçe uzaklaşan kamera.</summary>
    [RequireComponent(typeof(Camera))]
    public class CameraFollow : MonoBehaviour
    {
        public Transform target;
        public Vector2 offset = new Vector2(4f, 1.5f);
        public float smoothTime = 0.25f;
        public float baseSize = 7f;
        public float maxExtraSize = 2.5f;
        public float speedForMaxZoom = 18f;

        Camera _cam;
        Vector3 _vel;
        Rigidbody2D _rb;

        void Awake() { _cam = GetComponent<Camera>(); }

        void Start()
        {
            if (target != null)
            {
                _rb = target.GetComponent<Rigidbody2D>();
                transform.position = new Vector3(target.position.x + offset.x, target.position.y + offset.y, -10f);
            }
        }

        void LateUpdate()
        {
            if (target == null) return;
            float speed = _rb != null ? _rb.linearVelocity.magnitude : 0f;
            float look = Mathf.Clamp(_rb != null ? _rb.linearVelocity.x * 0.25f : 0f, -2f, 4f);
            var desired = new Vector3(target.position.x + offset.x + look, target.position.y + offset.y, -10f);
            transform.position = Vector3.SmoothDamp(transform.position, desired, ref _vel, smoothTime);
            float size = baseSize + maxExtraSize * Mathf.Clamp01(speed / speedForMaxZoom);
            _cam.orthographicSize = Mathf.Lerp(_cam.orthographicSize, size, Time.deltaTime * 2f);
        }
    }
}

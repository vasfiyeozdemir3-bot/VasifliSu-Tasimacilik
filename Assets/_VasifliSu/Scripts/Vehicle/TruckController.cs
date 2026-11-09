using UnityEngine;

namespace VasifliSu
{
    /// <summary>
    /// Vasıflı Su su kamyonu. İki WheelJoint2D ile tekerlekleri gövdeye bağlar:
    ///  - Motor (JointMotor2D): gaz/fren girdisine göre tekerlek dönüş hızı ve torku
    ///  - Süspansiyon (JointSuspension2D): engebeli arazide yay + sönümleme
    /// Havadayken gaz/fren gövdeyi döndürür (Hill Climb Racing tarzı hava kontrolü).
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class TruckController : MonoBehaviour
    {
        [Header("WheelJoint2D Bağlantıları")]
        public WheelJoint2D rearJoint;
        public WheelJoint2D frontJoint;
        public Rigidbody2D rearWheel;
        public Rigidbody2D frontWheel;

        [Header("Motor")]
        [Tooltip("İleri yönde maksimum tekerlek hızı (derece/sn)")]
        public float maxMotorSpeed = 1400f;
        [Tooltip("Geri yönde maksimum tekerlek hızı (derece/sn)")]
        public float reverseMotorSpeed = 350f;
        [Tooltip("Tekerlek başına maksimum motor torku")]
        public float motorTorque = 85f;
        [Tooltip("Motor yönü ters çalışıyorsa işaretle")]
        public bool invertMotor = false;
        [Tooltip("Arka tekerlekten çekiş oranı (önden çekiş = 1 - bu değer)")]
        [Range(0f, 1f)] public float rearDriveShare = 0.6f;

        [Header("Süspansiyon")]
        public float suspensionFrequency = 4f;
        [Range(0f, 1f)] public float suspensionDamping = 0.75f;

        [Header("Hava Kontrolü")]
        public float airTorque = 14f;
        public float maxAirSpin = 220f;

        [Header("Kütle Merkezi")]
        public Vector2 centerOfMass = new Vector2(0f, -0.35f);

        [Header("Ses")]
        public AudioSource engineSource;

        [Tooltip("Azami hız (km/s) — yokuş aşağı da aşılamaz")]
        public float maxSpeedKmh = 34f;
        [Tooltip("Gazın tam güce ulaşma süresi (sn) — yumuşak kalkış")]
        public float throttleRamp = 0.6f;

        public bool ControlsEnabled { get; set; }
        public bool IsGrounded { get; private set; }
        public float Throttle { get; private set; }
        public Rigidbody2D Body { get; private set; }

        /// <summary>Anlık hız (km/s). 1 birim = 1 metre.</summary>
        public float SpeedKmh => Body == null ? 0f : Body.linearVelocity.magnitude * 3.6f;

        Collider2D _rearCol, _frontCol;
        readonly ContactPoint2D[] _contacts = new ContactPoint2D[8];

        void Awake()
        {
            Body = GetComponent<Rigidbody2D>();
            Body.centerOfMass = centerOfMass;
            if (rearWheel != null) _rearCol = rearWheel.GetComponent<Collider2D>();
            if (frontWheel != null) _frontCol = frontWheel.GetComponent<Collider2D>();
            ApplySuspension(rearJoint);
            ApplySuspension(frontJoint);
            IgnoreWheelCargoCollisions();
        }

        void ApplySuspension(WheelJoint2D j)
        {
            if (j == null) return;
            var s = j.suspension;
            s.frequency = suspensionFrequency;
            s.dampingRatio = suspensionDamping;
            s.angle = 90f;
            j.suspension = s;
        }

        /// <summary>Damacanalar tekerleklere takılmasın.</summary>
        void IgnoreWheelCargoCollisions()
        {
            var cargo = GetComponentsInChildren<Damacana>(true);
            foreach (var d in cargo)
            {
                var dc = d.GetComponent<Collider2D>();
                if (dc == null) continue;
                if (_rearCol) Physics2D.IgnoreCollision(dc, _rearCol);
                if (_frontCol) Physics2D.IgnoreCollision(dc, _frontCol);
            }
        }

        void Update()
        {
            float target = ControlsEnabled ? InputBridge.Throttle() : 0f;
            // Gaz kademeli artar, bırakınca hızlı düşer
            float rate = Mathf.Abs(target) > Mathf.Abs(Throttle) ? 1f / Mathf.Max(0.01f, throttleRamp) : 4f;
            Throttle = Mathf.MoveTowards(Throttle, target, rate * Time.deltaTime);
            UpdateEngineSound();
        }

        void FixedUpdate()
        {
            IsGrounded = TouchingGround(_rearCol) || TouchingGround(_frontCol);
            float t = Throttle;

            if (Mathf.Abs(t) > 0.01f)
            {
                float target = t > 0f ? maxMotorSpeed * t : reverseMotorSpeed * t;
                // WheelJoint2D'de negatif motor hızı tekerleği saat yönünde döndürür = sağa (ileri) gider.
                float speed = invertMotor ? target : -target;
                SetMotor(rearJoint, speed, motorTorque * 2f * rearDriveShare);
                SetMotor(frontJoint, speed, motorTorque * 2f * (1f - rearDriveShare));
            }
            else
            {
                if (rearJoint) rearJoint.useMotor = false;
                if (frontJoint) frontJoint.useMotor = false;
            }

            // Hız sınırı
            float maxV = maxSpeedKmh / 3.6f;
            float v = Body.linearVelocity.magnitude;
            if (v > maxV) Body.linearVelocity = Body.linearVelocity * Mathf.Lerp(1f, maxV / v, 0.5f);

            // Hava kontrolü: gaz = burun yukarı, fren = burun aşağı
            if (!IsGrounded && Mathf.Abs(t) > 0.01f && Mathf.Abs(Body.angularVelocity) < maxAirSpin)
                Body.AddTorque(t * airTorque);
        }

        static void SetMotor(WheelJoint2D j, float speed, float torque)
        {
            if (j == null) return;
            j.motor = new JointMotor2D { motorSpeed = speed, maxMotorTorque = torque };
            j.useMotor = true;
        }

        bool TouchingGround(Collider2D c)
        {
            if (c == null) return false;
            int n = c.GetContacts(_contacts);
            for (int i = 0; i < n; i++)
            {
                var other = _contacts[i].collider;
                if (other != null && other.GetComponent<GroundSurface>() != null) return true;
            }
            return false;
        }

        void UpdateEngineSound()
        {
            if (engineSource == null) return;
            if (engineSource.clip == null && GameAssets.I != null) engineSource.clip = GameAssets.I.engine;
            if (engineSource.clip != null && !engineSource.isPlaying && Time.timeScale > 0f) engineSource.Play();
            if (Time.timeScale == 0f) { if (engineSource.isPlaying) engineSource.Pause(); return; }

            float wheelSpin = rearWheel != null ? Mathf.Abs(rearWheel.angularVelocity) / Mathf.Max(1f, maxMotorSpeed) : 0f;
            float load = Mathf.Abs(Throttle);
            engineSource.pitch = Mathf.Lerp(engineSource.pitch, 0.7f + wheelSpin * 0.9f + load * 0.2f, Time.deltaTime * 6f);
            engineSource.volume = SaveSystem.SfxVolume * (0.18f + load * 0.22f) * (ControlsEnabled ? 1f : 0.5f);
        }

        public void StopEngine()
        {
            ControlsEnabled = false;
            if (engineSource) engineSource.Stop();
        }
    }
}

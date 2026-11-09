using System.Collections.Generic;
using UnityEngine;

namespace VasifliSu
{
    /// <summary>
    /// Kamyon kasasındaki su yükünü takip eder: kalan damacana, litre ve oran.
    /// </summary>
    public class WaterCargo : MonoBehaviour
    {
        [Tooltip("Kamyondan bu kadar uzaklaşan damacana kaybedilmiş sayılır")]
        public float maxDistance = 5f;
        public Transform truckRoot;

        readonly List<Damacana> _items = new List<Damacana>();
        public event System.Action Changed;

        public int TotalCount => _items.Count;
        public int RemainingCount { get; private set; }
        public int TotalLitres => TotalCount * Damacana.Litres;
        public int RemainingLitres => RemainingCount * Damacana.Litres;
        public float RemainingRatio => TotalCount == 0 ? 0f : (float)RemainingCount / TotalCount;

        void Awake()
        {
            if (truckRoot == null) truckRoot = transform;
            _items.Clear();
            _items.AddRange(GetComponentsInChildren<Damacana>(true));
            foreach (var d in _items) d.onLost += OnLost;
            RemainingCount = _items.Count;
        }

        void Update()
        {
            for (int i = 0; i < _items.Count; i++)
            {
                var d = _items[i];
                if (d == null || d.Lost) continue;
                if (Vector2.Distance(d.transform.position, truckRoot.position) > maxDistance) d.MarkLost();
            }
        }

        void OnLost(Damacana d)
        {
            RemainingCount = Mathf.Max(0, RemainingCount - 1);
            Changed?.Invoke();
        }
    }
}

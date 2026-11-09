using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace VasifliSu
{
    /// <summary>Ekrandaki gaz / fren pedalı (dokunmatik ve fare ile basılı tutulur).</summary>
    public class HoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        public bool isGas = true;
        Image _img;
        Color _base;
        void Awake() { _img = GetComponent<Image>(); if (_img) _base = _img.color; }

        void Set(bool v)
        {
            if (isGas) InputBridge.GasHeld = v; else InputBridge.BrakeHeld = v;
            transform.localScale = Vector3.one * (v ? 0.92f : 1f);
            if (_img) _img.color = v ? new Color(_base.r * 0.75f, _base.g * 0.75f, _base.b * 0.75f, 1f) : _base;
        }

        public void OnPointerDown(PointerEventData e) => Set(true);
        public void OnPointerUp(PointerEventData e) => Set(false);
        public void OnPointerExit(PointerEventData e) => Set(false);
        void OnDisable() => Set(false);
    }
}

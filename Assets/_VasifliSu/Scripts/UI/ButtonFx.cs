using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace VasifliSu
{
    /// <summary>Butonlara "su damlası" hissi: üzerine gelince büyüme, basınca sıkışma, tıklama sesi.</summary>
    public class ButtonFx : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
    {
        public float hoverScale = 1.06f;
        public float pressScale = 0.94f;

        Vector3 _base = Vector3.one;
        float _target = 1f;
        Selectable _sel;

        void Awake() { _base = transform.localScale; _sel = GetComponent<Selectable>(); }
        void OnDisable() { _target = 1f; transform.localScale = _base; }

        bool Active => _sel == null || _sel.interactable;

        public void OnPointerEnter(PointerEventData e) { if (Active) _target = hoverScale; }
        public void OnPointerExit(PointerEventData e) { _target = 1f; }
        public void OnPointerDown(PointerEventData e) { if (Active) _target = pressScale; }
        public void OnPointerUp(PointerEventData e) { _target = 1f; }
        public void OnPointerClick(PointerEventData e) { if (Active) AudioManager.Click(); }

        void Update()
        {
            transform.localScale = Vector3.Lerp(transform.localScale, _base * _target, Time.unscaledDeltaTime * 14f);
        }
    }
}

using UnityEngine;

namespace VasifliSu
{
    /// <summary>Statik arayüz metinlerini dil değişince günceller.</summary>
    [RequireComponent(typeof(UnityEngine.UI.Text))]
    public class LocalizedText : MonoBehaviour
    {
        public string key;
        UnityEngine.UI.Text _t;

        void Awake()
        {
            _t = GetComponent<UnityEngine.UI.Text>();
            if (string.IsNullOrEmpty(key)) key = _t.text;
        }

        void OnEnable() { Loc.Changed += Apply; Apply(); }
        void OnDisable() { Loc.Changed -= Apply; }
        void Apply() { if (_t != null) _t.text = Loc.T(key); }
    }
}

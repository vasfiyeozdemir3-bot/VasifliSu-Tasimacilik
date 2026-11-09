using UnityEngine;

namespace VasifliSu
{
    /// <summary>Teslimat noktası tetikleyicisi. Kamyon girince bölüm tamamlanır.</summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class FinishLine : MonoBehaviour
    {
        void OnTriggerEnter2D(Collider2D other)
        {
            var rb = other.attachedRigidbody;
            if (rb == null || rb.GetComponent<TruckController>() == null) return;
            if (LevelManager.I != null) LevelManager.I.Finish();
        }
    }
}

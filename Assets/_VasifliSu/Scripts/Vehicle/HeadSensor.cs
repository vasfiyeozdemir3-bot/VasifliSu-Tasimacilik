using UnityEngine;

namespace VasifliSu
{
    /// <summary>
    /// Kabin tavanındaki tetikleyici. Zemine değerse kamyon devrilmiş demektir.
    /// </summary>
    public class HeadSensor : MonoBehaviour
    {
        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponent<GroundSurface>() == null) return;
            if (LevelManager.I != null) LevelManager.I.Fail(Loc.T("Kamyon devrildi!"));
        }
    }
}

using UnityEngine;

namespace VasifliSu
{
    /// <summary>
    /// Zemin işareti. Arazi ve kayalarda bulunur; tekerlek temas kontrolü,
    /// devrilme sensörü ve damacana kaybı bu bileşene bakar.
    /// </summary>
    public class GroundSurface : MonoBehaviour {
        /// <summary>Engel/kaya ise true: damacana buna sürtününce kaybolmuş sayılmaz.</summary>
        public bool isObstacle;
 }
}

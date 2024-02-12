using UnityEngine;
using UnityEngine.Serialization;

namespace Systems.Vision
{
    public class VisionConeController : MonoBehaviour
    {
        [Header("Cone settings")]
        [FormerlySerializedAs("FOV")][Range(1,360)] 
        public float fov = 90;
        public float viewDistance = 3.5f;
        [Header("Cone visual")]
        public Material visionDecalMaterial;
        public GameObject visionDecal;
    
        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        /// <param name="worldSpace">Coordinates in world space</param>
        /// <returns>Coordinates in grid space</returns>
        private Vector3 TransformCoordinates(Vector3 worldSpace)
        {
            return new Vector3(worldSpace.x, worldSpace.y * 2, worldSpace.z);
        }
    }
}

using UnityEngine;

namespace Systems.Ability.Aiming
{
    public class WorldPositionAimingController : MonoBehaviour
    {
        public float aimDistance = 3.5f;
        public GameObject owner;
        public Vector3 currentTarget;

        [Header("Collisions")] 
        public bool collide = false;
        public LayerMask visionMask;

        [Header("Visual")] 
        public GameObject worldReferenceParticle;
        public GameObject cursorParticle;
        
        private Camera _camera;
        private bool _isCameraNotNull;
        
        // Start is called before the first frame update
        void Start()
        {
            _camera = Camera.main;
            _isCameraNotNull = _camera != null;
        }

        // Update is called once per frame
        void Update()
        {
            if(!_isCameraNotNull) return;
            Vector3 mousePositionWS = _camera.ScreenToWorldPoint(Input.mousePosition);
            mousePositionWS.z = 0.0f;
            Vector3 delta = mousePositionWS - owner.transform.position ;
            Vector3 deltaNormalized = delta.normalized;
            
            //todo aim distance to grid space distance.
            
            if (delta.magnitude > aimDistance)
                delta = deltaNormalized * aimDistance;

            if (collide)
            {
                RaycastHit2D hit = Physics2D.Raycast(owner.transform.position, deltaNormalized, delta.magnitude, visionMask);
                if (hit.collider != null)
                {
                    delta = deltaNormalized * hit.distance;
                }
            }
            Debug.DrawLine(owner.transform.position, mousePositionWS, Color.red);

            currentTarget = owner.transform.position + delta;
            transform.position = currentTarget;
            cursorParticle.transform.localPosition = mousePositionWS - transform.position;
        }
    }
}

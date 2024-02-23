using UnityEngine;

namespace Systems.Ability.Aiming
{
    public class WorldPositionAimingController : MonoBehaviour
    {
        public float aimDistance = 3.5f;
        public GameObject owner;
        public Vector3 currentTarget;
        private Vector3 _lastValidTarget;

        [Header("Collisions")] 
        public bool blockVisionToWall = false;
        public bool canGoInAnotherRoom = false;
        public LayerMask visionMask;
        public LayerMask floorMask;
        public float collisionRadius = 0.25f;

        public RoomData currentRoom;


        [Header("Visual")] 
        public GameObject worldReferenceParticle;
        public GameObject cursorParticle;

        public RaycastHit2D hit;
        
        private Camera _camera;
        private bool _isCameraNotNull;
        
        // Start is called before the first frame update
        void Start()
        {
            _camera = Camera.main;
            _isCameraNotNull = _camera != null;

            _lastValidTarget = owner.transform.position;
            currentRoom = PlayerState.GetInstance().currentRoom;
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

            if(!blockVisionToWall)
                hit = Physics2D.CircleCast(owner.transform.position + delta, collisionRadius, Vector2.zero, 0,visionMask);

            RaycastHit2D floor = Physics2D.CircleCast(owner.transform.position + delta, 0.001f, Vector2.zero, 0, floorMask);
            RoomData cRoom = null;
            bool isAnotherRoom = false;
            if (floor.collider != null)
            {
                cRoom = floor.collider.GetComponentInParent<RoomData>();
                isAnotherRoom = !cRoom.name.Equals(PlayerState.GetInstance().currentRoom.name);
            }


            if (blockVisionToWall)
            {
                hit = Physics2D.Raycast(owner.transform.position, deltaNormalized, delta.magnitude, visionMask);
                if (hit.collider != null)
                {
                    delta = deltaNormalized * hit.distance;
                }
            }

            if (blockVisionToWall || hit.collider == null && (floor.collider != null && (!isAnotherRoom || canGoInAnotherRoom)))
            {
                currentTarget = owner.transform.position + delta;
                _lastValidTarget = currentTarget;
                currentRoom = cRoom;
            }
            else
            {
                currentTarget = _lastValidTarget;
            }
            transform.position = currentTarget;
            cursorParticle.transform.localPosition = mousePositionWS - transform.position;
        }
    }
}

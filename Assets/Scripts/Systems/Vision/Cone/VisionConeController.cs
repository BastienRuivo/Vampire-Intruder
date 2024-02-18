using DefaultNamespace;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Systems.Vision.Cone
{
    public class VisionConeController : VisionShapeController
    {
        public override bool HasVisibility(Vector3 target)
        {
            Vector3 conePosition = transform.position;
            Vector3 delta = target - conePosition;
            
            //Compute target angle world space
            float angleWP = Tools.ComputeAngle(conePosition, target);

            //Transform target angle to grid space
            Vector2 angleWPVec = Tools.RotateVector(new Vector2(1,0), angleWP);
            Vector2 angleLocalVec = Tools.GridToWorldCoordinates(angleWPVec);
            float angle = Tools.ComputeAngle(new Vector2(0,0), angleLocalVec);
            
            //Compute a viewDistance vector transformed in grid space.
            Vector2 distVec = Tools.RotateVector(new Vector2(1,0) * viewDistance, angle + 2*Mathf.PI);
            Vector2 localViewDistanceDir = Tools.WorldToGridCoordinates(distVec);
            
            //Target distance check
            if (delta.magnitude > localViewDistanceDir.magnitude)
                return false;

            //Compute cone definition angle in world space
            float coneAngleMinLocal = Tools.ComputeAngle(new Vector2(0,0),
                Tools.WorldToGridCoordinates(Tools.RotateVector(new Vector2(1,0), _coneAngleMin))
                );
            float coneAngleMaxLocal = Tools.ComputeAngle(new Vector2(0,0),
                Tools.WorldToGridCoordinates(Tools.RotateVector(new Vector2(1,0), _coneAngleMin + _coneAngle))
            );

            //Cone angle inclusion check (taking modulo operator in account).
            if (coneAngleMinLocal > coneAngleMaxLocal && angleWP >= coneAngleMinLocal - 2*Mathf.PI && angleWP <= coneAngleMaxLocal) return true;
            if (angleWP >= coneAngleMinLocal && angleWP <= coneAngleMaxLocal) return true;
            if (coneAngleMinLocal > coneAngleMaxLocal && angleWP >= coneAngleMinLocal && angleWP <= coneAngleMaxLocal + 2*Mathf.PI) return true;
            
            return false;
        }
        
        public override bool HasRefreshability(Vector3 target)
        {
            Vector3 conePosition = transform.position;
            Vector3 direction = target - conePosition;
            
            //Compute target angle world space
            float angleWP = Tools.ComputeAngle(conePosition, target);

            //Transform target angle to grid space
            Vector2 angleWPVec = Tools.RotateVector(new Vector2(1,0), angleWP);
            Vector2 angleLocalVec = Tools.GridToWorldCoordinates(angleWPVec);
            float angle = Tools.ComputeAngle(new Vector2(0,0), angleLocalVec);
            
            //Compute a refreshDistance vector transformed in grid space.
            Vector2 distVec = Tools.RotateVector(new Vector2(1,0) * refreshDistance, angle + 2*Mathf.PI);
            Vector2 localRefreshDistanceDir = Tools.WorldToGridCoordinates(distVec);
            
            //Target distance check
            if (direction.magnitude > localRefreshDistanceDir.magnitude)
                return false;
            
            //Direct visibility check
            float traceDistance = Vector3.Distance(target, conePosition);
            RaycastHit2D hit = Physics2D.Raycast(conePosition, direction, traceDistance, visionCollisionLayerMask);
            
            return hit.collider == null;
        }
        
        public override void Enable()
        {
            if(enabled) return;
            enabled = true;
            _isEnabledMaterialSetting.RetargetValue(1.0f);
        }
        
        public override void Disable()
        {
            if(! enabled) return;
            enabled = false;
            _isEnabledMaterialSetting.RetargetValue(0.0f);
        }
        
        public override bool IsEnabled()
        {
            return enabled;
        }
        
        /// <summary>
        /// Expose the sprite material of this cone so that you can change settings in.
        /// </summary>
        /// <returns>Sprite material reference.</returns>
        public Material GetMaterial()
        {
            return _visionDecalMaterial;
        }

        public float GetViewMinAngle()
        {
            return _coneAngleMin;
        }

        public float GetViewAngle()
        {
            return _coneAngle;
        }

        public Texture2D GetDepthMap()
        {
            return _linearDepthMap;
        }
        
        private void Awake()
        {
            _isEnabledMaterialSetting = new SmoothScalarValue(enabled ? 1.0f : 0.0f, 0.25f);
            if(visionDecal.GetComponent<SpriteRenderer>() != null)
                _visionDecalMaterial = visionDecal.GetComponent<SpriteRenderer>().material;
            if(visionDecal.GetComponent<Image>() != null)
                _visionDecalMaterial = visionDecal.GetComponent<Image>().material;
            
            _linearDepthMap = new Texture2D((int)traceCount, 1, TextureFormat.RFloat, false);
            _depthMapData = new Color[traceCount] ;
            for (var i = 0; i < _depthMapData.Length; i++)
            {
                _depthMapData[i] = new Color(0,0,0);
            }
            _linearDepthMap.SetPixels(_depthMapData);
            _linearDepthMap.Apply();
            _visionDecalMaterial.SetTexture(ShadowMap, _linearDepthMap);
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (enabled)
            {
                ComputeAngles();
                UpdateShadowMap();
            }
            _visionDecalMaterial.SetVector(ObserverPosition, transform.position);
            _visionDecalMaterial.SetFloat(ObserverMinAngle, _coneAngleMin);
            _visionDecalMaterial.SetFloat(ObserverViewDistance, viewDistance);
            _visionDecalMaterial.SetFloat(ObserverFieldOfView, _coneAngle);
            _visionDecalMaterial.SetFloat(Visibility, _isEnabledMaterialSetting.UpdateGetValue());
        }

        /// <summary>
        /// Refresh the angles that defines the cone in world space.
        /// </summary>
        private void ComputeAngles()
        {
            _coneAngle = fov * Mathf.Deg2Rad;
            _coneAngleMin = 3 * Mathf.PI / 2 + ((gameObject.transform.rotation.eulerAngles.z % 360)  *  Mathf.Deg2Rad) - _coneAngle / 2;
            _firstDir = Tools.RotateVector(new Vector2(1,0), _coneAngleMin + _coneAngle);
        }
        
        /// <summary>
        /// Update the shadow/depth map of the visibility cone.
        /// </summary>
        private void UpdateShadowMap()
        {
            float step = _coneAngle / (float)(traceCount - 1.0f);
            float angle = 0.0f;
            Vector3 guardGridPosition = transform.position;

            //Ray casting
            for (uint i = 0; i < traceCount; i++)
            {
                Vector2 dir2D = Tools.RotateVector(_firstDir, -angle);
                dir2D.Normalize();
                dir2D *= viewDistance;
                Vector2 localDirDist2D = Tools.WorldToGridCoordinates(dir2D);
                float localDist = localDirDist2D.magnitude;
                RaycastHit2D hit = Physics2D.Raycast(guardGridPosition, localDirDist2D, localDist, visionCollisionLayerMask);
               
                //Store depth
                if (hit.collider == null)
                {
                    //depth max
                    _depthMapData[i].r = 1.0f;
                    _depthMapData[i].b = 1.0f;
                    _depthMapData[i].g = 1.0f;
                }
                else
                {
                    float gridDist = hit.distance / localDist;
                    _depthMapData[i].r = gridDist;
                    _depthMapData[i].b = gridDist;
                    _depthMapData[i].g = gridDist;
                }

                angle += step;
            }

            _linearDepthMap.SetPixels(_depthMapData);
            _linearDepthMap.Apply();
        }

        [FormerlySerializedAs("Enabled")] 
        [Header("Cone settings")] public new bool enabled = true;
        [FormerlySerializedAs("FOV")]
        [Range(1,360)] public float fov = 90;
        public float viewDistance = 3.5f;
        public LayerMask visionCollisionLayerMask;
        [Header("Performance settings")] 
        [Range(1,128)] public uint traceCount = 64;
        public float refreshDistance = 3.5f;
        [Header("Cone visual")]
        public GameObject visionDecal;

        private Material _visionDecalMaterial;
        private Texture2D _linearDepthMap;
        private Color[] _depthMapData;
        
        private float _coneAngleMin;
        private float _coneAngle;
        private Vector2 _firstDir;

        private SmoothScalarValue _isEnabledMaterialSetting;
        
        //Material cached property IDs
        private static readonly int ObserverPosition = Shader.PropertyToID("_ObserverPosition");
        private static readonly int ObserverMinAngle = Shader.PropertyToID("_ObserverMinAngle");
        private static readonly int ObserverViewDistance = Shader.PropertyToID("_ObserverViewDistance");
        private static readonly int ObserverFieldOfView = Shader.PropertyToID("_ObserverFieldOfView");
        private static readonly int ShadowMap = Shader.PropertyToID("_ShadowMap");
        private static readonly int Visibility = Shader.PropertyToID("_Visibility");
    }
}

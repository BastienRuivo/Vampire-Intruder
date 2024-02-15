using System;
using DefaultNamespace;
using UnityEngine;
using UnityEngine.Serialization;

namespace Systems.Vision
{
    public class VisionConeController : MonoBehaviour
    {
        public bool HasVisibility(Vector3 target)
        {
            Vector3 conePosition = transform.position;
            Vector3 direction = target - conePosition;
            Vector3 directionGridSpace = Tools.GridToWorldCoordinates(direction);

            if (directionGridSpace.magnitude > viewDistance)
                return false;
            
            
            return false;
        }

        public bool HasRefreshability(Vector3 target)
        {
            Vector3 conePosition = transform.position;
            Vector3 direction = target - conePosition;
            Vector3 directionGridSpace = Tools.GridToWorldCoordinates(direction);

            if (directionGridSpace.magnitude > refreshDistance)
                return false;
            
            RaycastHit2D hit = Physics2D.Raycast(conePosition, direction, Vector3.Distance(target, conePosition), visionCollisionLayerMask);
            
            return hit != false;
            return false;
        }

        public void Enable()
        {
            if(enabled) return;
            enabled = true;
            isEnabledMaterial.RetargetValue(1.0f);
        }

        public void Disable()
        {
            if(! enabled) return;
            enabled = false;
            isEnabledMaterial.RetargetValue(0.0f);
        }

        public bool isEnabled()
        {
            return enabled;
        }
    
        // Start is called before the first frame update
        void Start()
        {
            _visionDecalMaterial = visionDecal.GetComponent<SpriteRenderer>().material;
            
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
            _visionDecalMaterial.SetFloat(AlertRatio, 0.0f);
            _visionDecalMaterial.SetFloat("_Visibility", isEnabledMaterial.UpdateGetValue());
        }

        private void ComputeAngles()
        {
            _coneAngle = fov * Mathf.Deg2Rad;
            _coneAngleMin = 3 * Mathf.PI / 2 + ((gameObject.transform.rotation.eulerAngles.z % 360)  *  Mathf.Deg2Rad) - _coneAngle / 2;
            _firstDir = Tools.RotateVector(new Vector2(1,0), _coneAngleMin + _coneAngle);
        }
        
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
                //Debug.DrawLine(guardGridPosition, guardGridPosition + new Vector3(localDirDist2D.x, localDirDist2D.y, 0.0f), i == 0? Color.red : Color.blue);
                
                //Store depth
                if (hit.collider == null)
                {
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

        private SmoothScalarValue isEnabledMaterial = new SmoothScalarValue(1);
        
        //Material cached property IDs
        private static readonly int ObserverPosition = Shader.PropertyToID("_ObserverPosition");
        private static readonly int ObserverMinAngle = Shader.PropertyToID("_ObserverMinAngle");
        private static readonly int ObserverViewDistance = Shader.PropertyToID("_ObserverViewDistance");
        private static readonly int ObserverFieldOfView = Shader.PropertyToID("_ObserverFieldOfView");
        private static readonly int AlertRatio = Shader.PropertyToID("_AlertRatio");
        private static readonly int ShadowMap = Shader.PropertyToID("_ShadowMap");
    }
}

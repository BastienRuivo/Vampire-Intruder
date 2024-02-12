using DefaultNamespace;
using JetBrains.Annotations;
using UnityEngine;

namespace Systems.Ability.Aiming
{
    public class MouseLockingAimeController : MonoBehaviour
    {
        public float aimLockDistance = 0.5f;
        public string targetTag = "";
        public Material cursorMaterial;
        [CanBeNull] public GameObject currentTarget = null;
        public bool isTargetValid;
        private static readonly int HasTarget = Shader.PropertyToID("_hasTarget");
        private static readonly int CanTarget = Shader.PropertyToID("_canTarget");
        
        
        private Camera _camera;
        private bool _isCameraNotNull;

        private readonly SmoothScalarValue _hasTargetMatVal = new SmoothScalarValue(0, 2.5f);
        private readonly SmoothScalarValue _canTargetMatVal = new SmoothScalarValue(0, 2.5f);

        private bool _previousIsTargetValid;
        private bool _previousHasTarget;

        private void Start()
        {
            _camera = Camera.main;
            _isCameraNotNull = _camera != null;
            _previousIsTargetValid = isTargetValid;
            _previousIsTargetValid = false;
        }

        private void Awake()
        {
            GetComponentInChildren<SpriteRenderer>().material = cursorMaterial;
        }

        private void Update()
        {
            //bind mouse to cursor
            if (_isCameraNotNull) 
                transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            var vector3 = transform.position;
            vector3.z = 0;
            transform.position = vector3;
            
            currentTarget = null;
            if(isTargetValid != _previousIsTargetValid)
                _canTargetMatVal.RetargetValue(isTargetValid ? 1.0f : 0.0f);
            _previousIsTargetValid = isTargetValid;
            
            cursorMaterial.SetFloat(HasTarget, _hasTargetMatVal.UpdateGetValue());
            cursorMaterial.SetFloat(CanTarget, _canTargetMatVal.UpdateGetValue());
            
            if(targetTag.Equals("")) return;
            foreach (GameObject target in GameObject.FindGameObjectsWithTag(targetTag))
            {
                if (Vector3.Distance(target.transform.position, transform.position) < aimLockDistance)
                {
                    transform.position = target.transform.position;
                    vector3 = transform.position;
                    vector3.y += 0.25f;
                    transform.position = vector3;
                    currentTarget = target;
                    if (_previousHasTarget == false)
                    {
                        _hasTargetMatVal.RetargetValue(1.0f);
                        _previousHasTarget = true;
                    }
                    break;
                }
            }
            if(currentTarget == null)
                if (_previousHasTarget)
                {
                    _hasTargetMatVal.RetargetValue(0.0f);
                    _previousHasTarget = false;
                }
        }
    }
}
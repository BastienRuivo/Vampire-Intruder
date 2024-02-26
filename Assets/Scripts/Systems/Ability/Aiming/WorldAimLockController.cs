using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using JetBrains.Annotations;
using UnityEngine;

namespace Systems.Ability.Aiming
{
    public class WorldAimLockController : MonoBehaviour
    {
        public float aimLockDistance = 0.5f;
        public Targetable.TargetType targetType = Targetable.TargetType.NOONE;
        public GameObject owner;
        public Material cursorMaterial;
        [CanBeNull] public GameObject currentTarget = null;
        public LayerMask visionMask;
    
        public bool isTargetValid = true;
    
        private static readonly int HasTarget = Shader.PropertyToID("_hasTarget");
        private static readonly int CanTarget = Shader.PropertyToID("_canTarget");
    
        private Camera _camera;
        private bool _isCameraNotNull;
    
        private readonly SmoothScalarValue _hasTargetMatVal = new SmoothScalarValue(0, 2.5f);
        private readonly SmoothScalarValue _canTargetMatVal = new SmoothScalarValue(0, 2.5f);
    
        private Collider2D[] _collider2DBuff;

        private bool _previousIsTargetValid;
        private bool _previousHasTarget;
    
        // Start is called before the first frame update
        void Start()
        {
            _camera = Camera.main;
            _isCameraNotNull = _camera != null;
            GetComponentInChildren<SpriteRenderer>().material = cursorMaterial;
            _collider2DBuff = new Collider2D[16]; // todo calculate right count with 
        }

        // Update is called once per frame
        void Update()
        {
            if (targetType == Targetable.TargetType.NOONE)
                return;

            if (!_isCameraNotNull)
                return;
            
            if(isTargetValid != _previousIsTargetValid)
                _canTargetMatVal.RetargetValue(isTargetValid ? 1.0f : 0.0f);
            _previousIsTargetValid = isTargetValid;

            Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPosition.z = 0;

            bool bindToMouse = false;
            int size = Physics2D.OverlapCircleNonAlloc(owner.transform.position, aimLockDistance, _collider2DBuff);
            if (size != 0)
            {
                int i = 0;
                IEnumerable<Collider2D> targets = _collider2DBuff.Where(
                    target =>
                        i++ < size &&
                        target != null &&
                        target.gameObject.CompareTag(Targetable.TargetableTag) &&
                        target.gameObject.GetComponent<Targetable>().IsVisibleByPlayer &&
                        Physics2D.Raycast(owner.transform.position, target.transform.position - owner.transform.position,
                            aimLockDistance, visionMask).collider == null
                ).OrderBy(target => Vector3.Distance(target.transform.position, mouseWorldPosition));
                
                Collider2D firstCollider = targets.FirstOrDefault();
                
                if (firstCollider == null)
                    bindToMouse = true; 
                else
                {
                    currentTarget = firstCollider.gameObject;
                    Vector3 vector3 =  currentTarget.transform.position;
                    vector3.y += 0.25f;
                    transform.position = vector3;
                    if (_previousHasTarget == false)
                    {
                        _hasTargetMatVal.RetargetValue(1.0f);
                        _previousHasTarget = true;
                    }
                }
            }

            if (bindToMouse)
            {
                currentTarget = null;
                if (Vector3.Distance(mouseWorldPosition, owner.transform.position) > aimLockDistance)
                    transform.position = owner.transform.position + (mouseWorldPosition - owner.transform.position).normalized * aimLockDistance;
                else
                    transform.position = mouseWorldPosition;
            }

            if (currentTarget == null)
                if (_previousHasTarget)
                {
                    _hasTargetMatVal.RetargetValue(0.0f);
                    _previousHasTarget = false;
                }
            
            
            cursorMaterial.SetFloat(HasTarget, _hasTargetMatVal.UpdateGetValue());
            cursorMaterial.SetFloat(CanTarget, _canTargetMatVal.UpdateGetValue());
        }
    }
}

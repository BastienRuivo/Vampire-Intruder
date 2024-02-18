using System;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using UnityEngine;

namespace Systems.Vision
{
    public class VisionSystemController : MonoBehaviour
    {
        public string targetEntityTag;
        
        [Header("performance")]
        public float refreshDistance;
        public float disabledRefreshTime = 0.5f;
        public bool keepEnableWhenIsNotToRefresh = false;
        
        private float _updateTimer = 0.0f;
        private VisionShapeController _shapeController;

        private Collider2D[] _collider2DBuff;
        private HashSet<GameObject> _inCollisionObjects = new ();

        public readonly EventDispatcher<OverlapData> OnOverlapChanged = new EventDispatcher<OverlapData>();
        public struct OverlapData
        {
            public readonly GameObject Target;
            
            /// <summary>
            /// true if event was triggered on begin overlap, false if it was triggered on overlap ended
            /// </summary>
            public readonly bool BeginOverlap;

            public OverlapData(GameObject target, bool beginOverlap)
            {
                Target = target;
                BeginOverlap = beginOverlap;
            }
        }

        private void Awake()
        {
            _shapeController = GetComponent<VisionShapeController>();

            _collider2DBuff = new Collider2D[128]; // todo calculate right count with 
        }

        private void Update()
        {
            if (!_shapeController.IsEnabled())
            {
                _updateTimer += Time.deltaTime;
                if (_updateTimer > disabledRefreshTime)
                    _updateTimer -= disabledRefreshTime;
                else
                    return;
            }
            
            int size = Physics2D.OverlapCircleNonAlloc(transform.position, refreshDistance, _collider2DBuff);
            if(size == 0) return;

            bool shouldRefresh = false;
            IEnumerable<Collider2D> targets = _collider2DBuff.Where(target => target != null && target.gameObject.CompareTag(targetEntityTag));
            foreach (Collider2D target in targets)
            {
                bool hadKnown = _inCollisionObjects.Contains(target.gameObject);
                
                Vector3 transformPosition = target.transform.position;
                bool targetRefreshable = _shapeController.HasRefreshability(transformPosition);
                if(!targetRefreshable)
                    if (hadKnown)
                    {
                        _inCollisionObjects.Remove(target.gameObject);
                        OnOverlapChanged.BroadcastEvent(new (target.gameObject, false));
                    }
                    else
                        continue;
                
                shouldRefresh = true;
                bool hasCollision = _shapeController.HasVisibility(transformPosition);
                if (hadKnown)
                {
                    if (!hasCollision)
                    {
                        _inCollisionObjects.Remove(target.gameObject);
                        OnOverlapChanged.BroadcastEvent(new (target.gameObject, false));
                    }
                }
                else
                {
                    if (hasCollision)
                    {
                        _inCollisionObjects.Add(target.gameObject);
                        OnOverlapChanged.BroadcastEvent(new (target.gameObject, true));
                    }
                }
            }
                
            if(shouldRefresh)
                _shapeController.Enable();
            else if(!keepEnableWhenIsNotToRefresh)
                _shapeController.Disable();
        }
    }
}
using UnityEngine;

namespace Systems.Vision.Cone
{
    public class CameraInputToVisionSystemBehavior : InputToVisionSystemBehavior
    {
        private Camera _camera;

        private void Awake()
        {
            _camera = Camera.main;
        }

        private void Update()
        {
            Vector3 characterPosition = transform.position;
            Vector3 mousePosition = _camera.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0f;
            Vector3 direction = mousePosition - characterPosition;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            visionSystemObject.transform.localRotation = Quaternion.Euler(0,0, angle + 90);
        }
    }
}
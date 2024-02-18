using UnityEngine;

namespace Systems.Vision.Cone
{
    public class KeyboardInputToVisionSystemBehavior : InputToVisionSystemBehavior
    {
        private PlayerController _playerController;

        private void Awake()
        {
            _playerController = GetComponent<PlayerController>();
        }

        private void Update()
        {
            visionSystemObject.transform.localRotation = Quaternion.Euler(0,0,DirectionHelper.AngleDeg(_playerController.directionPerso));
        }
    }
}
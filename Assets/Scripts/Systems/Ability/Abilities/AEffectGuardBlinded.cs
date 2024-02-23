using System.Collections;
using UnityEngine;

namespace Systems.Ability.Abilities
{
    public class AEffectGuardBlinded : Ability
    {
        private const float BlindnessTime = 5.0f;
        
        public override IEnumerator OnAbilityTriggered(GameObject avatar)
        {
            GuardManager guardController = avatar.GetComponent<GuardManager>();
            float baseFOV = guardController.GetVision().fov;
            float baseViewDistance = guardController.GetVision().viewDistance;
            guardController.GetVision().fov = 70;
            guardController.GetVision().viewDistance = 1.5f;
            
            yield return new WaitForSeconds(BlindnessTime);
            
            guardController.GetVision().fov = baseFOV;
            guardController.GetVision().viewDistance = baseViewDistance;
        }

        public override bool ShouldAbilityTrigger(GameObject avatar)
        {
            throw new System.NotImplementedException();
        }
    }
}
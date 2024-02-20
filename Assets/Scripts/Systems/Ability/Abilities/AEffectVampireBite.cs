using System.Collections;
using UnityEngine;

namespace Systems.Ability.Abilities
{
    public class AEffectVampireBite : Ability
    {
        public AEffectVampireBite()
        {
            //Add the ability stat costs here.
            AbilityCosts.Add("Blood", -25.0f); //todo balance
        }
        
        public override IEnumerator OnAbilityTriggered(GameObject avatar)
        {
            //Debug.Log("Vampire is draining blood...");
            
            //todo play animation
            
            //Lock Input
            LockInput(avatar);
            
            //wait for a period of time;
            yield return new WaitForSeconds(4);
            
            //Debug.Log("Vampire is done drinking...");

        }

        public override bool ShouldAbilityTrigger(GameObject avatar)
        {
            //implementation required only when SelfTriggeringAbility = true
            throw new System.NotImplementedException();
        }
    }
}
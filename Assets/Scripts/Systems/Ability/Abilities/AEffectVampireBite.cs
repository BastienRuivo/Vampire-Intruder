using System.Collections;
using UnityEngine;

namespace Systems.Ability.Abilities
{

    public class AEffectVampireBite : Ability
    {
        public static readonly float eatTime = 4f;
        public AEffectVampireBite()
        {
            //Add the ability stat costs here.
            AbilityCosts.Add("Blood", -25.0f); //todo balance
            AppState.getInstance().guardKilledInCurrentScene++;
        }
        
        public override IEnumerator OnAbilityTriggered(GameObject avatar)
        {
            //Debug.Log("Vampire is draining blood...");
            PlayerController player = avatar.GetComponent<PlayerController>();

            //todo play animation

            //Lock Input
            LockInput(avatar);
            
            //wait for a period of time;
            yield return new WaitForSeconds(eatTime);

            player.UnlockVision();
            
            //Debug.Log("Vampire is done drinking...");

        }

        public override bool ShouldAbilityTrigger(GameObject avatar)
        {
            //implementation required only when SelfTriggeringAbility = true
            throw new System.NotImplementedException();
        }
    }
}
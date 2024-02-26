using System.Collections;
using UnityEngine;

namespace Systems.Ability.Abilities
{
    public class AEffectVampireThirst : Ability
    {
        public AEffectVampireThirst()
        {
            //Specify if ability triggers itself on a specific context
            SelfTriggeringAbility = true; //false by default.
            
            //Specify the ability cooldown.
            Cooldown = 20.0f; //too much time, anyways games end when this ability is trigger so whatever.
            //todo ensure of pause system
        }
        public override IEnumerator OnAbilityTriggered(GameObject avatar)
        {
            //call the endgame
            //todo call the endgame from :
            //GameController.GetGameMode().
            yield return new WaitForSeconds(Cooldown);
            if(GetAbilitySystemComponent(avatar).QueryStat("Blood") <= 0)
            {
                GameController.GetGameMode().GetDesiccated();
            }
            yield return null; //at the end if ability does run on a single tick (no yield return before).
        }

        public override bool ShouldAbilityTrigger(GameObject avatar)
        {
            return GetAbilitySystemComponent(avatar).QueryStat("Blood") <= 0;
        }
    }
}
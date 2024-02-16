using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Systems.Ability.Abilities
{
    public class AEffectStatClamp : Ability
    {
        public AEffectStatClamp()
        {
            //Specify if ability triggers itself on a specific context
            SelfTriggeringAbility = true; //false by default.
            
            //Specify the ability cooldown.
            Cooldown = 0.1f; //-1 aka cooldown disabled by default.
        }
        
        public override IEnumerator OnAbilityTriggered(GameObject avatar)
        {
            AbilitySystemComponent asc = GetAbilitySystemComponent(avatar);
            asc.ApplyEffect(this, new KeyValuePair<string, float>("Blood",
                - (asc.QueryStat("Blood") - asc.QueryStat("BloodMax"))
                ));
            yield break;
        }

        public override bool ShouldAbilityTrigger(GameObject avatar)
        {
            AbilitySystemComponent asc = GetAbilitySystemComponent(avatar);
            return asc.QueryStat("Blood") > asc.QueryStat("BloodMax");
        }
    }
}
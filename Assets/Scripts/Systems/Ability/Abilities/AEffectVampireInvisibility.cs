using System;
using System.Collections;
using DefaultNamespace;
using Unity.VisualScripting;
using UnityEngine;

namespace Systems.Ability.Abilities
{
    public class AEffectVampireInvisibility : Ability
    {
        public AEffectVampireInvisibility()
        {
            //Specify if ability is a consumable object.
            //ConsumableAbility = true; //false by default.
            
            //Specify if ability triggers itself on a specific context
            //SelfTriggeringAbility = true; //false by default.
            
            //Specify if ability's costs should be refunded in case of the ability being canceled
            //RefundOnCancel = false; //true by default.

            //Specify the ability cooldown.
            //Cooldown = 20.0f; //-1 aka cooldown disabled by default.
            
            //Add the ability stat costs here.
            //AbilityCosts.Add("Blood", 25.0f);

            //Specify if costs should automatically applied by the ability when triggered
            //ApplyCostsOnTrigger = false;  //true by default, abilities costs will be apply at the end instead,
                                            //or manually using the CommitAbility() function.
                                            
        }

        public override IEnumerator OnAbilityTriggered(GameObject avatar)
        {
            SpriteRenderer playerRenderer = avatar.GetComponent<SpriteRenderer>();
            Color invisibilityColor = new Color(0.7f, 0.7f, 1, 0.5f);
            Color defaultColor = playerRenderer.color;
  
            avatar.GetComponent<Targetable>().targetType = Targetable.TargetType.NOONE;
            playerRenderer.color = invisibilityColor;
            yield return new WaitForSeconds(15f);
            
            //todo spawn particle at the 5 lasts seconds to indicate it ends
            
            avatar.GetComponent<Targetable>().targetType = Targetable.TargetType.PLAYER;
            playerRenderer.color = Color.white;
        }

        public override bool ShouldAbilityTrigger(GameObject avatar)
        {
            throw new System.NotImplementedException();
        }
    }
}
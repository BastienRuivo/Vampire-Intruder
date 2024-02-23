using System.Collections;
using UnityEngine;

namespace Systems.Ability.Abilities
{
    public class AVampireInvisibility : Ability
    {
        public AVampireInvisibility()
        {
            //Specify if ability is a consumable object.
            //ConsumableAbility = true; //false by default.
            
            //Specify if ability triggers itself on a specific context
            //SelfTriggeringAbility = true; //false by default.
            
            //Specify if ability's costs should be refunded in case of the ability being canceled
            //RefundOnCancel = false; //true by default.

            //Specify the ability cooldown.
            Cooldown = 20.0f; //-1 aka cooldown disabled by default.
            
            //Add the ability stat costs here.
            AbilityCosts.Add("Blood", 25.0f);

            //Specify if costs should automatically applied by the ability when triggered
            //ApplyCostsOnTrigger = false;  //true by default, abilities costs will be apply at the end instead,
                                            //or manually using the CommitAbility() function.
                                          
            IconPath = "Graphics/Sprite/UI/T_AbilityIcon_Invisibility";
            UIName = "Invisibilité";
            AbilityDescription = "Disparaissez complètement du regard des autres pendant 15 secondes et devenez" +
                                 " indétectable avec Invisibilité.";
        }

        public override IEnumerator OnAbilityTriggered(GameObject avatar)
        {
            avatar.GetComponent<Targetable>().targetType = Targetable.TargetType.NOONE;
            avatar.GetComponent<SpriteRenderer>().color = new Color(0.7f, 0.7f, 1, 0.5f);
            yield return new WaitForSeconds(15f);
            avatar.GetComponent<Targetable>().targetType = Targetable.TargetType.PLAYER;
            avatar.GetComponent<SpriteRenderer>().color = Color.white;
        }

        public override bool ShouldAbilityTrigger(GameObject avatar)
        {
            throw new System.NotImplementedException();
        }
    }
}
using System.Collections;
using UnityEngine;

namespace Systems.Ability.Abilities
{
    public class AConsumableBloodPocket :  Ability
    {
        public AConsumableBloodPocket()
        {
            //Specify if ability is a consumable object.
            ConsumableAbility = true; //false by default.
            
            //Specify if ability triggers itself on a specific context
            //SelfTriggeringAbility = true; //false by default.
            
            //Specify if ability's costs should be refunded in case of the ability being canceled
            //RefundOnCancel = false; //true by default.

            //Specify the ability cooldown.
            //Cooldown = 1.0f; //-1 aka cooldown disabled by default.
            
            //Add the ability stat costs here.
            AbilityCosts.Add("Blood", +50.0f);

            //Specify if costs should automatically applied by the ability when triggered
            //ApplyCostsOnTrigger = false;  //true by default, abilities costs will be apply at the end instead,
                                            //or manually using the CommitAbility() function.
                                            
            IconPath = "Graphics/Sprite/UI/T_AbilityIcon_BloodSupply";
            UIName = "Poche de sang";
            AbilityDescription = "";
        }
        
        public override IEnumerator OnAbilityTriggered(GameObject avatar)
        {
            GameController.GetGameMode().MessageToUser(new GameController.UserMessageData(GameController.UserMessageData.MessageToUserSenderType.Player, 
                "Pensez a donnez votre sang."));
            yield return null;
        }

        public override bool ShouldAbilityTrigger(GameObject avatar)
        {
            throw new System.NotImplementedException();
        }
    }
}
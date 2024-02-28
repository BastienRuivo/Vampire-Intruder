using System.Collections;
using UnityEngine;

namespace Systems.Ability.Abilities
{
    public class AEffectVampireThirst : Ability
    {
        private const float ThirstStamina = 10.0f;
        public AEffectVampireThirst()
        {
            //Specify if ability triggers itself on a specific context
            SelfTriggeringAbility = true; //false by default.
            
            //Specify the ability cooldown.
            Cooldown = 1.0f;
        }
        public override IEnumerator OnAbilityTriggered(GameObject avatar)
        {
            //call the endgame
            GameController.GetGameMode().MessageToUser(new GameController.UserMessageData(
                GameController.UserMessageData.MessageToUserSenderType.Player,
                "Ah... Je me sens faible... Il faut vite que je boive du sang !...",
                4.0f,
                priority:GameController.UserMessageData.MessageToUserScheduleType.ImportanceOnReadability
            ));
            
            yield return new WaitForSeconds(ThirstStamina);
            if(GetAbilitySystemComponent(avatar).QueryStat("Blood") <= 0)
            {
                GameController.GetGameMode().MessageToUser(new GameController.UserMessageData(
                    GameController.UserMessageData.MessageToUserSenderType.Player,
                    "Non... Mes forces... Elles m'abandonnent...",
                    5.0f,
                    priority:GameController.UserMessageData.MessageToUserScheduleType.ImportanceOnReadability
                ));
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
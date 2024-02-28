using System.Collections;
using System.Collections.Generic;
using Systems.Ability;
using Systems.Ability.Abilities;
using UnityEngine;

public class AEffectGuardBiten : Ability
{
    private const string GuardDeathQuoteA = "Aarrrgh, non... Pourquoi moi ?! Je n'ai pas été embauché pour ça !";
    private const string GuardDeathQuoteB = "Aïe, mais... Ah, mes forces... Elles se vident...";
    private const string GuardDeathQuoteC = "Non, je dois protéger le manoir ! Quel est... Ce maléfice ?!";
    private const string GuardDeathQuoteK = "Aïe, mais... Oh... Cette morsure... C'est agréable...";
    
    public override IEnumerator OnAbilityTriggered(GameObject avatar)
    {
        if (Random.Range(0f, 1f) > 0.05)
        {
            float r = Random.Range(0f, 1f);
            if (r > 1f / 3f)
            {
                if (r > 1f / 3f)
                {
                    GameController.GetGameMode().MessageToUser(new GameController.UserMessageData(
                        GameController.UserMessageData.MessageToUserSenderType.Guard,
                        GuardDeathQuoteA,
                        priority:GameController.UserMessageData.MessageToUserScheduleType.ImportanceOnReadability
                    ));
                }
                else
                {
                    GameController.GetGameMode().MessageToUser(new GameController.UserMessageData(
                        GameController.UserMessageData.MessageToUserSenderType.Guard,
                        GuardDeathQuoteB,
                        priority:GameController.UserMessageData.MessageToUserScheduleType.ImportanceOnReadability
                    ));
                }
            }
            else
            {
                GameController.GetGameMode().MessageToUser(new GameController.UserMessageData(
                    GameController.UserMessageData.MessageToUserSenderType.Guard,
                    GuardDeathQuoteC,
                    priority:GameController.UserMessageData.MessageToUserScheduleType.ImportanceOnReadability
                ));
            }
        }
        else
        {
            GameController.GetGameMode().MessageToUser(new GameController.UserMessageData(
                GameController.UserMessageData.MessageToUserSenderType.Guard,
                GuardDeathQuoteK,
                priority:GameController.UserMessageData.MessageToUserScheduleType.ImportanceOnReadability
            ));
        }
        
        
        bool effectDone = false;
        var guard = avatar.GetComponent<GuardManager>();
        guard.GetComponent<Glow>().Activate();
        guard.enabled = false;
        
        float timer = AEffectVampireBite.eatTime;
        guard.GetAnimator().SetInteger("state", (int)GuardManager.AnimationState.IDLE);
        
        while (timer > 0)
        {
            if(timer < 0.6f && !effectDone){
                guard.GetAnimator().SetTrigger("isDie");
                effectDone = true;
            }

            guard.CallForHelp();
            yield return new WaitForSeconds(0.1f);
            timer -= 0.1f;
        }
        DestroyObject(avatar, avatar);

    }

    public override bool ShouldAbilityTrigger(GameObject avatar)
    {
        throw new System.NotImplementedException();
    }
}

using System.Collections;
using System.Collections.Generic;
using Systems.Ability;
using Systems.Ability.Abilities;
using UnityEngine;

public class AEffectGuardBiten : Ability
{
    public override IEnumerator OnAbilityTriggered(GameObject avatar)
    {
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

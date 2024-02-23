using System.Collections;
using System.Collections.Generic;
using Systems.Ability;
using Systems.Ability.Abilities;
using UnityEngine;

public class AEffectGuardBiten : Ability
{
    public override IEnumerator OnAbilityTriggered(GameObject avatar)
    {
        var guard = avatar.GetComponent<GuardManager>();
        guard.GetComponent<Glow>().Activate();
        guard.enabled = false;
        PlayerController player = PlayerState.GetInstance().GetPlayerController();
        player.LockVision(avatar.transform.position);
        float timer = AEffectVampireBite.eatTime;
        guard.GetAnimator().SetInteger("state", (int)GuardManager.AnimationState.IDLE);
        while (timer > 0)
        {
            guard.CallForHelp();
            player.LockVision(avatar.transform.position);
            yield return new WaitForSeconds(0.1f);
            timer -= 0.1f;
        }
        DestroyObject(avatar, avatar);
         player.UnlockVision();

    }

    public override bool ShouldAbilityTrigger(GameObject avatar)
    {
        throw new System.NotImplementedException();
    }
}

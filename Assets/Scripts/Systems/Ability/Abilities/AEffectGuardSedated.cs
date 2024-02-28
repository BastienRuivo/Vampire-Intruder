using System.Collections;
using Systems.Vision;
using UnityEngine;

namespace Systems.Ability.Abilities
{
    public class AEffectGuardSedated : Ability
    {
        public static readonly float SleepTime = 30f;

        public override IEnumerator OnAbilityTriggered(GameObject avatar)
        {
            GuardManager guardManager = avatar.GetComponent<GuardManager>();
            guardManager.GetVision().GetComponent<VisionSystemController>().enabled = false;
            guardManager.feedBackAnimatorSedate.SetBool("isSedate",true);
            guardManager.enabled = false;
            yield return new WaitForSeconds(SleepTime);
            guardManager.enabled = true;
            guardManager.feedBackAnimatorSedate.SetBool("isSedate",false);
            guardManager.GetVision().GetComponent<VisionSystemController>().enabled = true;
            
        }

        public override bool ShouldAbilityTrigger(GameObject avatar)
        {
            throw new System.NotImplementedException();
        }
    }
}
using System.Collections;
using UnityEngine;

namespace Systems.Ability.Abilities
{
    public class AEffectLureLifeTime : Ability
    {
        private const float lifeTime = 15f;
        
        public AEffectLureLifeTime()
        {
            //Specify if ability is a consumable object.
            //ConsumableAbility = true; //false by default.
            
            //Specify if ability triggers itself on a specific context
            SelfTriggeringAbility = true; //false by default.
            
            //Specify if ability's costs should be refunded in case of the ability being canceled
            //RefundOnCancel = false; //true by default.
            
            //Specify the ability cooldown.
            Cooldown = 1.0f; //-1 aka cooldown disabled by default.
            
            //Add the ability stat costs here.
            //AbilityCosts.Add("Blood", 15);
        }
        
        public override IEnumerator OnAbilityTriggered(GameObject avatar)
        {
            yield return new WaitForSeconds(lifeTime);
            avatar.transform.position = new Vector3(1000, 1000, 0);
            yield return null;
            DestroyObject(avatar, avatar);
        }

        public override bool ShouldAbilityTrigger(GameObject avatar)
        {
            return true;
        }
    }
}
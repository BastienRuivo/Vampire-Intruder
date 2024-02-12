using System.Collections;
using JetBrains.Annotations;
using UnityEngine;

namespace Systems.Ability.Abilities
{
    public class AVampireTryBite : Ability
    {
        [CanBeNull] public GameObject cursor = null;
        public AVampireTryBite()
        {
            Cooldown = 0.5f;
            //cursor = Resources.Load("Prefabs/myPrefab")
            
        }
        
        public override IEnumerator OnAbilityTriggered(GameObject avatar)
        {
            //GameObject instance = Instantiate(Resources.Load("enemy", typeof(GameObject))) as GameObject;
            
            Input.GetMouseButtonDown(0);
            
            GetAbilitySystemComponent(avatar).TriggerAbility("Bite");
            yield return null;
        }

        public override bool ShouldAbilityTrigger(GameObject avatar)
        {
            //implementation required only when SelfTriggeringAbility = true
            throw new System.NotImplementedException();
        }
    }
}
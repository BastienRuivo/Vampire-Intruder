﻿using System.Collections;
using UnityEngine;

namespace Systems.Ability.tests
{
    /// <summary>
    /// This is a self triggering / passive ability demo.
    /// </summary>
    public class TestPassiveAbility : Ability
    {
        public TestPassiveAbility()
        {
            //Specify if ability is a consumable object.
            //ConsumableAbility = true; //false by default.
            
            //Specify if ability triggers itself on a specific context
            SelfTriggeringAbility = true; //false by default.
            
            //Specify if ability's costs should be refunded in case of the ability being canceled
            //RefundOnCancel = false; //true by default.
            
            //Specify the ability cooldown.
            //Cooldown = 1.0f; //-1 aka cooldown disabled by default.
            
            //Add the ability stat costs here.
            //AbilityCosts.Add("Blood", 15);
        }
        
        public override IEnumerator OnAbilityTriggered(GameObject avatar)
        {
            Debug.Log("Ability (no more 'Blood') logic execution begins here.");
            
            //"avatar" gives you access to the game object triggering this ability.
            
            //You can get the Ability System Component of any given game object (avatar here) with this method :
            //AbilitySystemComponent avatarASC = GetAbilitySystemComponent(avatar);
            
            //wait for a period of time;
            //yield return new WaitForSeconds(5);
            
            //wait for a frame;
            //yield return null;
            
            //for more yield clauses, check out Unity's Coroutine documentation.
            
            //Debug.Log("Ability logic execution ends here.");
            
            yield return null; //at the end if ability does run on a single tick (no yield return before).
        }

        public override bool ShouldAbilityTrigger(GameObject avatar)
        {
            //implementation required only when SelfTriggeringAbility = true
            return GetAbilitySystemComponent(avatar).QueryStat("Blood") <= 0;
        }
    }
}
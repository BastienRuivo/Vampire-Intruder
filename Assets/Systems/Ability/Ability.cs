using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Systems.Ability
{
    public abstract class Ability
    {
        protected Dictionary<string, float> AbilityCosts = new();

        protected bool ConsumableAbility = false;

        protected bool SelfTriggeringAbility = false;
        
        protected bool RefundOnCancel = true;

        protected float Cooldown = -1.0f;

        public Dictionary<string, float> GetAbilityCosts()
        {
            return AbilityCosts;
        }
        
        /// <returns>true if the ability works with charges. Abilities with charges needs to consume one charge to be able to be triggered.</returns>
        public bool IsConsumableAbility()
        {
            return ConsumableAbility;
        }
        
        /// <returns>true if the ability can be self triggered. The condition(s) of when trigger the ability is(are) implemented in the ShouldAbilityTrigger method.</returns>
        public bool IsSelfTriggeringAbility()
        {
            return SelfTriggeringAbility;
        }
        
        /// <returns>true if the ability canceling the ability will refund the owner.</returns>
        public bool DoRefundOnCancel()
        {
            return RefundOnCancel;
        }

        public float GetCooldown()
        {
            return Cooldown;
        }
        
        /// <summary>
        /// Run ability. This function contains the the logic of an ability.
        /// </summary>
        /// <param name="avatar">Ability owner game object</param>
        /// <returns></returns>
        public abstract IEnumerator OnAbilityTriggered(GameObject avatar);
        
        /// <summary>
        /// Check if ability should trigger itself. Required only when SelfTriggeringAbility is set to true.
        /// </summary>
        /// <param name="avatar">Ability owner game object</param>
        /// <returns>true when the ability should trigger.</returns>
        public abstract bool ShouldAbilityTrigger(GameObject avatar);


        /// <param name="avatar">A given game object</param>
        /// <returns>a reference to the Ability System Component of a game object.</returns>
        protected static AbilitySystemComponent GetAbilitySystemComponent(GameObject avatar)
        {
            return avatar.GetComponent<AbilitySystemComponent>();
        }
    }
}
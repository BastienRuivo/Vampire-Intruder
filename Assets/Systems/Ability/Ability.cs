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

        public Dictionary<string, float> GetAbilityCosts()
        {
            return AbilityCosts;
        }
        
        /// <returns>true if the ability works with charges. Abilities with charges needs to consume one charge to be able to be triggered.</returns>
        public bool IsConsumableAbility()
        {
            return ConsumableAbility;
        }
        
        /// <summary>
        /// Run ability. This function contains the the logic of an ability.
        /// </summary>
        /// <param name="avatar">Ability owner game object</param>
        /// <returns></returns>
        public abstract IEnumerator OnAbilityTriggered(GameObject avatar);


        /// <param name="avatar">A given game object</param>
        /// <returns>a reference to the Ability System Component of a game object.</returns>
        protected static AbilitySystemComponent GetAbilitySystemComponent(GameObject avatar)
        {
            return avatar.GetComponent<AbilitySystemComponent>();
        }
    }
}
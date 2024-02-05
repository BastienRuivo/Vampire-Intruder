using System.Collections;
using UnityEngine;

namespace Systems.Ability.tests
{
    /// <summary>
    /// This is an ability demo.
    /// </summary>
    public class TestAbility : Ability
    {
        public TestAbility()
        {
            //Specify is ability is a consumable object.
            //ConsumableAbility = true; //false by default.
            
            //Add the ability stat costs here.
            AbilityCosts.Add("Blood", 15);
        }
        
        public override IEnumerator OnAbilityTriggered(GameObject avatar)
        {
            Debug.Log("Ability logic execution begins here.");
            
            //"avatar" gives you access to the game object triggering this ability.
            
            //You can get the Ability System Component of any given game object (avatar here) with this method :
            //AbilitySystemComponent avatarASC = GetAbilitySystemComponent(avatar);
            
            //wait for a period of time;
            yield return new WaitForSeconds(5);
            
            //wait for a frame;
            //yield return null;
            
            //for more yield clauses, check out Unity's Coroutine documentation.
            
            Debug.Log("Ability logic execution ends here.");

            
            //yield return null; //at the end if ability does run on a single tick (no yield return before).
        }
    }
}
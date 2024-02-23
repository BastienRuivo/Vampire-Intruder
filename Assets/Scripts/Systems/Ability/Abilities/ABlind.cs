using System.Collections;
using Systems.Ability.Aiming;
using Unity.VisualScripting;
using UnityEngine;

namespace Systems.Ability.Abilities
{
    public class ABlind : Ability
    {
         public ABlind()
        {
            //Specify if ability is a consumable object.
            //ConsumableAbility = true; //false by default.
            
            //Specify if ability triggers itself on a specific context
            //SelfTriggeringAbility = true; //false by default.
            
            //Specify if ability's costs should be refunded in case of the ability being canceled
            //RefundOnCancel = false; //true by default.

            //Specify the ability cooldown.
            Cooldown = 6.0f; //-1 aka cooldown disabled by default.
            
            //Add the ability stat costs here.
            AbilityCosts.Add("Blood", 50.0f);

            //Specify if costs should automatically applied by the ability when triggered
            ApplyCostsOnTrigger = false;  //true by default, abilities costs will be apply at the end instead,
                                          //or manually using the CommitAbility() function.
                                          
            IconPath = "Graphics/Sprite/UI/T_AbilityIcon_Blind";
            UIName = "Cataracte";
            AbilityDescription = "Aveugle un garde pendant une durée déterminé."; //todo description
        }
        
        public override IEnumerator OnAbilityTriggered(GameObject avatar)
        {
            //"avatar" gives you access to the game object triggering this ability.
            
            //wait for a period of time;
            
            //wait for a frame;
            //yield return null;
            
            //for more yield clauses, check out Unity's Coroutine documentation.
            
            GameObject cursor = InstanceResource(avatar, "Abilities/Aiming/MouseAimLock");
            GameObject target = null;
            if (cursor == null)
                CancelAbility(avatar);
            
            avatar.GetComponent<PlayerController>().BindVisionToMouse();

            MouseLockingAimeController cursorController = cursor.GetComponent<MouseLockingAimeController>();
            cursorController.aimLockDistance = 1.0f;
            cursorController.targetType = Targetable.TargetType.ENEMY;
            cursorController.isTargetValid = false;
            while (true)
            {
                yield return new WaitForNextFrameUnit();
                //Cancel ability on right click
                if (Input.GetMouseButtonDown((int)MouseButton.Right))
                {
                    avatar.GetComponent<PlayerController>().UnbindVisionFromMouse();
                    CancelAbility(avatar);
                    break;
                }
                
                if (cursorController.currentTarget == null)
                {
                    cursorController.isTargetValid = false;
                    continue;
                }
                
                cursorController.isTargetValid = true;
            
                if (Input.GetMouseButtonDown(0))
                {
                    target = cursorController.currentTarget;
                    break;
                }
            }
            
            avatar.GetComponent<PlayerController>().UnbindVisionFromMouse();
            
            if (target != null)
            {
                CommitAbility(avatar);

                GuardManager guardController = cursorController.currentTarget.GetComponent<GuardManager>();
                DestroyResource(avatar, cursor);

                float baseFOV = guardController.GetVision().fov;
                float baseViewDistance = guardController.GetVision().viewDistance;
                guardController.GetVision().fov = 70;
                guardController.GetVision().viewDistance = 1.5f;

                yield return new WaitForSeconds(5.0f);

                guardController.GetVision().fov = baseFOV;
                guardController.GetVision().viewDistance = baseViewDistance;
            }
            
            //yield return null; //at the end if ability does run on a single tick (no yield return before).
        }

        public override bool ShouldAbilityTrigger(GameObject avatar)
        {
            //implementation required only when SelfTriggeringAbility = true
            throw new System.NotImplementedException();
        }
    }
}
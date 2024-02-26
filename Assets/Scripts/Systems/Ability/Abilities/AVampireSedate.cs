using System.Collections;
using Systems.Ability.Aiming;
using Unity.VisualScripting;
using UnityEngine;

namespace Systems.Ability.Abilities
{
    public class AVampireSedate : Ability
    {
        public AVampireSedate()
        {
            //Specify if ability is a consumable object.
            ConsumableAbility = true; //false by default.
            
            //Specify if ability triggers itself on a specific context
            //SelfTriggeringAbility = true; //false by default.
            
            //Specify if ability's costs should be refunded in case of the ability being canceled
            //RefundOnCancel = false; //true by default.

            //Specify the ability cooldown.
            Cooldown = 0.5f; //-1 aka cooldown disabled by default.
            
            //Add the ability stat costs here.
            //AbilityCosts.Add("Blood", 50.0f);

            //Specify if costs should automatically applied by the ability when triggered
            ApplyCostsOnTrigger = false;    //true by default, abilities costs will be apply at the end instead,
                                            //or manually using the CommitAbility() function.
                                          
            IconPath = "Graphics/Sprite/UI/T_AbilityIcon_Sedation";
            UIName = "Sédatif";
            AbilityDescription = "Endor un ennemi pendant une longue durée. Approchez-vous d’un garde pour le " +
                                 "neutraliser mais faites attention, vous ne pouvez sédater un garde s’il se doute " +
                                 "de quelque chose.";
        }   
        
        public override IEnumerator OnAbilityTriggered(GameObject avatar)
        {
            GameObject cursor = InstanceResource(avatar, "Abilities/Aiming/WorldAimLock");
            avatar.GetComponent<PlayerController>().BindVisionToMouse();

            GameObject target = null;
            if (cursor != null)
            {
                WorldAimLockController cursorController = cursor.GetComponent<WorldAimLockController>();
                cursorController.targetType = Targetable.TargetType.ENEMY;
                cursorController.isTargetValid = false;
                while (true)
                {
                    yield return new WaitForNextFrameUnit();
                    //Cancel ability on right click
                    if (Input.GetMouseButtonDown((int)MouseButton.Right))
                    {
                        break;
                    }
                    
                    if (cursorController.currentTarget == null)
                    {
                        cursorController.isTargetValid = false;
                        continue;
                    }


                    GuardManager guardController = cursorController.currentTarget.GetComponent<GuardManager>();
                    if (guardController.alertStage != AlertStage.Idle)
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
            }
            
            avatar.GetComponent<PlayerController>().UnbindVisionFromMouse();
            
            if (target != null)
            {
                avatar.transform.position = target.transform.position;
                GetAbilitySystemComponent(target).TriggerAbility("Sedate");
                LockInput(avatar);
            
                yield return new WaitForSeconds(1.0f);
            }
            else
            {
                CancelAbility(avatar);
            }
            
            yield return null;
        }

        public override bool ShouldAbilityTrigger(GameObject avatar)
        {
            throw new System.NotImplementedException();
        }
    }
}
using System.Collections;
using JetBrains.Annotations;
using Systems.Ability;
using Systems.Ability.Aiming;
using Unity.VisualScripting;
using UnityEngine;

namespace Systems.Ability.Abilities
{
    public class AVampireTryBite : Ability
    {
        public AVampireTryBite()
        {
            Cooldown = 0.5f;
            IconPath = "Graphics/Sprite/UI/T_AbilityIcon_Bite";
        }
        
        public override IEnumerator OnAbilityTriggered(GameObject avatar)
        {
            GameObject cursor = InstanceResource(avatar, "Abilities/Aiming/MouseAimLock");
            avatar.GetComponent<PlayerController>().BindVisionToMouse();

            GameObject target = null;
            if (cursor != null)
            {
                MouseLockingAimeController cursorController = cursor.GetComponent<MouseLockingAimeController>();
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

                    if (Vector3.Distance(cursorController.currentTarget.transform.position, avatar.transform.position) <
                        0.5f)
                    {
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
                    else
                    {
                        cursorController.isTargetValid = false;
                    }
                }
            }
            
            avatar.GetComponent<PlayerController>().UnbindVisionFromMouse();
            
            if (target != null)
            {
                avatar.transform.position = target.transform.position;
                GetAbilitySystemComponent(avatar).TriggerAbility("Bite");
                GetAbilitySystemComponent(target).TriggerAbility("Eaten");
            }
            
            yield return null;
        }

        public override bool ShouldAbilityTrigger(GameObject avatar)
        {
            //implementation required only when SelfTriggeringAbility = true
            throw new System.NotImplementedException();
        }
    }
}
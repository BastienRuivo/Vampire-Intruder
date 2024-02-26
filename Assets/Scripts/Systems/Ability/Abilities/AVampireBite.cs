﻿using System.Collections;
using JetBrains.Annotations;
using Systems.Ability;
using Systems.Ability.Aiming;
using Unity.VisualScripting;
using UnityEngine;

namespace Systems.Ability.Abilities
{
    public class AVampireBite : Ability
    {
        public AVampireBite()
        {
            Cooldown = 0.5f;
            IconPath = "Graphics/Sprite/UI/T_AbilityIcon_Bite";
        }
        
        public override IEnumerator OnAbilityTriggered(GameObject avatar)
        {
            GameObject cursor = InstanceResource(avatar, "Abilities/Aiming/WorldAimLock");
            PlayerController player = avatar.GetComponent<PlayerController>();
            player.BindVisionToMouse();

            GameObject target = null;
            if (cursor != null)
            {
                WorldAimLockController cursorController = cursor.GetComponent<WorldAimLockController>();
                cursorController.owner = avatar;
                cursorController.targetType = Targetable.TargetType.ENEMY;
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
            
            player.UnbindVisionFromMouse();
            
            if (target != null)
            {
                avatar.transform.position = target.transform.position;
                GetAbilitySystemComponent(avatar).CancelAbility("Invisibility");
                GetAbilitySystemComponent(avatar).TriggerAbility("Bite");
                GetAbilitySystemComponent(target).TriggerAbility("Eaten");
                player.LockVision(avatar.transform.position);
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
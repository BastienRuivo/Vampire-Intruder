using System.Collections;
using JetBrains.Annotations;
using Systems.Ability;
using Systems.Ability.Aiming;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

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
                WorldAimLockController cursorController = cursor.GetComponent<WorldAimLockController>();
                Vector2 dir = (target.transform.position - avatar.transform.position);
                RaycastHit2D hit = Physics2D.Raycast(avatar.transform.position, dir, Vector2.Distance(avatar.transform.position, target.transform.position), cursorController.visionMask | LayerMask.GetMask("Enemy"));
                Debug.DrawRay(avatar.transform.position, dir.normalized, Color.red, 10);
                if(hit.collider != null)
                {
                    Vector2 newpos = dir.normalized * hit.distance * 0.9f;
                    avatar.transform.position = avatar.transform.position + new Vector3(newpos.x, newpos.y);
                }
                Direction rawDir = DirectionHelper.BetweenTwoObjects(avatar, target);
                Vector2 d = DirectionHelper.FromDirection(rawDir);
                player.GetAnimator().SetFloat("xSpeed", d.x);
                player.GetAnimator().SetFloat("ySpeed", d.y);
                GetAbilitySystemComponent(avatar).CancelAbility("Invisibility");
                GetAbilitySystemComponent(avatar).TriggerAbility("Bite");
                GetAbilitySystemComponent(target).TriggerAbility("Eaten");
                player.LockVision(target.transform.position);
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
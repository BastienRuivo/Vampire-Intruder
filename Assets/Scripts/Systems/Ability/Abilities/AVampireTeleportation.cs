using System.Collections;
using Systems.Ability.Aiming;
using Unity.VisualScripting;
using UnityEngine;

namespace Systems.Ability.Abilities
{
    public class AVampireTeleportation : Ability
    {
        public AVampireTeleportation()
        {
            Cooldown = 5.0f;
            
            AbilityCosts.Add("Blood", 12.5f);

            RefundOnCancel = true;

            ApplyCostsOnTrigger = false;
            
            IconPath = "Graphics/Sprite/UI/T_AbilityIcon_TP";
            UIName = "Téléportation";
            AbilityDescription = ""; //todo description
        }
        public override IEnumerator OnAbilityTriggered(GameObject avatar)
        {
            GameObject cursor = InstanceResource(avatar, "Abilities/Aiming/MouseAim");
            if(cursor == null)
                yield break;

            WorldPositionAimingController cursorController = cursor.GetComponent<WorldPositionAimingController>();
            cursorController.canGoInAnotherRoom= true;
            cursorController.owner = avatar;
            cursorController.aimDistance = 2.5f;
            cursorController.blockVisionToWall = false;

            bool hasTarget = false;
            Vector3 targetPosition = default;
            
            avatar.GetComponent<PlayerController>().BindVisionToMouse();
            
            while (true)
            {
                yield return new WaitForNextFrameUnit();
                if (Input.GetMouseButtonDown((int)MouseButton.Right))
                {
                    break;
                }

                if (Input.GetMouseButtonDown(0))
                {
                    hasTarget = true;
                    targetPosition = cursorController.currentTarget;
                    if(PlayerState.GetInstance().currentRoom != cursorController.currentRoom)
                    {
                        cursorController.currentRoom.SetCurrent(PlayerState.GetInstance().currentRoom);
                    }


                    break;
                }
            }
            avatar.GetComponent<PlayerController>().UnbindVisionFromMouse();

            if (hasTarget)
            {
                Vector2 dir = (targetPosition - avatar.transform.position).normalized;
                RaycastHit2D ray = Physics2D.Raycast(avatar.transform.position, dir, 0, cursorController.visionMask);
                if(ray.collider == null)
                {
                    avatar.transform.position = targetPosition;
                }
                else
                {
                    Vector2 nd = dir * ray.distance * 0.75f;
                    avatar.transform.position += new Vector3(nd.x, nd.y, 0f);
                }
            }
            else
            {
                CancelAbility(avatar);
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
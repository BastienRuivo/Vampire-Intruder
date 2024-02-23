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
            cursorController.owner = avatar;
            cursorController.aimDistance = 2.5f;
            cursorController.collide = false;

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


                    break;
                }
            }
            avatar.GetComponent<PlayerController>().UnbindVisionFromMouse();

            if (hasTarget)
            {
                avatar.transform.position = targetPosition;
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
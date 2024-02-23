using System.Collections;
using Systems.Ability.Aiming;
using Unity.VisualScripting;
using UnityEngine;

namespace Systems.Ability.Abilities
{
    public class AVampireLure : Ability
    {
        public AVampireLure()
        {
            //Specify if ability is a consumable object.
            //ConsumableAbility = true; //false by default.
            
            //Specify if ability triggers itself on a specific context
            //SelfTriggeringAbility = true; //false by default.
            
            //Specify if ability's costs should be refunded in case of the ability being canceled
            //RefundOnCancel = false; //true by default.

            //Specify the ability cooldown.
            Cooldown = 5.0f; //-1 aka cooldown disabled by default.
            
            //Add the ability stat costs here.
            AbilityCosts.Add("Blood", 25f);

            //Specify if costs should automatically applied by the ability when triggered
            ApplyCostsOnTrigger = false;    //true by default, abilities costs will be apply at the end instead,
                                            //or manually using the CommitAbility() function.
                                            
            IconPath = "Graphics/Sprite/UI/T_AbilityIcon_Lure";
            UIName = "Leure";
            AbilityDescription = "Créez un leure qui détournera vos ennemis pendant un certain temps. Attirez-les hors" +
                                 " de votre chemin. Attention, un bon garde saura repérer la supercherie.";
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
                GameObject lureObject = GetAbilitySystemComponent(avatar)
                    .InstanceGameObject(this, "Abilities/Aiming/PlayerLure", false);
                lureObject.transform.position = targetPosition;
                AbilitySystemComponent lureAsc = GetAbilitySystemComponent(lureObject);
                lureAsc.GrantAbility<AEffectLureLifeTime>("lifeTime");
            }
            else
            {
                CancelAbility(avatar);
            }
        }

        public override bool ShouldAbilityTrigger(GameObject avatar)
        {
            throw new System.NotImplementedException();
        }
    }
}
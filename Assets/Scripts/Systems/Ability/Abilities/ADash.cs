using System.Collections;
using DefaultNamespace;
using Unity.VisualScripting;
using UnityEngine;
using Tools = DefaultNamespace.Tools;

namespace Systems.Ability.Abilities
{
    //todo remove
    
    public class ADash : Ability
    {
        private const float DashDistance = 2.0f;

        public ADash()
        {
            //Specify if ability is a consumable object.
            //ConsumableAbility = true; //false by default.
            
            //Specify if ability triggers itself on a specific context
            //SelfTriggeringAbility = true; //false by default.
            
            //Specify if ability's costs should be refunded in case of the ability being canceled
            //RefundOnCancel = false; //true by default.

            //Specify the ability cooldown.
            Cooldown = 1.0f; //-1 aka cooldown disabled by default.
            
            //Add the ability stat costs here.
            AbilityCosts.Add("Blood", 6.25f);

            //Specify if costs should automatically applied by the ability when triggered
            ApplyCostsOnTrigger = false;    //true by default, abilities costs will be apply at the end instead,
                                            //or manually using the CommitAbility() function.
        }

        public override IEnumerator OnAbilityTriggered(GameObject avatar)
        {
            GameObject lure = InstanceResource(avatar, "Abilities/Aiming/PlayerLure");

            PlayerController controller = avatar.GetComponent<PlayerController>();
            Vector3 playerPosition = avatar.transform.position;
            Vector3 targetPosition = GetDashDestination(playerPosition, controller);
            lure.transform.position = targetPosition;
            
            if(Input.GetKey(KeyCode.E))
                while (!Input.GetKeyUp(KeyCode.E))
                {
                    targetPosition = GetDashDestination(playerPosition, controller);
                    lure.transform.position = targetPosition;
                    yield return new WaitForNextFrameUnit();
                }
            
            DestroyResource(avatar, lure);
            CommitAbility(avatar);
            LockInput(avatar);

            
            SmoothScalarValue interpolationAlpha = new SmoothScalarValue(0, 0.25f);
            interpolationAlpha.RetargetValue(1);
            while (interpolationAlpha.GetValue() < 1)
            {
                avatar.transform.position =
                    Tools.Lerp(playerPosition, targetPosition, interpolationAlpha.UpdateGetValue());
                yield return new WaitForNextFrameUnit();
            }
            
            yield return null;
        }

        private Vector3 GetDashDestination(Vector3 playerPosition, PlayerController playerController)
        {
            Vector2 dir = DirectionHelper.FromDirection(playerController.directionPerso);
            dir = dir.normalized;

            float dist = DashDistance;

            Vector3 dir3D = new Vector3(dir.x, dir.y);
            
            RaycastHit2D hit = Physics2D.Raycast(playerPosition + dir3D, dir, DashDistance - 1);
            if (hit.collider != null)
            {
                dist = hit.distance - 0.5f;
            }

            dir3D = dir3D * dist;

            return playerPosition + dir3D;
        }

        public override bool ShouldAbilityTrigger(GameObject avatar)
        {
            //implementation required only when SelfTriggeringAbility = true
            throw new System.NotImplementedException();
        }
    }
}
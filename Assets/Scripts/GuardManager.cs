using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public enum AlertStage
{
    Idle,
    Suspicious,
    Alerted
}

public class GuardManager : MonoBehaviour
{
    public bool playerInFOV;
    public LayerMask visionMask;

    public AlertStage alertStage;
    [Range(0,100)]  public float alertLevel; // 0-100

    private void Awake() {
        alertStage = AlertStage.Idle;
        alertLevel = 0;
    }

    private void Update() {
        updateAlertStage(playerInFOV);

        Color c = Color.green;
        if(alertStage == AlertStage.Suspicious)
            c = Color.Lerp(Color.green, Color.red, alertLevel/100);
        if(alertStage == AlertStage.Alerted){
            if(playerInFOV){
                playerInFOV = false;
                GameController.GetGameMode().GetCaught();
            }
                
            c = Color.red;
        }
        GetComponent<SpriteRenderer>().color = c;
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if(other.CompareTag("Player") && noWallBetweenPlayerAndEnemy(other.gameObject.transform.position))
        {
            playerInFOV = true;
        }
    }

    private void OnTriggerStay2D(Collider2D other) {
        if(other.CompareTag("Player"))
        {
            if(noWallBetweenPlayerAndEnemy(other.gameObject.transform.position)){
                playerInFOV = true;
            }else{
                playerInFOV = false;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        if(other.CompareTag("Player"))
        {
            playerInFOV = false;
        }
    }

    private void updateAlertStage(bool playerInFOV)
    {
        switch (alertStage)
        {
            case AlertStage.Idle:
                if (playerInFOV)
                    alertStage = AlertStage.Suspicious;
                break;
            case AlertStage.Suspicious:
                if (playerInFOV)
                {
                    alertLevel++;
                    if (alertLevel >= 100)
                        alertStage = AlertStage.Alerted;
                }
                else
                {
                    alertLevel--;
                    if (alertLevel <= 0)
                        alertStage = AlertStage.Idle;
                }
                break;
            case AlertStage.Alerted:
                if (!playerInFOV)
                    alertStage = AlertStage.Suspicious;
                break;
        }
    }

    private bool noWallBetweenPlayerAndEnemy(Vector3 playerPosition)
    {
        Vector3 direction = playerPosition - transform.position;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, Vector3.Distance(playerPosition,transform.position), visionMask);
        Debug.DrawRay(transform.position, direction, Color.red);
        if(hit.collider != null)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    

}
// CONE VISION
//          Collider[] targetsInFOV = Physics.OverlapSphere(
//             transform.position, fov);
//         foreach (Collider c in targetsInFOV)
//         {
//             if (c.CompareTag("Player"))
//             {
//                 float signedAngle = Vector3.Angle(
//                     transform.forward,
//                     c.transform.position - transform.position);
//                 if (Mathf.Abs(signedAngle) < fovAngle / 2)
//                     playerInFOV = true;
//                 break;
//             }
//         }
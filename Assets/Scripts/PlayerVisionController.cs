using System.Collections;
using System.Collections.Generic;
using Systems.Vision;
using UnityEngine;


public class PlayerVisionController : MonoBehaviour
{   
    public LayerMask visionMask;
    public GameObject visionCone;
    private VisionConeController _visionConeController;
    PlayerController playerController;
    Transform vision;

    private void Awake() {
        playerController = GetComponent<PlayerController>();
        vision = transform.GetChild(0); // Récupérer le transform de la vision //TODO recuperer autrement
        _visionConeController = visionCone.GetComponent<VisionConeController>();
        _visionConeController.Enable();
    }

    private void Update() {
        if(Input.GetMouseButton(0))
        {
            Vector3 characterPosition = transform.position;
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0f;
            Vector3 direction = mousePosition - characterPosition;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            visionCone.transform.localRotation = Quaternion.Euler(0,0, angle + 90);
        }
        else
        {
            visionCone.transform.localRotation = Quaternion.Euler(0,0,(int)playerController.directionPerso * 45);
        }
        
    }

    private void OnTriggerEnter2D(Collider2D other) {
        return;
        if(other.gameObject.tag == "object")
        {
            Debug.Log("I see the object" + other.gameObject.name);
        }
        if(other.gameObject.tag == "Enemy" && noWallBetweenPlayerAndEnemy(other.gameObject.transform.position))
        {
            GetComponent<SpriteRenderer>().color = Color.red;
            Debug.Log("I see the enemy");
            var manager = other.GetComponent<GuardManager>();
            manager.EnterPlayerSigth();
        }
    }

    private void OnTriggerStay2D(Collider2D other) {
        return;
        if(other.gameObject.tag == "Enemy")
        {
            if(noWallBetweenPlayerAndEnemy(other.gameObject.transform.position)){
                GetComponent<SpriteRenderer>().color = Color.red;
                other.gameObject.GetComponent<Animator>().SetBool("enemyVisible", true);
            }
            else
            {
                GetComponent<SpriteRenderer>().color = Color.white;
                var manager = other.GetComponent<GuardManager>();
                manager.ExitPlayerSight();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        return;
        if(other.gameObject.tag == "Enemy")
        {
            other.gameObject.GetComponent<Animator>().SetBool("enemyVisible", false);
            GetComponent<SpriteRenderer>().color = Color.white;
        }
    }

    private bool noWallBetweenPlayerAndEnemy(Vector3 enemyPosition)
    {
        Vector3 direction = enemyPosition - transform.position;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, Vector3.Distance(enemyPosition,transform.position), visionMask);
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

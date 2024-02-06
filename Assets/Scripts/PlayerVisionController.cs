using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerVisionController : MonoBehaviour
{   
    public LayerMask visionMask;
    PlayerController playerController;
    Transform vision;

    private void Awake() {
        playerController = GetComponent<PlayerController>();
        vision = transform.GetChild(1); // Récupérer le transform de la vision //TODO recuperer autrement
    }

    private void Update() {
        //click enfonce 

        if(Input.GetMouseButton(0))
        {
            // Récupérer la position du personnage
            Vector3 characterPosition = transform.position;

            // Récupérer la position du clic de souris
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0f; // Assurer que la position z est la même que celle du personnage

            // Calculer la différence de position entre le personnage et le clic de souris
            Vector3 direction = mousePosition - characterPosition;

            // Calculer l'angle en degrés entre le personnage et le clic de souris
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            vision.localRotation = Quaternion.Euler(0,0, angle + 90);
        }
        else
        {
            vision.localRotation = Quaternion.Euler(0,0,(int)playerController.directionPerso * 45);
        }
        
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if(other.gameObject.tag == "object")
        {
            Debug.Log("I see the object" + other.gameObject.name);
        }
        if(other.gameObject.tag == "Enemy" && noWallBetweenPlayerAndEnemy(other.gameObject.transform.position))
        {
            GetComponent<SpriteRenderer>().color = Color.red;
            Debug.Log("I see the enemy");
            other.gameObject.GetComponent<Animator>().SetBool("enemyVisible", true);
        }
    }

    private void OnTriggerStay2D(Collider2D other) {
        if(other.gameObject.tag == "Enemy")
        {
            if(noWallBetweenPlayerAndEnemy(other.gameObject.transform.position)){
                GetComponent<SpriteRenderer>().color = Color.red;
                other.gameObject.GetComponent<Animator>().SetBool("enemyVisible", true);
            }
            else
            {
                GetComponent<SpriteRenderer>().color = Color.white;
                other.gameObject.GetComponent<Animator>().SetBool("enemyVisible", false);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
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

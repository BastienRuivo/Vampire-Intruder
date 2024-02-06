using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerColliderController : MonoBehaviour
{
    private int exitLock = 0;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Enter tag " + collision.gameObject.tag);
        if(collision.gameObject.tag == "EnterArea")
        {
            Debug.Log("Entered a start zone !");
        }
        else if(collision.gameObject.tag == "RoomChanger")
        {
            Debug.Log("Entered a wall changer zone");
            // Get Tilemap of collision
            if(exitLock == 0)
            {
                collision.gameObject.GetComponent<RoomConnector>().Enter();
            }
            exitLock++;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        Debug.Log("Left tag " + collision.gameObject.tag);
        if (collision.gameObject.tag == "EnterArea")
        {
            Debug.Log("Left a start zone !");
        }
        else if (collision.gameObject.tag == "RoomChanger")
        {
            Debug.Log("Entered a wall changer zone");
            // Get Tilemap of collision
            if(exitLock == 1)
                collision.gameObject.GetComponent<RoomConnector>().Exit();
            exitLock--;
        }
    }
}

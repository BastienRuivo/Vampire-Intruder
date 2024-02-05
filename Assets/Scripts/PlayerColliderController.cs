using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerColliderController : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Enter tag " + collision.gameObject.tag);
        if(collision.gameObject.tag == "EnterArea")
        {
            Debug.Log("Entered a start zone !");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        Debug.Log("Left tag " + collision.gameObject.tag);
        if (collision.gameObject.tag == "EnterArea")
        {
            Debug.Log("Left a start zone !");
        }
    }
}

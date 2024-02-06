using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TargetWallDown : MonoBehaviour
{
    // Tile you leave
    [Header("Leaving Room")]
    public GameObject currentRoom;

    [Header("Entering Room")]
    public GameObject TargetRoom;

    public void Enter()
    {
        TargetRoom.GetComponent<Renderer>().enabled = true;
    }

    public void Exit()
    {
        TargetRoom.GetComponent<Renderer>().enabled = false;
    }



}

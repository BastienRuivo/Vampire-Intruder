using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RoomConnector : MonoBehaviour
{
    // Tile you leave
    [Header("Rooms info")]
    // Current room will always be set visible within this script, it's the connector from the other room
    // that will disable it
    public RoomData room;
    public RoomData targetRoom;
    public RoomConnector targetRoomConnector;

    [Header("Inner data")]
    public Direction dir;
    public Vector3Int coordOnGrid;
    public RoomData.Type[] allowedBranch;
    /// <summary>
    /// Int to tell at wich number it's this connector 
    /// </summary>
    public int[] branchPercentage;

    [Header("Materials")]
    public Material transparentMtl;
    public Material defaultMtl;
    public bool isFilled = false;

    private GameObject _collisions;
    private GameObject _trigger;


    /// <summary>
    /// Get an allowed branch from a number between 0 and 100
    /// </summary>
    /// <param name="roll"></param>
    /// <returns>An allowed data type</returns>
    public RoomData.Type GetFromDice(int roll)
    {
        for (int i = 0; i < allowedBranch.Length; ++i)
        {
            if (roll < branchPercentage[i])
            {
                return allowedBranch[i];
            }
        }

        Debug.Log("How did we get here ? RoomConnector.GetFromDice");
        return RoomData.Type.NOONE;
    }

    public void Enter()
    {
        room.EnterToAdjacent(targetRoom, dir);
    }

    public void Exit()
    {
        room.SetCurrent(targetRoom);
    }
    

    void Awake()
    {
        Transform coll = transform.Find("Grid/Collision");
        if(coll != null)
        {
            _collisions = coll.gameObject;
        }
        _trigger = transform.Find("Grid/ColliderTrigger").gameObject;
    }

    public void SetActiveState(bool isActive)
    {
        if(isActive)
        {
            _collisions.SetActive(false);
            _trigger.SetActive(true);
        }
        else
        {
            _collisions.SetActive(true);
            _trigger.SetActive(false);
            targetRoom = null;
        }
    }
}

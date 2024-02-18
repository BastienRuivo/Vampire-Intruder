using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RoomConnector : MonoBehaviour
{
    // Tile you leave
    [Header("Current Room")]
    // Current room will always be set visible within this script, it's the connector from the other room
    // that will disable it
    public GameObject currentRoom;
    public GameObject currentWalls;

    [Header("Connected Room")]
    public GameObject targetRoom;
    public GameObject targetWalls;
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

    private IEnumerator _roomFadeAway = null;
    private bool _fade = false;

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
        SetRoomVisibility(targetRoom, 1f);
        targetWalls.GetComponent<Renderer>().material.color = transparentMtl.color;
        currentWalls.GetComponent<Renderer>().material.color = transparentMtl.color;
    }

    public void Exit()
    {
        SetRoomVisibility(currentRoom, 1f);

        //// Start coroutine
        // Disable Coroutine if one is already started
        if (targetRoomConnector._roomFadeAway != null)
        {
            targetRoomConnector._fade = false;
            StopCoroutine(targetRoomConnector._roomFadeAway);
            SetRoomVisibility(currentRoom, 1f);
        }
        
        _roomFadeAway = DisableRoom(targetRoom);
        _fade = true;
        StartCoroutine(_roomFadeAway);

        PlayerState.GetInstance().currentRoom = currentRoom;
    }

    private void SetRoomVisibility(GameObject roomRoot, float value)
    {
        foreach (Renderer renderer in roomRoot.GetComponentsInChildren<Renderer>())
        {
            if (!renderer.material.HasColor("_Color"))
            {
                continue;
            }
            Color c = renderer.material.color;
            if(renderer.material.name.Contains(defaultMtl.name))
            {
                c.a = value * defaultMtl.color.a;
            }
            else
            {
                c.a = value * transparentMtl.color.a;
            }
            renderer.material.color = c;
        }
    }

    public IEnumerator DisableRoom(GameObject roomRoot)
    {
        for (float alpha = 1f; alpha > 0.02f; alpha -= Time.deltaTime)
        {
            if (!_fade) break;
            alpha = Mathf.Max(alpha, 0.02f);
            SetRoomVisibility(roomRoot, alpha);
            yield return null;
        }
    }
}

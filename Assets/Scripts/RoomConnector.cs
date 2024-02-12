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
    public GameObject targetCollider;

    [Header("Materials")]
    public Material transparentMtl;
    public Material defaultMtl;

    private IEnumerator roomFadeAway = null;
    public bool fade = false;

    public void Enter()
    {

        //SetRoomVisibility(currentRoom, 1.0f);
        //SetRoomVisibility(targetRoom, 1.0f);
        SetRoomVisibility(targetRoom, 1f);
        targetWalls.GetComponent<Renderer>().material.color = transparentMtl.color;
        currentWalls.GetComponent<Renderer>().material.color = transparentMtl.color;


    }

    void ResetMaterial(GameObject obj)
    {
        if (obj == null) return;
        if (obj.GetComponent<Renderer>().material.name.Contains(defaultMtl.name))
        {
            obj.GetComponent<Renderer>().material.color = defaultMtl.color;
        }
        else
        {
            obj.GetComponent<Renderer>().material.color = transparentMtl.color;
        }
    }

    public void Exit()
    {
        SetRoomVisibility(currentRoom, 1f);
        //SetRoomVisibility(targetRoom, 0f);
        //ResetMaterial(targetWalls);
        //ResetMaterial(currentWalls);

        //// Start coroutine
        RoomConnector rm = targetCollider.GetComponent<RoomConnector>();
        // Disable Coroutine if one is already started
        if (rm.roomFadeAway != null)
        {
            rm.fade = false;
            StopCoroutine(rm.roomFadeAway);
            SetRoomVisibility(currentRoom, 1f);
        }
        roomFadeAway = DisableRoom(targetRoom);
        fade = true;
        StartCoroutine(roomFadeAway);

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
        for (float alpha = 1f; alpha > 0f; alpha -= Time.deltaTime)
        {
            if (!fade) break;
            SetRoomVisibility(roomRoot, alpha);
            yield return null;
        }
    }
}

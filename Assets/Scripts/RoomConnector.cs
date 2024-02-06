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

    public void Enter()
    {
        RoomConnector rm = targetCollider.GetComponent<RoomConnector>();
        if (rm.roomFadeAway != null)
        {
            StopCoroutine(roomFadeAway);
            roomFadeAway = null;
        }
        SetRoomVisibility(currentRoom, 1.0f);
        SetRoomVisibility(targetRoom, 1.0f);
        targetWalls.GetComponent<Renderer>().material.color = transparentMtl.color;

    }

    public void Exit()
    {
        currentWalls.GetComponent<Renderer>().material.color = defaultMtl.color;
        roomFadeAway = DisableRoom(targetRoom);
        StartCoroutine(roomFadeAway);
    }

    private void SetRoomVisibility(GameObject roomRoot, float value)
    {
        foreach (Renderer renderer in roomRoot.GetComponentsInChildren<Renderer>())
        {
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
            SetRoomVisibility(roomRoot, alpha);
            yield return null;
        }
    }
}

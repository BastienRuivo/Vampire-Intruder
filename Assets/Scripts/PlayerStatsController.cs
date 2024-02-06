using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatsController : MonoBehaviour
{
    public GameObject currentRoom;
    public static PlayerStatsController instance;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("Multiple PlayerStatsController");
            return;
        }
        instance = this;
    }
}

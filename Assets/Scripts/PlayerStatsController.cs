using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatsController : MonoBehaviour
{
    public static PlayerStatsController instance;

    [Header("Values")]
    public GameObject currentRoom;
    private bool inputLocked = false;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("Multiple PlayerStatsController");
            return;
        }
        instance = this;
    }

    public void LockInput()
    {
        inputLocked= true;
    }

    public void UnlockInput()
    {
        inputLocked= false;
    }

    public bool CanMove()
    {
        return !inputLocked;
    }


}

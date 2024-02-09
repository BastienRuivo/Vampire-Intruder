using UnityEngine;

public class PlayerStateController : MonoBehaviour
{
    private static PlayerStateController instance;

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

    public static PlayerStateController GetInstance()
    {
        return instance;
    }


}

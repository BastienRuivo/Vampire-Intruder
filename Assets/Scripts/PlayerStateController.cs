using UnityEngine;

public class PlayerState : Singleton<PlayerState>
{
  

    [Header("Values")]
    public RoomData currentRoom;
    private bool inputLocked = false;

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

    public GameObject GetPlayer()
    {
        return gameObject;
    }

    public PlayerController GetPlayerController()
    {
        return GetComponent<PlayerController>();
    }

}

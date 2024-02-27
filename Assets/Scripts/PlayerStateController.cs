using UnityEngine;

public class PlayerState : Singleton<PlayerState>
{
  

    [Header("Values")]
    public RoomData currentRoom;
    private bool _endLock = false;
    public GameObject GetPlayer()
    {
        return gameObject;
    }

    public PlayerController GetPlayerController()
    {
        return GetComponent<PlayerController>();
    }

    public void LockEndGame()
    {
        _endLock = true;
    }

    public bool GetEndLock()
    {
        return _endLock;
    }

}

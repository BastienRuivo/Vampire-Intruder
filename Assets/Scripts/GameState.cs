using Interfaces;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TimeProgression
{
    /// <summary>
    /// First event of the game.
    /// </summary>
    Begin,
    /// <summary>
    /// Event marking the middle of the game.
    /// </summary>
    Middle,
    /// <summary>
    /// Last game event tick before the end of the game.
    /// </summary>
    Last,
    /// <summary>
    /// Event when the player get killed.
    /// </summary>
    End
}

public enum GameStatus
{
    Running,
    Paused,
    Ended
}

public class GameState : Singleton<GameState>
{
    public GameStatus status;
    public TimeProgression timeStatus;
    public uint timeEventID;
    public List<Interactible> objectives;
}

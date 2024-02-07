using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using Interfaces;
using UnityEngine;
using UnityEngine.Serialization;

public enum GameProgressionState
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
    
public struct GameState
{
    public readonly GameProgressionState Progression;
    public readonly int EventID;

    public GameState(GameProgressionState progression, int eventID)
    {
        Progression = progression;
        EventID = eventID;
    }
}// todo remove if unused


public class GameController : MonoBehaviour
{
    /// <returns>Game Mode Controller.</returns>
    /// 
    public static GameController GetGameMode()
    {
        if (GameObject.FindGameObjectsWithTag("GameController").Length != 0)
            return GameObject.FindGameObjectsWithTag("GameController")[0].GetComponent<GameController>();
        Debug.LogError("Unable to get Game Mode.");
        return null;
    } //todo check
    
    [Header("Game Duration")]
    [FormerlySerializedAs("GameEventTickTime")] 
    public float gameEventTickTime = 60.0f;
    [FormerlySerializedAs("GameEventTickCount")] 
    public int gameEventTickCount = 12;

    [Header("Rooms")]
    public List<GameObject> rooms;

    private AudioSource _bellAudioSource;
    
    private int _eventCount = 0;
    private float _eventTime = 0.0f;

    private EventDispatcher<int> _gameEventDispatcher = new ();
    private EventDispatcher<GameProgressionState> _gameProgressionEventDispatcher = new ();

    /// <summary>
    /// Finish the level.
    /// </summary>
    public void LeaveLevel()
    {
        //todo implementation of going to the next level, computing the impacts of this level to the next one.
        //may need a singleton "GameState" to save the results from one game to another.
        Debug.Log("Level Completed.");
    }
    
    /// <summary>
    /// End the level on player missing time.
    /// </summary>
    public void TimeOut()
    {
        //todo implementation of going to the next level, computing the impacts of this level to the next one.
        //may need a singleton "GameState" to save the results from one game to another.
        Debug.Log("Level failed (time is up).");
    }
    
    /// <summary>
    /// End the level on player getting caught by a guard.
    /// </summary>
    public void GetCaught()
    {
        //todo implementation of going to the next level, computing the impacts of this level to the next one.
        //may need a singleton "GameState" to save the results from one game to another.
        Debug.Log("Level failed (Captured).");
    }

    public float GetGameTimeProgression()
    {
        return (_eventTime + _eventCount * gameEventTickTime) / (gameEventTickTime * gameEventTickCount);
    }

    /// <summary>
    /// Subscribe to Game Mode's Game Event
    /// </summary>
    /// <param name="observer"></param>
    public void SubscribeToGameEvent(IEventObserver<int> observer)
    {
        _gameEventDispatcher.Subscribe(observer);
    }

    /// <summary>
    /// Unsubscribe from Game Mode's Game Event
    /// </summary>
    /// <param name="observer"></param>
    public void UnsubscribeToGameEvent(IEventObserver<int> observer)
    {
        _gameEventDispatcher.Unsubscribe(observer);
    }

    /// <summary>
    /// Subscribe to Game Mode's Game Progression State Event
    /// </summary>
    /// <param name="observer"></param>
    public void SubscribeToGameProgressionEvent(IEventObserver<GameProgressionState> observer)
    {
        _gameProgressionEventDispatcher.Subscribe(observer);
    }
    
    /// <summary>
    /// Unsubscribe from Game Mode's Game Progression State Event
    /// </summary>
    /// <param name="observer"></param>
    public void UnsubscribeToGameProgressionEvent(IEventObserver<GameProgressionState> observer)
    {
        _gameProgressionEventDispatcher.Unsubscribe(observer);
    }
    
    private class TestEventReceiver : IEventObserver<int>
    {
        public void OnEvent(int context)
        {
            Debug.Log($"On game Event, Event {context}");
        }
    }

    void Start()
    {
        _eventTime = gameEventTickTime;
        _bellAudioSource = GetComponent<AudioSource>();
        
        SubscribeToGameEvent(new TestEventReceiver());
        HideOtherMaps();
    }

    private void HideOtherMaps()
    {
        rooms.Where(obj => obj.name != PlayerStatsController.instance.currentRoom.name).ToList().ForEach(obj =>
        {
            var renderers = obj.GetComponentsInChildren<Renderer>();
            foreach (var r in renderers)
            {
                Color c = r.material.color;
                c.a = 0f;
                r.material.color = c;
            }
        });
    }

    // Update is called once per frame
    void Update()
    {
        _eventTime += Time.deltaTime;
        if (_eventTime < gameEventTickTime)
            return;
        
        _bellAudioSource.Play();
        
        _gameEventDispatcher.BroadcastEvent(_eventCount);
        if (_eventCount < gameEventTickCount)
        {
            if(_eventCount == 0)
                _gameProgressionEventDispatcher.BroadcastEvent(GameProgressionState.Begin);
            else if (_eventCount == gameEventTickCount/2)
                _gameProgressionEventDispatcher.BroadcastEvent(GameProgressionState.Middle);
            else if (_eventCount == gameEventTickCount - 1)
                _gameProgressionEventDispatcher.BroadcastEvent(GameProgressionState.Last);
        }
        else
        {
            _gameProgressionEventDispatcher.BroadcastEvent(GameProgressionState.End);
            TimeOut();
        }
        
        _eventTime -= gameEventTickTime;
        _eventCount++;
    }
}

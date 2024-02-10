using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using Interfaces;
using UnityEngine;
using UnityEngine.Serialization;





public class GameController : Singleton<GameController>
{
    /// <returns>Game Mode Controller.</returns>
    public static GameController GetGameMode()
    {
        return GetInstance();
    }
    
    /// <returns>Game State</returns>
    public static GameState GetGameState()
    {
        return GameState.GetInstance();
    }

    [Header("Game Duration")]
    [FormerlySerializedAs("GameEventTickTime")] 
    public float gameEventTickTime = 60.0f;
    [FormerlySerializedAs("GameEventTickCount")] 
    public int gameEventTickCount = 12;

    [Header("Rooms")]
    public List<GameObject> rooms;

    [FormerlySerializedAs("TimeBell")] public AudioClip timeBell;
    [FormerlySerializedAs("CaughtBell")] public AudioClip caughtBell;
    
    private int _eventCount = 0;
    private float _eventTime = 0.0f;

    private readonly EventDispatcher<int> _gameEventDispatcher = new ();
    private readonly EventDispatcher<TimeProgression> _gameProgressionEventDispatcher = new ();


    /// <summary>
    /// Finish the level.
    /// </summary>
    public void LeaveLevel()
    {
        GetGameState().status = GameStatus.Ended;

        //todo implementation of going to the next level, computing the impacts of this level to the next one.
        //may need a singleton "GameState" to save the results from one game to another.
        GameEndingManager.instance.onPlayerVictory();
        Debug.Log("Level Completed.");
    }
    
    /// <summary>
    /// End the level on player missing time.
    /// </summary>
    public void TimeOut()
    {
        GetGameState().status = GameStatus.Ended;
        //todo implementation of going to the next level, computing the impacts of this level to the next one.
        //may need a singleton "GameState" to save the results from one game to another.
        GameEndingManager.instance.onPlayerDeath(GameEndingManager.GameOverState.TimeEnd);
        Debug.Log("Level failed (time is up).");
    }
    
    /// <summary>
    /// End the level on player getting caught by a guard.
    /// </summary>
    public void GetCaught()
    {
        // Vérifiez si l'instance de GameEndingManager existe avant d'y accéder
        GetGameState().status = GameStatus.Ended;
        if (GameEndingManager.instance != null)
        {
            AudioManager.GetInstance().playClip(caughtBell, transform.position);
            GameEndingManager.instance.onPlayerDeath(GameEndingManager.GameOverState.Caught);
        }
        else
        {
            Debug.LogError("GameEndingManager instance is null!");
        }
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
    public void SubscribeToGameProgressionEvent(IEventObserver<TimeProgression> observer)
    {
        _gameProgressionEventDispatcher.Subscribe(observer);
    }
    
    /// <summary>
    /// Unsubscribe from Game Mode's Game Progression State Event
    /// </summary>
    /// <param name="observer"></param>
    public void UnsubscribeToGameProgressionEvent(IEventObserver<TimeProgression> observer)
    {
        _gameProgressionEventDispatcher.Unsubscribe(observer);
    }
    
    //private class TestEventReceiver : IEventObserver<int>
    //{
    //    public void OnEvent(int context)
    //    {
    //        Debug.Log($"On game Event, Event {context}");
    //    }
    //}

    void Start()
    {
        _eventTime = gameEventTickTime;
        
        //SubscribeToGameEvent(new TestEventReceiver());
        HideOtherMaps();
        
    }


    private void HideOtherMaps()
    {
        rooms.Where(obj => obj.name != PlayerState.GetInstance().currentRoom.name).ToList().ForEach(obj =>
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

    private void UpdateGameStatus(){
        _eventTime += Time.deltaTime;
        if (_eventTime < gameEventTickTime)
            return;
        
        AudioManager.GetInstance().playClip(timeBell, transform.position);
        
        _gameEventDispatcher.BroadcastEvent(_eventCount);
        if (_eventCount < gameEventTickCount)
        {
            if(_eventCount == 0)
                _gameProgressionEventDispatcher.BroadcastEvent(TimeProgression.Begin);
            else if (_eventCount == gameEventTickCount/2)
                _gameProgressionEventDispatcher.BroadcastEvent(TimeProgression.Middle);
            else if (_eventCount == gameEventTickCount - 1)
                _gameProgressionEventDispatcher.BroadcastEvent(TimeProgression.Last);
        }
        else
        {
            _gameProgressionEventDispatcher.BroadcastEvent(TimeProgression.End);
            TimeOut();
        }
        
        _eventTime -= gameEventTickTime;
        _eventCount++;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateGameStatus();
    }
}

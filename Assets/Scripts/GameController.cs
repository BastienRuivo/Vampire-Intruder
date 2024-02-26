using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using Interfaces;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

public class GameController : Singleton<GameController>
{
    public enum ObjectiveState
    {
        UKNOWN_POS,
        DISCOVERED,
        PLAYER_AT,
        DONE
    }

    public enum ObjectiveEvent
    {
        IN_RANGE,
        OUT_RANGE,
        COMPLETE
    }

    public class Objective
    {
        public bool isMain;
        public ObjectiveState state;
        public string reference;
        public string phrase;
        public Objective(bool isMain, string reference, string phrase, ObjectiveState state)
        {
            this.isMain = isMain;
            this.reference = reference;
            this.phrase = phrase;
            this.state = state;
        }
    }

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
    public List<RoomData> rooms;
    public bool shouldGenerateLvl = true;
    public List<RoomData> activesRoom = new List<RoomData>();
    bool _hasLevelLoaded = false;

    [FormerlySerializedAs("TimeBell")] public AudioClip timeBell;
    [FormerlySerializedAs("CaughtBell")] public AudioClip caughtBell;

    public List<Objective> objectivesToComplete = new List<Objective>();
    private Objective _main;
    
    private int _eventCount = 0;
    private float _eventTime = 0.0f;

    public string mainObjectiveReference;
    public int nbObjectives;

    private readonly EventDispatcher<int> _gameEventDispatcher = new ();
    private readonly EventDispatcher<TimeProgression> _gameProgressionEventDispatcher = new ();
    private readonly EventDispatcher<UserMessageData> _gameUserMessageEventDispatcher = new ();



    /// <summary>
    /// Finish the level.
    /// </summary>
    public void LeaveLevel()
    {
        GetGameState().status = GameStatus.Ended;

        //todo implementation of going to the next level, computing the impacts of this level to the next one.
        //may need a singleton "GameState" to save the results from one game to another.
        GameEndingManager.instance.onPlayerExtraction();
        Debug.Log("Level Completed.");
    }
    
    public void SetObjectives()
    {
        var objectives = GameObject.FindGameObjectsWithTag("Interactible").Select(x => x.GetComponent<Interactible>()).ToList();
        Debug.Log(objectives.Count + " on map");
        List<string> chosenRefs = new List<string>();
        List<Interactible> rejected = new List<Interactible>();
        objectives.ForEach(o =>
        {

            bool isMain = mainObjectiveReference.Equals(o.reference);
            if (chosenRefs.Contains(o.reference))
            {
                o.SetInactive();
            }
            else if (isMain || (nbObjectives > 0 && UnityEngine.Random.Range(0, 100) > 75))
            {
                o.isMainObjective= isMain;
                Objective obj = new Objective(o.isMainObjective, o.reference, o.objectivePhrase, ObjectiveState.UKNOWN_POS);
                if (isMain) _main = obj;
                objectivesToComplete.Add(obj);
                chosenRefs.Add(o.reference);
                nbObjectives--;
                o.SetActive();
            }
            else if(!isMain)
            {
                rejected.Add(o);
                o.SetInactive();
            }
            
        });

        while(nbObjectives > 0 && rejected.Count > 0)
        {
            int r = UnityEngine.Random.Range(0, rejected.Count);
            Interactible o = rejected.ElementAt(r);
            rejected.RemoveAt(r);
            if (chosenRefs.Contains(o.reference))
            {
                continue;
            }

            Objective obj = new Objective(o.isMainObjective, o.reference, o.objectivePhrase, ObjectiveState.UKNOWN_POS);
            objectivesToComplete.Add(obj);
            chosenRefs.Add(o.reference);
            nbObjectives--;
        }

        if(nbObjectives > 0)
        {
            Debug.Log("Can't add enough objectives");
        }

        Debug.Log("There is " + objectivesToComplete.Count + " Objectives");
        objectives.Sort((a, b) =>
        {
            if (a.isMainObjective) return 1;
            else if(b.isMainObjective) return -1;
            else return a.reference.CompareTo(b.reference);
        });
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

    /// <summary>
    /// End the level on player getting out of blood
    /// </summary>
    public void GetDesiccated()
    {
        // Vérifiez si l'instance de GameEndingManager existe avant d'y accéder
        GetGameState().status = GameStatus.Ended;
        if (GameEndingManager.instance != null)
        {
            AudioManager.GetInstance().playClip(caughtBell, transform.position);
            GameEndingManager.instance.onPlayerDeath(GameEndingManager.GameOverState.Dead);
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
    
    public struct UserMessageData
    {
        public UserMessageData(MessageToUserSenderType sender, string message,  float duration = 2.0f, MessageToUserScheduleType priority = MessageToUserScheduleType.Regular)
        {
            Sender = sender;
            Priority = priority;
            Message = message;
            Duration = duration;
        }

        /// <summary>
        /// describes who is sending the message
        /// </summary>
        public enum MessageToUserSenderType
        {
            Player,
            Guard
        }//todo | extension : add other sender type if needed
        
        /// <summary>
        /// describe the priority of the message
        /// </summary>
        public enum MessageToUserScheduleType
        {
            /// <summary>
            /// Message will be placed last on the message queue.
            /// </summary>
            Regular,
            /// <summary>
            /// Message will be placed as first on message queue (behind a ImportanceOnTiming message)
            /// </summary>
            ImportanceOnReadability,
            /// <summary>
            /// Message will interrupt Current Message as it is required for the user to see fast. These
            /// messages are not enqueued so if it gets interrupted, it wont be recovered after this.
            /// </summary>
            ImportanceOnTiming
        }//todo | extension : add other sender type if needed

        public MessageToUserSenderType Sender { get;}
        public MessageToUserScheduleType Priority { get;}
        public string Message { get; }
        public float Duration { get; }
    }
    
    public void MessageToUser(UserMessageData message)
    {
        _gameUserMessageEventDispatcher.BroadcastEvent(message);
    }
    
    /// <summary>
    /// Subscribe to Game Mode's Game User Message events
    /// </summary>
    /// <param name="observer"></param>
    public void SubscribeToGameUserMessageEvent(IEventObserver<UserMessageData> observer)
    {
        _gameUserMessageEventDispatcher.Subscribe(observer);
    }
    
    /// <summary>
    /// Unsubscribe from Game Mode's Game User Message events
    /// </summary>
    /// <param name="observer"></param>
    public void UnsubscribeToGameUserMessageEvent(IEventObserver<UserMessageData> observer)
    {
        _gameUserMessageEventDispatcher.Unsubscribe(observer);
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
        // Generate map
        if(shouldGenerateLvl)
        {
            PlayerState.GetInstance().LockInput();
            RoomData hall = LevelGenerator.GetInstance().Generate(mainObjectiveReference);

            PlayerState.GetInstance().currentRoom = hall;
            var start = hall.transform.Find("CustomPivot/Start");
            PlayerState.GetInstance().GetPlayer().transform.position = start.transform.position;
        }
        else
        {
            OnLevelLoadComplete(rooms);

        }
    }

    private void GenerateAStarGraph()
    {
        Debug.Log("There is " + rooms.Count + " rooms to build");
        rooms.ForEach(room =>
        {
            room.BuildGraph(room.transform.position);
        });
        AstarPath.active.Scan();
    }


    private void HideOtherMaps()
    {
        var otherRooms = rooms.Where(obj => obj.name != PlayerState.GetInstance().currentRoom.name).ToList();
        Debug.Log("Current player room is " + PlayerState.GetInstance().currentRoom.name + " there is " + otherRooms.Count + " rooms");
        otherRooms.ForEach(obj =>
        {
            var renderers = obj.GetComponentsInChildren<Renderer>();
            foreach (var r in renderers)
            {
                if(r.material.HasColor("_Color"))
                {
                    Color c = r.material.color;
                    c.a = 0f;
                    r.material.color = c;
                }
                else
                {
                    Debug.Log(r.gameObject.name);
                }
            }
        });
    }

    public void OnLevelLoadComplete(List<RoomData> rooms)
    {
        this.rooms = rooms;
        Debug.Log("Level loaded with " + rooms.Count + " rooms");
        GenerateAStarGraph();
        countGard();
        HideOtherMaps();
        SetObjectives();

        OnRoomChange(rooms[0]);

        PlayerState.GetInstance().UnlockInput();



        _hasLevelLoaded = true;
    }

    private void countGard(){
        int count = 0;
        foreach (RoomData room in rooms)
        {
            count += room.guards.Count;
        }
        AppState.GetInstance().setTotalGuardsInCurrentScene(count);
    }

    private void UpdateGameStatus(){
        if(!_hasLevelLoaded) return;
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

    public void UpdateObjective(string reference, ObjectiveEvent ev)
    {
        var obj = objectivesToComplete.First(x => x.reference == reference);
        switch(ev)
        {
            case ObjectiveEvent.IN_RANGE: obj.state = ObjectiveState.PLAYER_AT; break;
            case ObjectiveEvent.OUT_RANGE: obj.state = ObjectiveState.DISCOVERED; break;
            case ObjectiveEvent.COMPLETE: obj.state = ObjectiveState.DONE; break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        UpdateGameStatus();
    }

    public void OnRoomChange(RoomData activeRoot)
    {

        activesRoom = activeRoot.GetConnectedRooms();
        rooms.ForEach(r =>
        {
            r.gameObject.SetActive(activesRoom.Contains(r));
            r.guards.ForEach(g =>
            {
                g.AskPathUpdate();
            });
        });
    }

    public bool IsLevelLoaded()
    {
        return _hasLevelLoaded;
    }

    public bool HasObtainedMainObjective
    {
        get
        {
            return _main.state == ObjectiveState.DONE;
        }
    }
}

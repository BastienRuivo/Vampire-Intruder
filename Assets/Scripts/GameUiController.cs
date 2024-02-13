using System;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using Interfaces;
using JetBrains.Annotations;
using Systems.Ability;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class GameUiController : MonoBehaviour, 
    IEventObserver<TimeProgression>, IEventObserver<GameObject>, IEventObserver<GameController.UserMessageData>
{
    [FormerlySerializedAs("JessikaBeginQuote")] [Header("Jessika's Quotes")] public string jessikaBeginQuote;
    [FormerlySerializedAs("JessikaMiddleQuote")] [Header("Jessika's Quotes")] public string jessikaMiddleQuote;
    [FormerlySerializedAs("JessikaFinalQuote")] [Header("Jessika's Quotes")] public string jessikaFinalQuote;

    private readonly SmoothScalarValue _playerBloodStatSmooth = new SmoothScalarValue(1);
    private readonly SmoothScalarValue _playerBloodStatEyeEffectSmooth =  new SmoothScalarValue(0, 5f);

    private GameObject _healthBarPanel;
    private GameObject _eyeballBarPanel;

    private struct MessageQueueElement
    {
        private readonly GameController.UserMessageData _messageData;
        private float _remainingTime;

        public MessageQueueElement(GameController.UserMessageData message)
        {
            _messageData = message;
            _remainingTime = message.Duration;
        }
        
        public GameController.UserMessageData GetMessage() {return _messageData;}
        
        public float GetRemainingTime() {return _remainingTime;}
        
        public void AdvanceTime(float t) {_remainingTime-=t;}
    }

    private readonly LinkedList<MessageQueueElement> _messageQueue = new ();
    [CanBeNull] private IEnumerator _currentDialogRoutine = null;
    private MessageQueueElement _currentDialog;

    // Start is called before the first frame update
    void Start()
    {
        GameController.GetGameMode().SubscribeToGameProgressionEvent(this);
        GameController.GetGameMode().SubscribeToGameUserMessageEvent(this);
        PlayerController.GetPlayer().GetComponent<AbilitySystemComponent>().SubscribeToStatChanges(this);
        
        
        foreach (Transform child in transform)
        {
            GameObject childGameObject = child.gameObject;
            if (childGameObject.CompareTag("GameUIHealthBar"))
            {
                _healthBarPanel = childGameObject;
            }
            
            if (childGameObject.CompareTag("GameUIEyeBall"))
            {
                _eyeballBarPanel = childGameObject;
            }
        }//todo switch to public values set in the prefab
    }

    // Update is called once per frame
    void Update()
    {
        _healthBarPanel.GetComponent<Image>().material.SetFloat("_Progression",_playerBloodStatSmooth.UpdateGetValue());
        _eyeballBarPanel.GetComponent<Image>().material.SetFloat("_Damage",_playerBloodStatEyeEffectSmooth.UpdateGetValue());

        HandleMessageUpdate();
    }

    private void HandleMessageUpdate()
    {
        if (_currentDialogRoutine != null)
        {
            _currentDialog.AdvanceTime(Time.deltaTime);
            if (_currentDialog.GetRemainingTime() <= 0)
            {
                _currentDialogRoutine = null;
            }
        }
        
        //next dialog
        if(_messageQueue.Count == 0 || _currentDialogRoutine != null) return;
        NextDialog();
    }

    private void NextDialog()
    {
        _currentDialog = _messageQueue.First.Value;
        _messageQueue.RemoveFirst();

        string sender = "";
        Color color = new Color(1,1,1);
        switch (_currentDialog.GetMessage().Sender)
        {
            case GameController.UserMessageData.MessageToUserSenderType.Player:
                sender = "Jessika";
                break;
            case GameController.UserMessageData.MessageToUserSenderType.Guard:
                sender = "Guard";
                color = new Color(0.87f,0.87f,0.87f);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        _currentDialogRoutine = DialogRoutine($"{sender} : {_currentDialog.GetMessage().Message}", color, _currentDialog.GetRemainingTime());
        StartCoroutine(_currentDialogRoutine);
    }

    private void EnqueueMessage(GameController.UserMessageData message)
    {
        switch (message.Priority)
        {
            case GameController.UserMessageData.MessageToUserScheduleType.Regular:
                _messageQueue.AddLast(new MessageQueueElement(message));
                break;
            case GameController.UserMessageData.MessageToUserScheduleType.ImportanceOnReadability:
                _messageQueue.AddFirst(new MessageQueueElement(message));
                break;
            case GameController.UserMessageData.MessageToUserScheduleType.ImportanceOnTiming:
                if (_currentDialogRoutine != null)
                {
                    if(_currentDialog.GetMessage().Priority != GameController.UserMessageData.MessageToUserScheduleType.ImportanceOnTiming)
                        _messageQueue.AddFirst(_currentDialog);
                    StopCoroutine(_currentDialogRoutine);
                    _currentDialogRoutine = null;
                }
                _messageQueue.AddFirst(new MessageQueueElement(message));
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    private IEnumerator DialogRoutine(string text, Color color, float dialogLineDuration = 5.0f)
    {
        GetComponentInChildren<TextMeshProUGUI>().SetText(text);
        GetComponentInChildren<TextMeshProUGUI>().color = color;
        GetComponentInChildren<Animator>().SetBool("ShowDialog", true);
        yield return new WaitForSeconds(dialogLineDuration);
        GetComponentInChildren<Animator>().SetBool("ShowDialog", false);
    }

    public void OnEvent(TimeProgression context)
    {
        switch (context)
        {
            case TimeProgression.Begin:
                EnqueueMessage(new GameController.UserMessageData(
                    GameController.UserMessageData.MessageToUserSenderType.Player,
                    $"Jessika : \"{jessikaBeginQuote}\"",
                    5.0f
                    ));
                //StartCoroutine(DialogRoutine($"Jessika : \"{jessikaBeginQuote}\"", new Color(1,1,1)));
                break;
            case TimeProgression.Middle:
                EnqueueMessage(new GameController.UserMessageData(
                    GameController.UserMessageData.MessageToUserSenderType.Player,
                    $"Jessika : \"{jessikaMiddleQuote}\"",
                    5.0f
                ));
                //StartCoroutine(DialogRoutine($"Jessika : \"{jessikaMiddleQuote}\"", new Color(1,1,1)));
                break;
            case TimeProgression.Last:
                EnqueueMessage(new GameController.UserMessageData(
                    GameController.UserMessageData.MessageToUserSenderType.Player,
                    $"Jessika : \"{jessikaFinalQuote}\"",
                    5.0f
                ));
                //StartCoroutine(DialogRoutine($"Jessika : \"{jessikaFinalQuote}\"", new Color(1,1,1)));
                break;
            case TimeProgression.End:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(context), context, null);
        }
    }

    public void OnEvent(GameObject context)
    {
        AbilitySystemComponent ascRef = context.GetComponent<AbilitySystemComponent>();
        
        float blood = ascRef.QueryStat("Blood");
        float maxBlood = ascRef.QueryStat("BloodMax");
        float bloodRatio = blood / maxBlood;
        bloodRatio = bloodRatio > 0? bloodRatio : 0.0f;
        
        _playerBloodStatSmooth.RetargetValue(bloodRatio);
        _playerBloodStatEyeEffectSmooth.RetargetValue(bloodRatio * 4 > 1 ? 0.0f : 1 - (bloodRatio * 4.0f));
    }

    public void OnEvent(GameController.UserMessageData context)
    {
        EnqueueMessage(context);
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using Interfaces;
using JetBrains.Annotations;
using Systems.Ability;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class GameUiController : MonoBehaviour, 
    IEventObserver<TimeProgression>, IEventObserver<GameObject>, IEventObserver<GameController.UserMessageData>
{
    public GameObject jessika;
    [Header("Jessika's Quotes")] 
    public Color jessikaQuotationColor;
    public Color guardsQuotationColor;
    [FormerlySerializedAs("JessikaBeginQuote")] public string jessikaBeginQuote;
    [FormerlySerializedAs("JessikaMiddleQuote")] public string jessikaMiddleQuote;
    [FormerlySerializedAs("JessikaFinalQuote")] public string jessikaFinalQuote;
    
    [Header("UI elements")]
    public GameObject healthBarPanel;
    public GameObject eyeballBarPanel;
    public GameObject objectivesPanel;
    public GameObject quotationTextBox;
    public GameObject levelLoadingPanel;
    public GameObject primaryAbilityIndicator;
    public GameObject secondaryAbilityIndicator;
    public GameObject abilitySelector;
    public GameObject abilitySelectorHintImage;
    public GameObject abilitySelectorHintText;
    public GameObject textBackgroundPanel;

    [Header("Objectives")]
    public float upSpeed;
    public float downSpeed;
    [Range(0f, 1f)]
    public float objectivesHeightOnScreen;

    private readonly SmoothScalarValue _playerBloodStatSmooth = new SmoothScalarValue(1);
    private readonly SmoothScalarValue _playerBloodStatEyeEffectSmooth =  new SmoothScalarValue(0, 5f);
    
    private float _objectiveDownY;
    private float _objectiveUpY;
    private bool _isObjectiveDisplayed;
    private IEnumerator _objectiveDisplayCoroutine;
    private IEnumerator _levelCompleteFade;

    private AbilitySystemComponent _playerASC;
    private GameUIAbilityIconController _abilityAIndicator;
    private GameUIAbilityIconController _abilityEIndicator;
    private GameUIAbilitySelectorController _abilitySelector;
    
    private readonly SmoothScalarValue _abilitySelectorOpacity = new (0);
    private bool _hasLock = false;

    private bool hasAbility = true;

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

    private void Awake()
    {
        _playerASC = jessika.GetComponent<AbilitySystemComponent>();
        _abilityAIndicator = primaryAbilityIndicator.GetComponent<GameUIAbilityIconController>();
        _abilityEIndicator = secondaryAbilityIndicator.GetComponent<GameUIAbilityIconController>();
        _abilitySelector = abilitySelector.GetComponent<GameUIAbilitySelectorController>();
    }

    // Start is called before the first frame update
    void Start()
    {
        GameController.GetGameMode().SubscribeToGameProgressionEvent(this);
        GameController.GetGameMode().SubscribeToGameUserMessageEvent(this);
        _playerASC.SubscribeToStatChanges(this);
        
        _abilityAIndicator.SetAbilitySystemComponent(_playerASC);
        _abilityAIndicator.SetAbilityTag(_playerASC.GetAbilityByBinding(KeyCode.Q));
        _abilityEIndicator.SetAbilitySystemComponent(_playerASC);
        _abilityEIndicator.SetAbilityTag(_playerASC.GetAbilityByBinding(KeyCode.E));

        if (_playerASC.GetAbilityByBinding(KeyCode.E) == null)
        {
            _abilityEIndicator.gameObject.SetActive(false);
            abilitySelectorHintText.SetActive(false);
            abilitySelectorHintImage.SetActive(false);
            hasAbility = false;
        }

        _abilitySelector.SetAbilitySystemComponent(_playerASC);
        _abilitySelector.UpdateAbilityUIBase();
        _abilitySelector.SetOpacity(0);
        
        _objectiveDownY = objectivesPanel.transform.localPosition.y;
        _objectiveUpY = _objectiveDownY * objectivesHeightOnScreen;
        _isObjectiveDisplayed = false;
    }

    // Update is called once per frame
    void Update()
    {
        healthBarPanel.GetComponent<Image>().material.SetFloat("_Progression",_playerBloodStatSmooth.UpdateGetValue());
        eyeballBarPanel.GetComponent<Image>().material.SetFloat("_Damage",_playerBloodStatEyeEffectSmooth.UpdateGetValue());

        _objectiveUpY = _objectiveDownY * objectivesHeightOnScreen;
        HandleMessageUpdate();

        if(Input.GetKeyDown(KeyCode.O))
        {
            _isObjectiveDisplayed = !_isObjectiveDisplayed;
            if (_objectiveDisplayCoroutine != null) 
                StopCoroutine(_objectiveDisplayCoroutine);
            _objectiveDisplayCoroutine = _isObjectiveDisplayed ? DisplayObjective() : HideObjective();
            StartCoroutine(_objectiveDisplayCoroutine);
        }

        _abilitySelector.SetOpacity(_abilitySelectorOpacity.UpdateGetValue());
        _abilityEIndicator.SetOpacity(1 - _abilitySelectorOpacity.GetValue());
        
        if(!hasAbility)
            return;
        
        TextMeshProUGUI hintText = abilitySelectorHintText.GetComponent<TextMeshProUGUI>();
        Color hintTxtCol = hintText.color;
        hintTxtCol.a = 1 - _abilitySelectorOpacity.GetValue();
        hintText.color = hintTxtCol;
            
        Image hintImg = abilitySelectorHintImage.GetComponent<Image>();
        Color hintImgCol = hintImg.color;
        hintImgCol.a = 1 - _abilitySelectorOpacity.GetValue();
        hintImg.color = hintImgCol;
        
        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            if (!_hasLock)
            {
                _abilitySelectorOpacity.RetargetValue(1);
                _playerASC.GetInputLock().Take();
                _hasLock = true;
            }
        }

        if (_hasLock && Input.GetMouseButtonDown(0))
        {
            _abilitySelectorOpacity.RetargetValue(0);
            _playerASC.GetInputLock().Release();
            _abilityEIndicator.SetAbilityTag(_playerASC.GetAbilityByBinding(KeyCode.E));
            _hasLock = false;
        }
    }

    IEnumerator DisplayObjective()
    {
        while(objectivesPanel.transform.localPosition.y < _objectiveUpY)
        {
            var pos = objectivesPanel.transform.localPosition;
            pos.y = Mathf.Lerp(objectivesPanel.transform.localPosition.y, _objectiveUpY, Time.deltaTime * upSpeed);
            objectivesPanel.transform.localPosition = pos;
            yield return null;
        }
    }

    IEnumerator HideObjective()
    {
        while (objectivesPanel.transform.localPosition.y > _objectiveDownY)
        {
            var pos = objectivesPanel.transform.localPosition;
            pos.y = Mathf.Lerp(objectivesPanel.transform.localPosition.y, _objectiveDownY, Time.deltaTime * downSpeed);
            objectivesPanel.transform.localPosition = pos;
            yield return null;
        }
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
        Color color = jessikaQuotationColor;
        switch (_currentDialog.GetMessage().Sender)
        {
            case GameController.UserMessageData.MessageToUserSenderType.Player:
                sender = "Jessika";
                break;
            case GameController.UserMessageData.MessageToUserSenderType.Guard:
                sender = "Guard";
                color = guardsQuotationColor;
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
        quotationTextBox.GetComponent<TextMeshProUGUI>().SetText(text);
        quotationTextBox.GetComponent<TextMeshProUGUI>().color = color;
        textBackgroundPanel.GetComponent<Animator>().SetBool("ShowDialog", true);
        yield return new WaitForSeconds(dialogLineDuration);
        textBackgroundPanel.GetComponent<Animator>().SetBool("ShowDialog", false);
    }

    public void OnEvent(TimeProgression context)
    {
        switch (context)
        {
            case TimeProgression.Begin:
                _levelCompleteFade = UpdateLevelLoadScreen();
                StartCoroutine(_levelCompleteFade);
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
        //Debug.Log("Blood (L + ) ratio is " + bloodRatio);
        
        bloodRatio = bloodRatio > 0? bloodRatio : 0.0f;
        
        _playerBloodStatSmooth.RetargetValue(bloodRatio);
        _playerBloodStatEyeEffectSmooth.RetargetValue(bloodRatio * 4 > 1 ? 0.0f : 1 - (bloodRatio * 4.0f));
    }

    public void OnEvent(GameController.UserMessageData context)
    {
        EnqueueMessage(context);
    }

    public IEnumerator UpdateLevelLoadScreen()
    {

        Image im = levelLoadingPanel.GetComponent<Image>();
        while(im.color.a > 0)
        {
            Color c = im.color;
            c.a -= Time.deltaTime * 0.5f;
            im.color = c;
            yield return null;
        }
    }
}

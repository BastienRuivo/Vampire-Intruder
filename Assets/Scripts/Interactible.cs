using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;

public class Interactible : MonoBehaviour
{
    public enum InteractibleType
    {
        TIMER,
        TIMER_RESET,
        TIMER_LOCKED
    };

    public enum AfterUse
    {
        NOTHING,
        REPLACE_SPRITE,
        DISAPPEAR,
        END_LVL
    };

    [Header("Interactible")]
    public InteractibleType type;
    public AfterUse afterUse;
    public Sprite replacementSprite;
    public string reference;
    public string objectivePhrase;
    public bool isMainObjective;
    public Color passiveGlowColor;
    public Color glowColor;
    public Color startColor;
    public Color endColor;

    private SpriteRenderer _clock;
    private SpriteRenderer _keyTooltip;
    private SpriteRenderer _icon;

    private Glow[] _glows;

    [Header("Timer")]
    public float duration;
    [SerializeField]
    private float _currentTime;
    [SerializeField]
    private bool _isActive;

    [Header("TagList")]
    public List<TagAttribute> tags;

    public string dialogOnStart = "";

    private bool _isColliding = false;
    private bool _interacted = false;

    private SpriteRenderer _spriteRenderer;


    private void Awake()
    {
        _currentTime = duration;
        _isActive = true;
        _clock = transform.Find("Clock").GetComponent<SpriteRenderer>();
        _keyTooltip = transform.Find("Key").GetComponent<SpriteRenderer>();
        _icon = transform.Find("Icon").GetComponent<SpriteRenderer>();
        _spriteRenderer = GetComponent<SpriteRenderer>();

        _glows = GetComponents<Glow>();

        _clock.material.SetColor("_Start_Color", startColor);
        _clock.material.SetColor("_End_Color", endColor);
        _isColliding = false;
    }

    public void StartCollision()
    {
        if (!GameController.GetInstance().IsLevelLoaded()) return;
        _isColliding = true;
        if (_isActive)
        {
            if (afterUse == AfterUse.END_LVL)
            {
                GameController.GetGameMode().MessageToUser(
                    new GameController.UserMessageData(
                        GameController.UserMessageData.MessageToUserSenderType.Player,
                        GameController.GetInstance().HasObtainedMainObjective ? "J'ai accomplie ma mission, je peut partir !" : "Si je pars sans remplir ma mission, je vais décevoir le prince...",
                        priority: GameController.UserMessageData.MessageToUserScheduleType.ImportanceOnReadability)
                );

                _glows[0].effectColor = GameController.GetInstance().HasObtainedMainObjective ? Color.green : Color.red;
                Debug.Log(GameController.GetInstance().HasObtainedMainObjective.ToString() + " " + _glows[0].effectColor.ToString());
            }
            else if(isMainObjective || !string.IsNullOrEmpty(dialogOnStart))
            {
                GameController.GetGameMode().MessageToUser(
                    new GameController.UserMessageData(
                        GameController.UserMessageData.MessageToUserSenderType.Player,
                        isMainObjective? "Vite ! C'est l'objectif pour le quel je suis ici" : dialogOnStart,
                        priority: GameController.UserMessageData.MessageToUserScheduleType.ImportanceOnReadability)
                );
            }
            _glows[1].Deactivate();
            _glows[0].Activate();
            DisplayTooltips(true);
            if (reference.Length > 0) GameController.GetInstance().UpdateObjective(reference, GameController.ObjectiveEvent.IN_RANGE);
            
        }
    }

    public void EndCollision()
    {
        _isColliding= false;
        if (_isActive)
        {
            _glows[0].Deactivate();
            _glows[1].Activate();
            DisplayTooltips(false);
            if (reference.Length > 0) GameController.GetInstance().UpdateObjective(reference, GameController.ObjectiveEvent.OUT_RANGE);
        }
            
    }

    public void DisplayTooltips(bool showTooltips)
    {
        _clock.enabled = showTooltips;
        _keyTooltip.enabled = showTooltips;
        _icon.enabled = showTooltips;
    }

    private void Update()
    {
        if (!_isActive) return;
        var axis = Input.GetAxis("Interact");

        
        

        if (type == InteractibleType.TIMER_LOCKED)
        {
            if (!_interacted)
            {
                _interacted = axis == 1f && _isColliding;
            }
        }
        else
        {
            bool v = axis == 1f && _isColliding;
            if (v != _interacted)
            {
                _interacted = v;
                if(_interacted) PlayerState.GetInstance().GetPlayerController().LockInput();
                else PlayerState.GetInstance().GetPlayerController().UnlockInput();
            }
        }

        if(_currentTime > 0f)
        {
            if (_isColliding && _interacted)
            {
                _currentTime -= Time.deltaTime;
            }
            else if (type == InteractibleType.TIMER_RESET)
            {
                _currentTime = duration;
            }
            if(_clock != null)
            {
                _clock.material.SetFloat("_t", 1f - _currentTime / duration);
            }
        }
        else
        {
            /// TODO : LAUNCH EVENT
            OnEnd();
        }
    }

    private void OnEnd()
    {
        PlayerState.GetInstance().GetPlayerController().UnlockInput();
        _isActive = false;
        DisplayTooltips(false);
        _glows[0].Deactivate();
        _glows[1].Deactivate();
        if (reference.Length > 0) GameController.GetInstance().UpdateObjective(reference, GameController.ObjectiveEvent.COMPLETE);
        switch (afterUse)
        {
            case AfterUse.REPLACE_SPRITE:
                _spriteRenderer.sprite = replacementSprite;
                break;

            case AfterUse.DISAPPEAR:
                Destroy(gameObject);
                break;

            case AfterUse.END_LVL:
                GameController.GetInstance().LeaveLevel();
                break;

            default: break;
        }
    }

    public void SetInactive()
    {
        _isActive = false;
    }

    public void SetActive()
    {
        _isActive = true;
        if(_glows != null)
        {
            
            _glows[0].Activate();
        }


    }
}

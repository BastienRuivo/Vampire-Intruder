using DefaultNamespace;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Targetable : MonoBehaviour
{
    public enum TargetType
    {
        PLAYER,
        ENEMY,
        ALERTER,
        NOONE
    }

    public TargetType targetType = TargetType.NOONE;
    private bool _isVisible = true;
    public static string TargetableTag = "Targetable";

    public readonly EventDispatcher<Targetable> dispatcher = new EventDispatcher<Targetable>();

    public void ActivateVisibility()
    {
        _isVisible = true;
        // TODO : Launch event enter the cone
    }

    public void DeactivateVisibility()
    {
        _isVisible = false;
        // TODO : Launch event leave the cone
    }
    
    public bool IsVisible
    { 
        get 
        { 
            return _isVisible; 
        } 
    }

    public bool IsVisibleByGuard
    {
        get 
        {
            return _isVisible && (targetType == TargetType.PLAYER || targetType == TargetType.ALERTER);
        }
    }

    public bool IsVisibleByPlayer
    {
        get
        {
            return _isVisible && targetType == TargetType.ENEMY;
        }
    }

    private void OnDestroy()
    {
        dispatcher.BroadcastEvent(this);
    }
}

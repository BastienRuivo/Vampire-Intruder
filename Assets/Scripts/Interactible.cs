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
        DISAPPEAR
    };

    [Header("Interactible")]
    public InteractibleType type;
    public AfterUse afterUse;
    public Sprite replacementSprite;
    public string reference;
    public string objectivePhrase;
    public bool isMainObjective;
    public Color glowColor;
    public GameObject clock;

    [Header("Timer")]
    public float duration;
    [SerializeField]
    private float currentTime;
    [SerializeField]
    private bool isActive;

    [Header("TagList")]
    public List<TagAttribute> tags;

    [SerializeField]
    private bool isColliding = false;
    [SerializeField]
    private bool interacted = false;

    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        currentTime = duration;
        isActive = true;

        spriteRenderer= GetComponent<SpriteRenderer>();
        spriteRenderer.material.SetColor("_Color", glowColor);
        clock.GetComponent<SpriteRenderer>().enabled = false;
    }

    public void StartCollision()
    {
        isColliding= true;
        clock.GetComponent<SpriteRenderer>().enabled = true;
    }

    public void EndCollision()
    {
        isColliding= false;
        clock.GetComponent<SpriteRenderer>().enabled = false;
    }

    private void Update()
    {
        if (!isActive) return;
        var axis = Input.GetAxis("Interact");

        if(isColliding)
        {
            float t = Mathf.PingPong(Time.time, 0.5f);
            spriteRenderer.material.SetFloat("_t", t);
        }
        else
        {
            spriteRenderer.material.SetFloat("_t", 0f);
        }

        if (type == InteractibleType.TIMER_LOCKED)
        {
            if (!interacted)
            {
                interacted = axis == 1f && isColliding;
            }
        }
        else
        {
            bool v = axis == 1f && isColliding;
            if (v != interacted)
            {
                interacted = v;
                if(interacted) PlayerState.GetInstance().LockInput();
                else PlayerState.GetInstance().UnlockInput();
            }
        }

        if(currentTime > 0f)
        {
            if (isColliding && interacted)
            {
                currentTime -= Time.deltaTime;
            }
            else if (type == InteractibleType.TIMER_RESET)
            {
                currentTime = duration;
            }
            if(clock != null)
            {
                clock.GetComponent<SpriteRenderer>().material.SetFloat("_t", 1f - currentTime / duration);
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
        PlayerState.GetInstance().UnlockInput();
        isActive = false;
        spriteRenderer.material.SetFloat("_t", 0f);
        switch (afterUse)
        {
            case AfterUse.REPLACE_SPRITE:
                GetComponent<SpriteRenderer>().sprite = replacementSprite;
                break;

            case AfterUse.DISAPPEAR:
                Destroy(gameObject);
                break;

            default: break;
        }
    }
}

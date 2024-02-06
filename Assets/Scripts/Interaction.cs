using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;

public class Interaction : MonoBehaviour
{
    public enum InteractibleType
    {
        TIMER,
        TIMER_RESET,
        TIMER_LOCKED
    };

    [Header("Interactible")]
    public InteractibleType type;

    [Header("Timer")]
    public float duration;
    private float currentTime;

    [Header("TagList")]
    public List<TagAttribute> tags;

    private bool isColliding = false;
    private bool interacted = false;

    private void Start()
    {
        currentTime = duration;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        isColliding = true;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        isColliding = false;
    }

    private void Update()
    {
        var axis = Input.GetAxis("Interact");

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
                if(interacted) PlayerStatsController.instance.LockInput();
                else PlayerStatsController.instance.UnlockInput();
            }
        }


        if (isColliding && interacted)
        {
            currentTime -= Time.deltaTime;
        }
        else if(type == InteractibleType.TIMER_RESET)
        {
            currentTime = duration;
        }

        if(duration <= 0f)
        {
            /// TODO : LAUNCH EVENT
        }
    }
}

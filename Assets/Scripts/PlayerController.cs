using System.Collections;
using System.Collections.Generic;
using Systems.Ability;
using Systems.Ability.Abilities;
using Systems.Ability.tests;
using UnityEngine;



public class PlayerController : MonoBehaviour
{
    [Header("Speed")]
    public float speed = 20f;

    [Header("Camera")]
    public Camera playerCamera;
    public float speedZoom = 10f;
    public float minZoom = 1f;
    public float maxZoom = 10f;

    // private variables
    private Animator animator;
    private Rigidbody2D rigidBody;
    public Direction directionPerso;
    private AbilitySystemComponent _ascRef;

    private enum State
    {
        Idle,
        Walking
    }

    public static GameObject GetPlayer()
    {
        GameObject[] candidates = GameObject.FindGameObjectsWithTag("Player");
        if (candidates.Length > 0)
            return candidates[0];
        Debug.LogError("Player not found in this scene.");
        return null;
    }

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        rigidBody = GetComponent<Rigidbody2D>();
        
        _ascRef = GetComponent<AbilitySystemComponent>();
        
        //Defines statistics the ASC will work with.
        _ascRef.DefineStat("Blood", baseValue:100.0f, lowerRange:-15.0f);
        _ascRef.DefineStat("BloodMax", baseValue:100.0f, lowerRange:1.0f);
        
        //Grant ability. For consumable ones this will just increment the available charge count if the ability has already been granted.
        _ascRef.GrantAbility<AEffectStatClamp>("_StatClamp");
        _ascRef.GrantAbility<AEffectVampireBite>("Bite");
        _ascRef.GrantAbility<AVampireTryBite>("TryBite");
        _ascRef.GrantAbility<TestAbility>("Test");
        _ascRef.GrantAbility<AVampireThirst>("Thirst");
        _ascRef.GrantAbility<ATeleportation>("TP");
        
        //bind ability to a keyboard input. The ability will then be executed when this key is pressed.
        _ascRef.BindAbility("TryBite", KeyCode.Q);
        _ascRef.BindAbility("TP", KeyCode.E);
    }

    private void ZoomCamera()
    {
        if(playerCamera == null)
        {
            return;
        }
        float dzoom = speedZoom * Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime * 10.0f;
        playerCamera.orthographicSize = Mathf.Clamp(playerCamera.orthographicSize - dzoom, minZoom, maxZoom);
    }

    void Move()
    {
        float hAxis = 0f;
        float vAxis = 0f;
        if (PlayerState.GetInstance().CanMove())
        {
            hAxis = Input.GetAxis("Horizontal");
            vAxis = Input.GetAxis("Vertical");
        }

        Vector3 direction = new Vector3(hAxis, vAxis, 0).normalized;
        direction.y *= 0.5f;

        if (hAxis != 0 && vAxis != 0) 
        {
            directionPerso = DirectionHelper.FromVector(direction);
        }

        rigidBody.velocity = direction * speed * Time.deltaTime;

        if(rigidBody.velocity.magnitude > 0)
        {
            animator.SetInteger("state", (int)State.Walking);
            animator.SetFloat("xSpeed", rigidBody.velocity.x);
            animator.SetFloat("ySpeed", rigidBody.velocity.y);
        }
        else
        {
            animator.SetInteger("state", (int)State.Idle);
        }
    }

    // Update is called once per frame
    void Update()
    {
        ZoomCamera();        
    }

    private void FixedUpdate()
    {
        if (_ascRef.GetInputLock().IsOpened())
        {
            PlayerState.GetInstance().UnlockInput();
        }
        else
        {
            PlayerState.GetInstance().LockInput();
        }
        Move();
    }
}

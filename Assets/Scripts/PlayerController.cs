using System;
using DefaultNamespace;
using Interfaces;
using Systems.Ability;
using Systems.Ability.Abilities;
using Systems.Ability.tests;
using Systems.Vision;
using Systems.Vision.Cone;
using Unity.VisualScripting;
using UnityEngine;



public class PlayerController : MonoBehaviour, IEventObserver<VisionSystemController.OverlapData>
{
    public GameObject visionObject;
    private VisionConeController _coneController;

    enum VisionBehavior
    {
        KEYBOARD,
        MOUSE,
        TARGET
    }
    
    [Header("Speed")]
    public float speed = 20f;

    [Header("Camera")]
    public Camera playerCamera;
    public float speedZoom = 10f;
    [Range(0.1f, 10f)] public float currentZoom;

    // private variables
    private Animator animator;
    private Rigidbody2D rigidBody;
    public Direction directionPerso;//todo cleanup this trash
    
    private AbilitySystemComponent _ascRef;
    private InputToVisionSystemBehavior[] _coneBehaviors;

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
    } //todo remove

    public void BindVisionToMouse()
    {
        _coneBehaviors[(int)VisionBehavior.KEYBOARD].enabled = false;
        _coneBehaviors[(int)VisionBehavior.MOUSE].enabled = true;
    }
    
    public void UnbindVisionFromMouse()
    {
        _coneBehaviors[(int)VisionBehavior.KEYBOARD].enabled = true;
        _coneBehaviors[(int)VisionBehavior.MOUSE].enabled = false;
    }

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rigidBody = GetComponent<Rigidbody2D>();
        
        _ascRef = GetComponent<AbilitySystemComponent>();
        _coneController = visionObject.GetComponent<VisionConeController>();

        _coneBehaviors = GetComponents<InputToVisionSystemBehavior>();
        
        visionObject.GetComponent<VisionSystemController>().OnOverlapChanged.Subscribe(this);
        
        //Defines statistics the ASC will work with.
        _ascRef.DefineStat("Blood", baseValue:100.0f, lowerRange:-15.0f);
        _ascRef.DefineStat("BloodMax", baseValue:100.0f, lowerRange:1.0f);
        
        //Grant ability. For consumable ones this will just increment the available charge count if the ability has already been granted.
        _ascRef.GrantAbility<AEffectVampireStatClamp>("_StatClamp");
        _ascRef.GrantAbility<AEffectVampireThirst>("Thirst");
        _ascRef.GrantAbility<AEffectVampireBite>("Bite");
        _ascRef.GrantAbility<AEffectVampireInvisibility>("EInvisibility");
        _ascRef.GrantAbility<AVampireBite>("TryBite");
        _ascRef.GrantAbility<AVampireTeleportation>("TP");
        _ascRef.GrantAbility<AVampireBlind>("Blind");
        _ascRef.GrantAbility<AVampireInvisibility>("Invisibility");
        _ascRef.GrantAbility<AVampireLure>("Lure");
        _ascRef.GrantAbility<AVampireSedate>("Sedate");
        _ascRef.GrantAbility<AVampireBloodPocket>("BloodPack");
        
        //bind ability to a keyboard input. The ability will then be executed when this key is pressed.
        _ascRef.BindAbility("TryBite", KeyCode.Q);
        _ascRef.BindAbility("Blind", KeyCode.E);


        playerCamera.orthographicSize = currentZoom;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public VisionConeController GetVision()
    {
        return _coneController;
    }

    private void ZoomCamera()
    {
        if(playerCamera == null)
        {
            return;
        }
        playerCamera.orthographicSize = currentZoom;
        //float dzoom = speedZoom * Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime * 10.0f;
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

        if (hAxis != 0 || vAxis != 0) 
        {
            directionPerso = DirectionHelper.FromVector(direction);
        }

        rigidBody.velocity = direction * speed * Time.deltaTime;

        if(rigidBody.velocity.magnitude > 0)
        {
            animator.SetInteger("state", (int)State.Walking);
            Vector2 velocityMag = rigidBody.velocity;
            velocityMag.y *= 2f;
            velocityMag = velocityMag.normalized;

            animator.SetFloat("xSpeed", velocityMag.x);
            animator.SetFloat("ySpeed", velocityMag.y);
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
        if(_ascRef.GetInputLock().IsOpened())
        {
            if (Input.GetMouseButtonDown((int)MouseButton.Right))
            {
                BindVisionToMouse();
            }

            if (Input.GetMouseButtonUp((int)MouseButton.Right))
            {
                UnbindVisionFromMouse();
            }
        }
       
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

    public void OnEvent(VisionSystemController.OverlapData context)
    {
        if (!context.Target.CompareTag("Targetable")) return;
        Targetable target = context.Target.GetComponent<Targetable>();
        if (!target.IsVisibleByPlayer) return;
        GuardManager guard = target.GetComponent<GuardManager>();
        if (context.BeginOverlap)
        {
            guard.EnterPlayerSigth();
        }
        else if(guard.alertStage != AlertStage.Alerted)
        {
            guard.ExitPlayerSight();
        }
    }

    public void LockVision(Vector3 target)
    {
        _coneBehaviors[(int)VisionBehavior.KEYBOARD].enabled = false;
        _coneBehaviors[(int)VisionBehavior.MOUSE].enabled = false;
        _coneBehaviors[(int)VisionBehavior.TARGET].enabled = true;
        ((TargetInputToVisionSystemBehavior)_coneBehaviors[(int)VisionBehavior.TARGET]).targetPosition = target;
    }

    public void UnlockVision()
    {
        _coneBehaviors[(int)VisionBehavior.KEYBOARD].enabled = true;
        _coneBehaviors[(int)VisionBehavior.MOUSE].enabled = false;
        _coneBehaviors[(int)VisionBehavior.TARGET].enabled = false;
    }


}

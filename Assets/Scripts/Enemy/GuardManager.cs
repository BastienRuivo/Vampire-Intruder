using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Pathfinding;
using Systems.Vision;
using Systems.Vision.Cone;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;
using Tools = DefaultNamespace.Tools;

public enum AlertStage
{
    Idle = 0,
    SeenSomething = 1,
    Suspicious = 2,
    Alerted = 3
}

public class GuardManager : MonoBehaviour
{
    public CameraShake cameraShake;
    public LayerMask visionMask;
    public GameObject currentRoom;
    public GameObject player;
    private PlayerController _playerController;
    public float guardVisionSpeed = 3f;

    public enum AnimationState
    {
        IDLE = 0,
        WALKING = 1
    }

    [Header("Alert")]
    public Animator feedbackAnimator;
    public AlertStage alertStage;
    private AlertStage _previousAlertStage;
    public float alertTimer;
    private float _alertRatio;
    private float _currentAlert;
    [Range(1, 5)] public float viewDistance = 3.5f;
    // Speed when appearing in player sight
    [Range(0.1f, 10f)] 
    public float inSight = 1.5f;
    // Speed when disappearing of player sight
    [Range(0.1f, 10f)] 
    public float outSight = 1.0f;

    [Header("Guard dialogs")] 
    public string idleToSeenSomethingQuote;
    public string seenSomethingToSuspiciousQuote;
    public string suspiciousToAlertedQuote;
    public string suspiciousToSeenSomethingQuote;
    public string seenSomethingToIdleQuote;

    [Header("Pathfinding")]
    // Speed when travaelling normally
    public float defaultSpeed = 200f;
    // Distance of the next point in path
    public float nextPointDistance = 0.25f;
    // Timer to compute a new path, it is expensive so it needs to be huge
    // if the target is not moving.
    public float timerRefresh = 2f;
    // Timer to compute a new path if the target is a moving target
    public float timerRefreshPlayer = 0.2f;
    // total Timer to stay Idle when the guard is reaching a target, will be divided by the number of dir to check inside node
    public float timerWaitingBetweenNodes = 2f;
    // total timer to stay idle when the guard is seeking for player
    public float timerWaitingTempNodes = 0.5f;
    // Distance of losing game for player
    public float caughtDistance = 0.2f;
    // Prefab of a node, used to create a temporary node when needed
    public GameObject nodePrefab;
    // percentage of point placement between player and mob when guard see player in vision
    public float distancePercentageSuspicious = 0.33f;

    [Header("SpotEffect")]
    // Speed when player
    public float spotSpeed = 2000f;
    // Speed of the camera from player to guard when guard spot a player
    public float camSpeed = 10f;
    // Sound to play when guard spot player
    public AudioClip _spotSound;

    [Header("Vision")] 
    public GameObject visionCone;
    private VisionConeController _visionConeController;

    // Target of pathfinding
    private GameObject _target;
    // Temporary target for pathfinding
    private GameObject _tempTarget = null;
    // Is following the path reversed
    private bool _isPathReversed = false;
    // Current guard path
    private Path _currentPath;
    // Id inside the path
    private int _currentPointInPath = 0;
    // Is at the end of path
    private bool _reachedEnd = false;
    // Should restart the path coroutine
    private bool _shouldUpdatePath = false;
    // Current travel speed
    private float _speed;
    // Path maker object
    private Seeker _seeker;
    // Rigidbody for physics computations
    private Rigidbody2D _body;
    // Current waiting timer at node
    private float _currentWaitingTimer = 0f;
    // Is doing a short or a long timer
    private bool _fastNodeWaiting = false;
    // current node
    private List<Direction> _directions;
    // Path update coroutine
    private IEnumerator _updatePathCoroutine;
    // Alpha update coroutine
    private IEnumerator _updateAlphaCoroutine;
    // Renderer
    private SpriteRenderer _guardRenderer;
    [FormerlySerializedAs("TraceResolution")] 
    public uint traceResolution = 128;
    
    
    private float _foVThetaMin;
    private float _foVThetaMax;

    private bool _playerInFOV;
    private bool _playerInRange;
    
    private Animator _animator;
    private Transform _cameraPos;
    private static readonly int AlertRatio = Shader.PropertyToID("_AlertRatio");

    private void Awake() {
        alertStage = AlertStage.Idle;
        _previousAlertStage = alertStage;
        _currentAlert = alertTimer;
        _visionConeController = visionCone.GetComponent<VisionConeController>();
        _playerController = player.GetComponent<PlayerController>();
    }
    private void Start()
    {
        _target = FindClosestNode();
        _seeker = GetComponent<Seeker>();
        _body = GetComponent<Rigidbody2D>();

        _speed = defaultSpeed;

        _updatePathCoroutine = UpdatePath();
        StartCoroutine(_updatePathCoroutine);

        _animator = GetComponent<Animator>();
        
        _guardRenderer = GetComponent<SpriteRenderer>();
        
        _visionConeController.GetMaterial().SetTexture("_PlayerShadowMap", _playerController.GetVision().GetDepthMap());
    }
    private void Update()
    {
        if (GameState.GetInstance().status != 0) return;

        if (_shouldUpdatePath)
        {
            _updatePathCoroutine = UpdatePath();
            StartCoroutine(_updatePathCoroutine);
        }

        if(_cameraPos != null)
        {
            var newPos = Vector3.Lerp(_cameraPos.transform.position, _body.position, Time.deltaTime * 10f);
            newPos.z = _cameraPos.position.z;
            _cameraPos.position = newPos;
        }

        _visionConeController.GetMaterial().SetVector("_PlayerPosition", _playerController.GetVision().transform.position);
        _visionConeController.GetMaterial().SetFloat("_PlayerViewDistance", _playerController.GetVision().viewDistance);
        _visionConeController.GetMaterial().SetFloat("_PlayerViewMinAngle", _playerController.GetVision().GetViewMinAngle());
        _visionConeController.GetMaterial().SetFloat("_PlayerFieldOfView", _playerController.GetVision().GetViewAngle());
        HandleVision();
    }

    /** Coroutine to compute new player path
     * Then set an alarm to the timer to avoid high computation
    */
    private IEnumerator UpdatePath()
    {
        _shouldUpdatePath = false;
        while(!GameController.GetInstance().IsLevelLoaded())
        {
            yield return new WaitForSeconds(0.5f);
        }
        if (!_seeker.IsDone())
        {
            yield return null;
        }

        if(_tempTarget != null)
        {
            _seeker.StartPath(_body.position, _tempTarget.transform.position, OnPathComplete);
        }
        else
        {
            _seeker.StartPath(_body.position, _target.transform.position, OnPathComplete);
        }
        yield return new WaitForSeconds(_target.tag == "Player" ? timerRefreshPlayer : timerRefresh);
        _shouldUpdatePath = true;
    }
    /**
     * Set and reset position on new path
     * @param p newly computed path by seeker
    */
    void OnPathComplete(Path p)
    {
        if (p.error)
        {
            Debug.Log("Error in path");
            return;
        }
        _currentPath = p;
        _currentPointInPath = 0;
    }

    /**
     * Handle alert stage and vision cone computation
    */
    private void HandleVision()
    {
        if (_visionConeController.HasRefreshability(player.transform.position))
        {
            _visionConeController.Enable();
            _visionConeController.GetMaterial().SetFloat(AlertRatio, _alertRatio);
            if (_visionConeController.HasVisibility(player.transform.position))
            {
                UpdateAlertStage(true);
                HandleDialogs();
                if (alertStage == AlertStage.Alerted && Vector2.Distance(transform.position, player.transform.position) < caughtDistance)
                {
                    GameController.GetGameMode().GetCaught();
                }
            }
            else
            {
                UpdateAlertStage(false);
                HandleDialogs();
            }
            
            //_AlertRatio
        }
        else
        {
            _visionConeController.Disable();
            UpdateAlertStage(false);
            HandleDialogs();
        }
    }
    
    private void FixedUpdate()
    {
        FollowPath();
    }

    public void EnterPlayerSigth()
    {
        return; //todo fix enemy visibility

        if(_updateAlphaCoroutine != null)
        {
            StopCoroutine(_updateAlphaCoroutine);
        }
        _updateAlphaCoroutine = AlphaIncrement();
        StartCoroutine(_updateAlphaCoroutine);
    }
    public void ExitPlayerSight()
    {
        return; //todo fix enemy visibility
        if (_updateAlphaCoroutine != null)
        {
            StopCoroutine(_updateAlphaCoroutine);
        }
        _updateAlphaCoroutine = AlphaDecrement();
        StartCoroutine(_updateAlphaCoroutine);
    }
    private void OnTriggerEnter2D(Collider2D other) {
        //if(other.CompareTag("Player") && NoWallBetweenPlayerAndEnemy(other.gameObject.transform.position))
        //{
        //    _playerInFOV = true;
        //}
    }
    private void OnTriggerStay2D(Collider2D other) {
        //if(other.CompareTag("Player"))
        //{
        //    if(NoWallBetweenPlayerAndEnemy(other.gameObject.transform.position)){
        //        _playerInFOV = true;
        //    }else{
        //        _playerInFOV = false;
        //    }
        //}
    }
    private void OnTriggerExit2D(Collider2D other) {
        //if(other.CompareTag("Player"))
        //{
        //    _playerInFOV = false;
        //}
    }

    /**
     * Update and compute the current level of alert
     * @param playerInFOV is the player currently in the guard's view
     */
    private void UpdateAlertStage(bool playerInFOV)
    {
        // If the guard is alerted, or going to check a position where he seen the player, the update should not be perform
        if (alertStage == AlertStage.Alerted) return;

        // Update the currentAlert timer
        if(!playerInFOV && _currentAlert < alertTimer && _tempTarget == null)
        {
            _currentAlert = Mathf.Min(_currentAlert + Time.deltaTime, alertTimer);
        }
        else if(playerInFOV && _currentAlert > 0)
        {
            _currentAlert = Mathf.Max(_currentAlert - Time.deltaTime, 0f);
        }

        _alertRatio = 1f - _currentAlert / alertTimer;
        AlertStage newAlertStage = ComputeAlertStage(_alertRatio);
        // If the alert level is increasing, perform...
        if(newAlertStage > alertStage)
        {
            switch (newAlertStage)
            {
                // If alerted, changer target to player and lock camera
                case AlertStage.Alerted:
                    cameraShake.Shake(0.2f);
                    _speed = spotSpeed;
                    //PlayerState.GetInstance().LockInput();
                    AudioManager.GetInstance().playClip(_spotSound, transform.position);
                    _cameraPos = player.transform.Find("Camera");
                    _speed = spotSpeed;
                    _currentWaitingTimer = 0f;
                    _fastNodeWaiting = false;
                    Destroy(_tempTarget);
                    _tempTarget = null;
                    ChangeTarget(player);
                    break;
                // If very sus, check the player position
                case AlertStage.Suspicious:
                    // create Node at 25% between guard and player
                    cameraShake.Shake(0.02f);
                    GameObject nodeGO = Instantiate(nodePrefab);
                    nodeGO.transform.position = Vector3.Lerp(_body.position, player.transform.position, distancePercentageSuspicious);
                    _currentWaitingTimer = 0;
                    _fastNodeWaiting = false;
                    SetTemporaryTarget(nodeGO);
                    Node node = nodeGO.GetComponent<Node>();
                    Direction dir = DirectionHelper.BetweenTwoObjects(gameObject, player);
                    node.directionsToLook.Add(dir);
                    node.directionsToLook.Add(DirectionHelper.Previous(dir));
                    node.directionsToLook.Add(dir);
                    node.directionsToLook.Add(DirectionHelper.Next(dir));
                    break;
            }
        }

        alertStage = newAlertStage;
        DebugStageAlert(_alertRatio);
    }

    private void HandleDialogs()
    {
        if (alertStage == _previousAlertStage) return;

        if (alertStage > _previousAlertStage)
        {
            switch (alertStage)
            {
                case AlertStage.Idle:
                    break;
                case AlertStage.SeenSomething:
                    GameController.GetGameMode().MessageToUser(
                        new GameController.UserMessageData(
                            GameController.UserMessageData.MessageToUserSenderType.Guard,
                            idleToSeenSomethingQuote,
                            priority:GameController.UserMessageData.MessageToUserScheduleType.ImportanceOnTiming)
                        );
                    break;
                case AlertStage.Suspicious:
                    GameController.GetGameMode().MessageToUser(
                        new GameController.UserMessageData(
                            GameController.UserMessageData.MessageToUserSenderType.Guard,
                            seenSomethingToSuspiciousQuote,
                            priority:GameController.UserMessageData.MessageToUserScheduleType.ImportanceOnTiming)
                    );
                    break;
                case AlertStage.Alerted:
                    GameController.GetGameMode().MessageToUser(
                        new GameController.UserMessageData(
                            GameController.UserMessageData.MessageToUserSenderType.Guard,
                            suspiciousToAlertedQuote,
                            priority:GameController.UserMessageData.MessageToUserScheduleType.ImportanceOnTiming)
                    );
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        else
        {
            switch (alertStage)
            {
                case AlertStage.Idle:
                    GameController.GetGameMode().MessageToUser(
                        new GameController.UserMessageData(
                            GameController.UserMessageData.MessageToUserSenderType.Guard,
                            seenSomethingToIdleQuote,
                            priority:GameController.UserMessageData.MessageToUserScheduleType.ImportanceOnTiming)
                    );
                    break;
                case AlertStage.SeenSomething:
                    GameController.GetGameMode().MessageToUser(
                        new GameController.UserMessageData(
                            GameController.UserMessageData.MessageToUserSenderType.Guard,
                            suspiciousToSeenSomethingQuote,
                            priority:GameController.UserMessageData.MessageToUserScheduleType.ImportanceOnTiming)
                    );
                    break;
                case AlertStage.Suspicious:
                    break;
                case AlertStage.Alerted:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        _previousAlertStage = alertStage;
    }
    
    private void DebugStageAlert(float alertRatio)
    {
        //Debug.Log($"{_playerInRange} {_playerInFOV}");
        _guardRenderer.color = Color.Lerp(Color.green, Color.red, alertRatio);
    }//todo replace with real animation.

    /**
     * Follow the current path
    */
    private void FollowPath()
    {
        if (_currentPath == null) return;

        // If we're waiting at a node
        if (_currentWaitingTimer > 0)
        {
            _currentWaitingTimer = Mathf.Max(0f, _currentWaitingTimer - Time.deltaTime);
            // Go to idle if moving really slow
            if (_body.velocity.magnitude < 0.25)
            {
                _animator.SetInteger("state", (int)AnimationState.IDLE);
            }
            // Compute current dir on node and change direction to seek for player
            float timer = _fastNodeWaiting ? timerWaitingTempNodes : timerWaitingBetweenNodes;
            int currentDir = (int)((_directions.Count) * (1f - _currentWaitingTimer / timer));
            if(currentDir < _directions.Count)
            {
                LookAt(DirectionHelper.AngleDeg(_directions[currentDir]));
                Vector2 d = DirectionHelper.FromDirection(_directions[currentDir]);
                _animator.SetFloat("xSpeed", d.x);
                _animator.SetFloat("ySpeed", d.y);
            }
            return;
        }

        _reachedEnd = _currentPointInPath >= _currentPath.vectorPath.Count;
        if (_reachedEnd)
        {
            // If the temp target is not null, and the guard is at it, go back to the reak target after waiting a little
            if(_tempTarget != null)
            {
                Node node = _tempTarget.GetComponent<Node>();
                _directions = node.directionsToLook;
                ChangeTarget(_target);
                _currentWaitingTimer = timerWaitingTempNodes;
                _fastNodeWaiting = true;
                Destroy(_tempTarget);
                _tempTarget = null;
            }
            else
            {
                // If we're at the current target node, then wait a little and go to next target in path
                Node node = _target.GetComponent<Node>();
                if (node != null)
                {
                    _directions = node.directionsToLook;
                    if (node.isPathEnd) _isPathReversed = false;
                    ChangeTarget(node.NextTarget(_isPathReversed));
                    _currentWaitingTimer = timerWaitingBetweenNodes;
                    _fastNodeWaiting = false;
                }
            }
            return;
        }
        // Get current path in 
        var visionTarget = _tempTarget == null? _target : _tempTarget;
        bool canSeeTarget = NoWallToTarget(visionTarget);
        Vector3 pathPos = _currentPath.vectorPath[_currentPointInPath];
        Vector2 dir = (new Vector2(pathPos.x, pathPos.y) - _body.position).normalized;
        float angle = Tools.ComputeAngle(transform.position, canSeeTarget? visionTarget.transform.position : pathPos) * Mathf.Rad2Deg;

        // Change cone orientation
        LookAt(angle + 90f);

        Vector2 force = dir * _speed * Time.deltaTime;
        force.y *= 0.5f;
        if (_body.velocity.magnitude > 0)
        {
            _animator.SetFloat("xSpeed", _body.velocity.x);
            _animator.SetFloat("ySpeed", _body.velocity.y);
            _animator.SetInteger("state", (int)AnimationState.WALKING);
        }

        float distance = Vector2.Distance(_body.position, pathPos);
        if (distance < nextPointDistance) _currentPointInPath++;

        _body.AddForce(force);
    }
    
    /**
     * Change anchor orientation a little bit
     * @param angle target of the vision anchor
    */
    private void LookAt(float angle)
    {
        visionCone.transform.rotation= Quaternion.Lerp(visionCone.transform.rotation, Quaternion.AngleAxis(angle, Vector3.forward), Time.deltaTime * guardVisionSpeed);
    }

    /**
     * Change the path target and reset path
     * @param newTarget the new target where we want the guard to go
    */
    private void ChangeTarget(GameObject newTarget)
    {
        _target = newTarget;
        ForceResetPathfinding();
    }

    /**
     * Set a temporary objective for the guard, and reset path
     * @param tmp temporary objective that will be destroy at the end, then the guard will go back the his real target
    */
    private void SetTemporaryTarget(GameObject tmp)
    {
        _tempTarget = tmp;
        ForceResetPathfinding();
    }

    /**
     * Force the recomputation of the pathfinding with whatever target is set on the guard
    */
    private void ForceResetPathfinding()
    {
        StopCoroutine(_updatePathCoroutine); 
        _updatePathCoroutine = UpdatePath();
        StartCoroutine(_updatePathCoroutine);
    }

    /**
     * Compute the current alert stage depending on the timer ratio
     * @param alertRatio ratio between 0 and 1, 0 being kalm and 1 being JESSIKA I'M COMMIN FOR YOU
     * @return the alert stage corresponding to the ratio
    */
    private AlertStage ComputeAlertStage(float alertRatio)
    {
        AlertStage newAlertStage;
        if (alertRatio >= 1f)
        {
            feedbackAnimator.SetTrigger("DetectAlert");
            newAlertStage = AlertStage.Alerted;
        }
        else if (alertRatio > 0.5f)
        {
            feedbackAnimator.SetTrigger("DetectStay");
            newAlertStage = AlertStage.Suspicious;
        }
        else if (alertRatio > 0f)
        {
            feedbackAnimator.SetBool("Dectect", true);
            newAlertStage = AlertStage.SeenSomething;
        }
        else
        {
            feedbackAnimator.SetBool("Dectect", false);
            newAlertStage = AlertStage.Idle;
        }
        return newAlertStage;
    }

    /**
     * Raycast to see if the target is currently visible for the guard
     * @param target the target we want to check if there's a wall between
     * @return a boolean indicating if there is no obstacle and therefore if the target is visible
    */
    private bool NoWallToTarget(GameObject target)
    {
        Vector3 direction = target.transform.position - transform.position;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, Vector3.Distance(target.transform.position, transform.position), visionMask);
        // Debug.DrawRay(transform.position, direction, Color.red);

        return hit.collider == null;
    }

    /**
     * Find the closest node from the guard on the current room
     * @return the closest node game object
    */
    private GameObject FindClosestNode()
    {
        Node[] nodes = currentRoom.transform.Find("CustomPivot/Nodes").GetComponentsInChildren<Node>();

        int closest = -1;
        float closestDist = 10e8f;

        for (int i = 0; i < nodes.Length; i++)
        {
            if (nodes[i] != null)
            {
                float dist = Vector3.Distance(transform.position, nodes[i].transform.position);
                if (dist >= closestDist) continue;
                closest = i;
                closestDist = dist;
            }
        }

        return closest != -1 ? nodes[closest].gameObject : null;
    }

    public void SetAlpha(float alpha)
    {
        var color = _guardRenderer.color;
        color.a = alpha;
        _guardRenderer.color = color;
    }

    public IEnumerator AlphaIncrement()
    {
        for(float alpha = _guardRenderer.color.a; alpha < 1.0f; alpha += Time.deltaTime * inSight)
        {
            SetAlpha(alpha);
            yield return null;
        }
    }

    public IEnumerator AlphaDecrement()
    {
        for(float alpha = _guardRenderer.color.a; alpha > 0.0f; alpha += Time.deltaTime * outSight)
        {
            SetAlpha(alpha);
            yield return null;
        }
    }
}
// CONE VISION
//          Collider[] targetsInFOV = Physics.OverlapSphere(
//             transform.position, fov);
//         foreach (Collider c in targetsInFOV)
//         {
//             if (c.CompareTag("Player"))
//             {
//                 float signedAngle = Vector3.Angle(
//                     transform.forward,
//                     c.transform.position - transform.position);
//                 if (Mathf.Abs(signedAngle) < fovAngle / 2)
//                     playerInFOV = true;
//                 break;
//             }
//         }
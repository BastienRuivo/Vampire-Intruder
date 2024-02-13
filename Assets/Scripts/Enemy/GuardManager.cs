using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Pathfinding;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

public enum AlertStage
{
    Idle,
    SeenSomething,
    Suspicious,
    Alerted
}

public class GuardManager : MonoBehaviour
{
    public CameraShake cameraShake;
    public LayerMask visionMask;
    public GameObject currentRoom;
    public GameObject player;
    public float guardVisionSpeed = 3f;

    public enum AnimationState
    {
        IDLE = 0,
        WALKING = 1
    }

    [Header("Alert")]
    public AlertStage alertStage;
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

    // Game object representing the guard vision
    private GameObject _visionAnchor;
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


    //Cone trace
    private GameObject _visionCone;
    private SpriteRenderer _visionRenderer;
    private Texture2D _linearShadowMap;
    private Color[] _shadowMapData;
    private Vector2 _firstDir;
    private float _coneAngle;
    private float _coneAngleMin;
    private Animator _animator;
    private Transform _cameraPos;
    
    private void Awake() {
        alertStage = AlertStage.Idle;
        _currentAlert = alertTimer;
    }
    private void Start()
    {
        _linearShadowMap = new Texture2D((int)traceResolution, 1, TextureFormat.RFloat, false);
        _shadowMapData = new Color[traceResolution] ;
        for (var i = 0; i < _shadowMapData.Length; i++)
        {
            _shadowMapData[i] = new Color(0,0,0);
        }
        _linearShadowMap.SetPixels(_shadowMapData);
        _linearShadowMap.Apply();
        
        //find children
        foreach (Transform child in transform.GetChild(0).transform)
        {
            if (!child.gameObject.CompareTag("EnemyVisionDecal")) continue;
            _visionCone = child.gameObject;
            child.gameObject.GetComponent<SpriteRenderer>().material.SetTexture("_ShadowMap", _linearShadowMap);
        }

        _target = FindClosestNode();
        _seeker = GetComponent<Seeker>();
        _body = GetComponent<Rigidbody2D>();

        _speed = defaultSpeed;

        _updatePathCoroutine = UpdatePath();
        StartCoroutine(_updatePathCoroutine);

        _visionAnchor = transform.GetChild(0).gameObject;
        _animator = GetComponent<Animator>();


        _visionRenderer = _visionCone.GetComponent<SpriteRenderer>();
        _guardRenderer = GetComponent<SpriteRenderer>();
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

        HandleVision();
    }

    /** Coroutine to compute new player path
     * Then set an alarm to the timer to avoid high computation
    */
    private IEnumerator UpdatePath()
    {
        _shouldUpdatePath = false;
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
        //handle distance
        UpdateRange(player.transform.position);
        if (!_playerInRange) { UpdateAlertStage(false); return; }

        //handle range
        UpdateFieldOfView(player.transform.position);
        UpdateShadowMap();
        _visionRenderer.material.SetVector("_ObserverPosition", transform.position);
        _visionRenderer.material.SetFloat("_ObserverMinAngle", _coneAngleMin);
        _visionRenderer.material.SetFloat("_ObserverViewDistance", viewDistance);
        _visionRenderer.material.SetFloat("_ObserverFieldOfView", _coneAngle);
        _visionRenderer.material.SetFloat("_AlertRatio", _alertRatio);


        
        if (!_playerInFOV) { UpdateAlertStage(false); return; }

        //handle direct line trace
        if (!NoWallToTarget(player))
        {
            UpdateAlertStage(false);
            return;
        }

        UpdateAlertStage(true);

        if (_playerInFOV && alertStage == AlertStage.Alerted && Vector2.Distance(transform.position, player.transform.position) < caughtDistance)
        {
            GameController.GetGameMode().GetCaught();
        }
    }
    
    private void FixedUpdate()
    {
        FollowPath();
    }

    public void EnterPlayerSigth()
    {
        if(_updateAlphaCoroutine != null)
        {
            StopCoroutine(_updateAlphaCoroutine);
        }
        _updateAlphaCoroutine = AlphaIncrement();
        StartCoroutine(_updateAlphaCoroutine);
    }
    public void ExitPlayerSight()
    {
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
                    PlayerState.GetInstance().LockInput();
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
                    cameraShake.Shake(0.05f);
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
    private void DebugStageAlert(float alertRatio)
    {
        //Debug.Log($"{_playerInRange} {_playerInFOV}");
        _guardRenderer.color = Color.Lerp(Color.green, Color.red, alertRatio);
    }//todo replace with real animation.

    /** 
     * Update guard's FOV cone
     */
    private void UpdateFieldOfView(Vector3 playerPosition)
    {
        //TODO Comment
        PolygonCollider2D childCollider = GetComponentInChildren<PolygonCollider2D>();
        if (childCollider is PolygonCollider2D)
        {
            Vector2[] path = childCollider.GetPath(0);

            Vector2 origin = childCollider.transform.TransformPoint(path[1]);
            Vector2 boundA = childCollider.transform.TransformPoint(path[0]);
            Vector2 boundB = childCollider.transform.TransformPoint(path[2]);

            float angleA = ComputeAngle(origin, boundA);
            float angleB = ComputeAngle(origin, boundB);

            _foVThetaMin = angleA > angleB ? angleB : angleA;
            _firstDir = angleA > angleB ? boundA - origin : boundB - origin;
            _foVThetaMax = angleA > angleB ? angleA : angleB;

            float theta = _foVThetaMax - _foVThetaMin;
            if (theta > Mathf.PI)
            {
                (_foVThetaMax, _foVThetaMin) = (_foVThetaMin, _foVThetaMax);
                _firstDir = angleA > angleB ? boundB - origin : boundA - origin;
                theta = 2 * Mathf.PI - theta;
            }

            _coneAngleMin = _foVThetaMin;
            _coneAngle = theta;
            _firstDir.Normalize();

            float playerAngle = ComputeAngle(origin, playerPosition);
            float dThetaMax = _foVThetaMax - playerAngle;
            float dThetaMix = _foVThetaMin - playerAngle;
            _playerInFOV = ((dThetaMax >= 0 && dThetaMax < theta) || (dThetaMix <= 0 && dThetaMix > -1 * theta));
        } //todo check
        else
        {
            _playerInFOV = false;
            Debug.LogError("No vision path collider attached to enemy.");
        }
    }

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
        float angle = ComputeAngle(transform.position, canSeeTarget? visionTarget.transform.position : pathPos) * Mathf.Rad2Deg;

        // Change cone orientation
        LookAt(angle + 90f);

        Vector2 force = dir * _speed * Time.deltaTime;
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
     * Compute the shadow vision
    */
    private void UpdateShadowMap()
    {
        float step = _coneAngle / (float)(traceResolution - 1.0f);
        float angle = 0.0f;

        //Ray casting
        for (uint i = 0; i < traceResolution; i++)
        {
            Vector2 dir2D = RotateVector(_firstDir, -angle);
            Vector3 direction = new Vector3(dir2D.x, dir2D.y, 0.0f);
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, viewDistance, visionMask);
            //Debug.DrawRay(transform.position, direction, hit.collider == null? Color.red : Color.green, 0.1f);

            //Store depth
            if (hit.collider == null)
            {
                _shadowMapData[i].r = 1.0f;
                _shadowMapData[i].b = 1.0f;
                _shadowMapData[i].g = 1.0f;
            }
            else
            {
                _shadowMapData[i].r = hit.distance / viewDistance;
                _shadowMapData[i].b = hit.distance / viewDistance;
                _shadowMapData[i].g = hit.distance / viewDistance;
            }

            angle += step;
        }

        _linearShadowMap.SetPixels(_shadowMapData);
        _linearShadowMap.Apply();
    }

    /**
     * Change anchor orientation a little bit
     * @param angle target of the vision anchor
    */
    private void LookAt(float angle)
    {
        _visionAnchor.transform.rotation = Quaternion.Lerp(_visionAnchor.transform.rotation, Quaternion.AngleAxis(angle, Vector3.forward), Time.deltaTime * guardVisionSpeed);
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
            newAlertStage = AlertStage.Alerted;
        }
        else if (alertRatio > 0.5f)
        {
            newAlertStage = AlertStage.Suspicious;
        }
        else if (alertRatio > 0f)
        {
            newAlertStage = AlertStage.SeenSomething;
        }
        else
        {
            newAlertStage = AlertStage.Idle;
        }
        return newAlertStage;
    }

    /**
     * Check if the player is in range of the guard
     * @param playerPosition position of the player on the map
    */
    private void UpdateRange(Vector3 playerPosition)
    {
        Vector3 origin = transform.position;
        _playerInRange = (Vector3.Distance(origin, playerPosition) < viewDistance);
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
     * Compute the angle between two Vectors
     * @param observer The object that you want to oriente
     * @param objective where you want your object to be oriented to
     * @return the angle in radian to oriente the observer
    */
    private float ComputeAngle(Vector3 observer, Vector3 objective)
    {
        Vector3 direction = objective - observer;
        float Pi2 = Mathf.PI * 2;
        return (Mathf.Atan2(direction.y, direction.x) + Pi2) % Pi2;
    }

    /**
     * Find the closest node from the guard on the current room
     * @return the closest node game object
    */
    private GameObject FindClosestNode()
    {
        Node[] nodes = currentRoom.transform.Find("Nodes").GetComponentsInChildren<Node>();

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

    /**
     * Rotate of angle a 2d vector
     * @param vector the vector to rotate
     * @param angleRadians the angle of the rotation
     * @return the rotated vector
    */
    private Vector2 RotateVector(Vector2 vector, float angleRadians)
    {
        float cosTheta = (float)Math.Cos(angleRadians);
        float sinTheta = (float)Math.Sin(angleRadians);

        float newX = vector.x * cosTheta - vector.y * sinTheta;
        float newY = vector.x * sinTheta + vector.y * cosTheta;

        return new Vector2(newX, newY);
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
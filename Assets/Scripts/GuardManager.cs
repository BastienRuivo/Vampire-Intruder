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
    Suspicious,
    Alerted
}

public class GuardManager : MonoBehaviour
{
    public LayerMask visionMask;
    public GameObject currentRoom;
    public GameObject player;

    public AlertStage alertStage;
    [Range(0,100)]  public float alertLevel; // 0-100
    [Range(1,5)]  public float viewDistance = 3.5f; // 0-100

    [Header("Pathfinding")]
    public float defaultSpeed = 200f;
    public float spotSpeed = 2000f;
    public float nextPointDistance = 0.25f;
    public float timerRefresh = 2f;
    public float timerRefreshPlayer = 0.2f;
    public float timerWaitingBetweenNodes = 2f;
    public float currentWaitingTimer = 0f;
    public float caughtDistance = 0.2f;

    private GameObject visionAnchor;


    private GameObject target;
    private bool isPathReversed = false;
    private Path currentPath;
    private int currentPointInPath = 0;
    private bool reachedEnd = false;
    private bool shouldUpdatePath = false;
    private bool hasBeenAlerted = false;
    private float speed;


    Seeker seeker;
    Rigidbody2D body;

    IEnumerator updatePathCoroutine;

    private IEnumerator UpdatePath()
    {
        shouldUpdatePath = false;
        if (!seeker.IsDone())
        {
            yield return null;
        }
        seeker.StartPath(body.position, target.transform.position, OnPathComplete);
        yield return new WaitForSeconds(target.tag == "Player" ? timerRefreshPlayer: timerRefresh);
        shouldUpdatePath = true;
    }

    void OnPathComplete(Path p)
    {
        if (p.error)
        {
            Debug.Log("Error in path");
            return;
        }
        currentPath = p;
        currentPointInPath = 0;
    }


    [FormerlySerializedAs("TraceResolution")] public uint traceResolution = 128;
    
    private float _foVThetaMin;
    private float _foVThetaMax;

    private bool _playerInFOV;
    private bool _playerInRange;

    //Cone trace
    private GameObject _visionCone;
    private Texture2D _linearShadowMap;
    private Color[] _shadowMapData;
    private Vector2 _firstDir;
    private float _coneAngle;
    private float _coneAngleMin;
    
    private void Awake() {
        alertStage = AlertStage.Idle;
        alertLevel = 0;
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

        target = FindClosestNode();
        Debug.Log("Target" + target.ToString());
        seeker = GetComponent<Seeker>();
        body = GetComponent<Rigidbody2D>();

        speed = defaultSpeed;

        updatePathCoroutine = UpdatePath();
        StartCoroutine(updatePathCoroutine);

        visionAnchor = transform.GetChild(0).gameObject;
    }

    private void Update()
    {
        if (GameState.GetInstance().status != 0) return;
        DebugStageAlert();

        if (shouldUpdatePath)
        {
            updatePathCoroutine = UpdatePath();
            StartCoroutine(updatePathCoroutine);
        }

        HandleVision();
    }

    private void HandleVision()
    {
        //handle distance
        UpdateRange(player.transform.position);
        if (!_playerInRange) { updateAlertStage(false); return; }

        //handle range
        UpdateFieldOfView(player.transform.position);
        UpdateShadowMap();
        SpriteRenderer coneSprite = _visionCone.GetComponent<SpriteRenderer>();
        coneSprite.material.SetVector("_ObserverPosition", transform.position);
        coneSprite.material.SetFloat("_ObserverMinAngle", _coneAngleMin);
        coneSprite.material.SetFloat("_ObserverViewDistance", viewDistance);
        coneSprite.material.SetFloat("_ObserverFieldOfView", _coneAngle);




        if (!_playerInFOV) { updateAlertStage(false); return; }

        //handle direct line trace
        if (!noWallBetweenPlayerAndEnemy(player.transform.position))
        {
            updateAlertStage(false);
            return;
        }

        updateAlertStage(true);

        if (_playerInFOV && alertStage == AlertStage.Alerted && Vector2.Distance(transform.position, player.transform.position) < caughtDistance)
        {
            GameController.GetGameMode().GetCaught();
        }
    }

    private void FixedUpdate()
    {
        FollowPath();
    }

    private void OnTriggerEnter2D(Collider2D other) {
        //if(other.CompareTag("Player") && noWallBetweenPlayerAndEnemy(other.gameObject.transform.position))
        //{
        //    _playerInFOV = true;
        //}
    }

    private void OnTriggerStay2D(Collider2D other) {
        //if(other.CompareTag("Player"))
        //{
        //    if(noWallBetweenPlayerAndEnemy(other.gameObject.transform.position)){
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

    private void updateAlertStage(bool playerInFOV)
    {
        switch (alertStage)
        {
            case AlertStage.Idle:
                if (playerInFOV)
                    alertStage = AlertStage.Suspicious;
                break;
            case AlertStage.Suspicious:
                if (playerInFOV)
                {
                    alertLevel++;
                    if (alertLevel >= 100)
                        alertStage = AlertStage.Alerted;
                }
                else
                {
                    alertLevel--;
                    if (alertLevel <= 0)
                        alertStage = AlertStage.Idle;
                }
                break;
            case AlertStage.Alerted:
                if (!playerInFOV)
                    alertStage = AlertStage.Suspicious;
                break;
        }
    }

    private void DebugStageAlert()
    {
        //Debug.Log($"{_playerInRange} {_playerInFOV}");
        Color r = new Color(1,0,0,1);
        Color g = new Color(0,1,0,1);
        Color c = g;
        if(alertStage == AlertStage.Suspicious)
            c = Color.Lerp(g, r, alertLevel/100);
        if(alertStage == AlertStage.Alerted){
            c = r;
            if(_playerInFOV && !hasBeenAlerted)
            {
                speed = spotSpeed;
                StopCoroutine(updatePathCoroutine);

                target = player;
                updatePathCoroutine = UpdatePath();
                StartCoroutine(updatePathCoroutine);
                speed = spotSpeed;
                hasBeenAlerted = true;

                currentWaitingTimer = 0f;
            }
        }
        gameObject.GetComponent<SpriteRenderer>().color = c;
    }//todo replace with real animation.

    private void UpdateRange(Vector3 playerPosition)
    {
        Vector3 origin = transform.position;
        _playerInRange = (Vector3.Distance(origin, playerPosition) < viewDistance);
    }

    private void UpdateFieldOfView(Vector3 playerPosition)
    {
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

    private bool noWallBetweenPlayerAndEnemy(Vector3 playerPosition)
    {
        Vector3 direction = playerPosition - transform.position;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, Vector3.Distance(playerPosition,transform.position), visionMask);
        // Debug.DrawRay(transform.position, direction, Color.red);

        return hit.collider == null;
    }

    private float ComputeAngle(Vector3 observer, Vector3 target)
    {
        Vector3 direction = target - observer;
        float Pi2 = Mathf.PI * 2;
        return (Mathf.Atan2(direction.y, direction.x) + Pi2) % Pi2;
    }
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

    private void FollowPath()
    {
        if (currentPath == null) return;

        if(currentWaitingTimer > 0)
        {
            currentWaitingTimer -= Time.deltaTime;
            return;
        }

        reachedEnd = currentPointInPath >= currentPath.vectorPath.Count;
        if(reachedEnd)
        {
            Node node = target.GetComponent<Node>();
            if (node != null)
            {
                if (node.isPathEnd) isPathReversed = false;
                target = node.NextTarget(isPathReversed);
                StopCoroutine(updatePathCoroutine);
                updatePathCoroutine = UpdatePath();
                StartCoroutine(updatePathCoroutine);
                currentWaitingTimer = timerWaitingBetweenNodes;
            }
            return;
        }
        Vector3 pathPos = currentPath.vectorPath[currentPointInPath];
        Vector2 dir = (new Vector2(pathPos.x, pathPos.y) - body.position).normalized;
        float angle;
        float angularSpeed = 1f;
        if(target.tag == player.tag)
        {
            angle = ComputeAngle(transform.position, target.transform.position) * Mathf.Rad2Deg;
            angularSpeed = 25f;
        }
        else
        {
            angle = ComputeAngle(transform.position, pathPos) * Mathf.Rad2Deg;
        }

        visionAnchor.transform.rotation = Quaternion.Lerp(visionAnchor.transform.rotation, Quaternion.AngleAxis(angle + 90f, Vector3.forward), Time.deltaTime * angularSpeed);




        Vector2 force = dir * speed * Time.deltaTime;

        float distance = Vector2.Distance(body.position, pathPos);
        if (distance < nextPointDistance) currentPointInPath++;

        body.AddForce(force);
    }
    

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
    
    private Vector2 RotateVector(Vector2 vector, float angleRadians)
    {
        float cosTheta = (float)Math.Cos(angleRadians);
        float sinTheta = (float)Math.Sin(angleRadians);

        float newX = vector.x * cosTheta - vector.y * sinTheta;
        float newY = vector.x * sinTheta + vector.y * cosTheta;

        return new Vector2(newX, newY);
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
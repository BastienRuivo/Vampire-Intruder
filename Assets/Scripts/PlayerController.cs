using System.Collections;
using System.Collections.Generic;
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

    private enum State
    {
        Idle,
        Walking
    }

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        rigidBody = GetComponent<Rigidbody2D>();
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

    private void Move()
    {
        float hAxis = Input.GetAxis("Horizontal");
        float vAxis = Input.GetAxis("Vertical");

        Vector3 direction = new Vector3(hAxis, vAxis, 0).normalized;

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
        Move();
    }
}

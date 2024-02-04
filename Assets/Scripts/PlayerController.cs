using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerController : MonoBehaviour
{
    public float Speed = 20f;
    public float ZoomSpeed = 10f;

    private Animator anim;

    public Camera cam;
    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if(cam == null)
        {
            return;
        }

        float hAxis = Input.GetAxis("Horizontal");
        float vAxis = Input.GetAxis("Vertical");

        anim.SetFloat("xSpeed", hAxis);
        anim.SetFloat("ySpeed", vAxis);

        Vector3 movement = new Vector3(hAxis, vAxis, 0);
        
        // translate the player by the movement vector normalized
        transform.Translate(movement.normalized * Speed * Time.deltaTime);

        float dzoom = ZoomSpeed * Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime * 10.0f;
        cam.orthographicSize = Mathf.Max(0.1f, cam.orthographicSize - dzoom);
    }
}

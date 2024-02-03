using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class FreeCameraController : MonoBehaviour
{
    public float panSpeed = 20f;
    public float zoomSpeed = 10f;

    public Camera cam;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float dx = panSpeed * Input.GetAxis("Horizontal") * Time.deltaTime;
        float dy = panSpeed * Input.GetAxis("Vertical") * Time.deltaTime;

        if(cam != null)
        {
            cam.transform.Translate(dx, dy, 0);
        }
    }
}

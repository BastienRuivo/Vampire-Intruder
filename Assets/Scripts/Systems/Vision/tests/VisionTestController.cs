using System.Collections;
using System.Collections.Generic;
using Systems.Vision;
using UnityEngine;

public class VisionTestController : MonoBehaviour
{
    public GameObject Player;

    private VisionConeController _cone;
    // Start is called before the first frame update
    void Start()
    {
        _cone = GetComponent<VisionConeController>();
    }

    // Update is called once per frame
    void Update()
    {
        //if(_cone.HasRefreshability(Player.transform.position))
        //    _cone.Enable();
        //else
        //    _cone.Disable();
        
    }
}

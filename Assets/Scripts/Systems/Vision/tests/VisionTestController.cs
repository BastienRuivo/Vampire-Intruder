using System.Collections;
using System.Collections.Generic;
using Systems.Vision;
using Systems.Vision.Cone;
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
        if (_cone.HasRefreshability(Player.transform.position))
        {
            _cone.Enable();
            if (_cone.HasVisibility(Player.transform.position))
            {
                _cone.GetMaterial().SetFloat(AlertRatio, 1.0f);
            }
            else
            {
                _cone.GetMaterial().SetFloat(AlertRatio, 0.0f);
            }
            
            //_AlertRatio
        }
        else
            _cone.Disable();
        
    }
    
    private static readonly int AlertRatio = Shader.PropertyToID("_AlertRatio");
}

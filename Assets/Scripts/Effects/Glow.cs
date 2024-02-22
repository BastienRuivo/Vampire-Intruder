using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Glow : MonoBehaviour
{
    public float upSpeed = 1f;
    public float downSpeed = 1f;
    [Range(0f, 1f)]
    public float maxAlpha = 1f;
    public Color effectColor = Color.white;
    public bool activeAtStart = false;

    private float _currentTime;
    private bool down = false;

    public Renderer render;
    public Material mat;
    private Material _originMat;


    private void Start()
    {
        if(activeAtStart)
            Activate();
    }

    void Update()
    {
        if(down)
        {
            _currentTime = Mathf.Max(0f, _currentTime - Time.deltaTime * downSpeed);
            down = _currentTime != 0f;
        }
        else
        {
            _currentTime = Mathf.Min(1f, _currentTime + Time.deltaTime * upSpeed);
            down = _currentTime == 1f;
        }

        render.material.SetColor("_Color", render.material.color);
        render.material.SetColor("_GlowColor", effectColor);
        render.material.SetFloat("_t", _currentTime);

    }

    public void Activate()
    {
        enabled = true;
        _originMat = render.material;
        render.material = mat;
        render.material.SetColor("_Color", render.material.color);
        render.material.SetColor("_GlowColor", effectColor);
        render.material.SetFloat("_t", 0);
        _currentTime = 0f;
        down = false;
    }

    public void Deactivate()
    {
        enabled = false;
        render.material = _originMat;
    }
}

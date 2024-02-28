using DefaultNamespace;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ArrowPointer : MonoBehaviour
{
    public Vector3 target;
    private SpriteRenderer _spriteRenderer;
    public SpriteRenderer _seal;
    public SpriteRenderer _exit;
    void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if(target == null)
        {

            transform.rotation = Quaternion.AngleAxis(0f, Vector3.forward);
            return;
        }
        float angle = (Tools.ComputeAngle(transform.parent.position, target) * Mathf.Rad2Deg) - 90f;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        _exit.transform.rotation =  Quaternion.Euler(0.0f, 0.0f, transform.rotation.z * -1.0f);
        _seal.transform.rotation = _exit.transform.rotation;
    }

    public void SetTarget(Vector3 t)
    {
        target= t;
    }

    public void SetColor(Color col)
    {
        _spriteRenderer.color = col;
        _spriteRenderer.enabled= true;
    }

    public void ShowExit()
    {
        _exit.enabled= true;
        _seal.enabled= false;
    }

    public void ShowSeal()
    {
        _seal.enabled= true;
        _exit.enabled= false;
    }

    public void Disable()
    {
        gameObject.SetActive(false);
    }


}

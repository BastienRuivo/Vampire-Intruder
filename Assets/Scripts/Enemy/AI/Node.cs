using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    [Header("Node")]
    public GameObject next;
    public GameObject previous;
    public bool isPathEnd;
    public float tolerance = 1f;

    public GameObject NextTarget(bool isReversed)
    {
        return isReversed? previous : next;
    }

    public bool isInRange(Vector2 pos)
    {
        return Vector2.Distance(transform.position, pos) < tolerance;
    }
}

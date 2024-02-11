using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    [Header("Node")]
    public GameObject next;
    public GameObject previous;
    public bool isPathEnd;

    public List<Direction> directionsToLook;

    public GameObject NextTarget(bool isReversed)
    {
        return isReversed? previous : next;
    }
}

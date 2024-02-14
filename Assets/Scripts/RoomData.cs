using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomData : MonoBehaviour
{
    enum RoomType
    {
        HALL,
        BEDROOM,
        BEDROOM_CORRIDORS,
        TREASURE,
        // ~~KOUIZINE~~
        KITCHEN,
        /*THE_*/
        OFFICE
    }

    [Header("Grid graph data")]
    public int width = 1;
    public int height = 1;
    public Vector3 localGraphPosition = Vector3.zero;
    public LayerMask layers;

    private float _nodeSize = 0.176775f;
    private float _obstacleSize = 1.5f;

    private void Awake()
    {
        GridGraph gg = AstarPath.active.data.AddGraph(typeof(GridGraph)) as GridGraph;
        gg.SetGridShape(InspectorGridMode.IsometricGrid);
        gg.isometricAngle = 60f;
        gg.rotation = new Vector3(-45f, 270f, 90f);
        gg.center = transform.position + localGraphPosition;
        gg.SetDimensions(width, height, _nodeSize);
        gg.collision.use2D = true;
        gg.collision.diameter = _obstacleSize;
        gg.collision.mask = layers;
    }
}

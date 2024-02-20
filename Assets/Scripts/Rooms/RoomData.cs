using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoomData : MonoBehaviour
{
    public enum Type
    {
        HALL,
        TREASURE,
        CORRIDOR,
        DECORATION,
        NOONE
    }

    public static float[] TypeCost =
    {
        // HALL ALWAYS COST 0
        0f, 
        // TREASURE ROOM COST
        0.1f,
        // CORRIDOR ROOM COST
        0.2f,
        // DECORATION COST
        0.1f
    };
    /// <summary>
    /// Type of room for procedural connection
    /// </summary>
    public Type type;
    /// <summary>
    /// What are the connectable directions
    /// </summary>
    public Direction[] connectableDir;

    /// <summary>
    /// Prevent connector to generate a certain type of room even if it's allowed for them
    /// Like when you have a hall that can generate a treasure room for each node, but you want at most 1
    /// </summary>
    public int[] maxTypeBranch;

    [Header("Grid graph data")]
    public int width = 1;
    /// <summary>
    /// Height of the grid
    /// </summary>
    public int height = 1;
    /// <summary>
    /// Local position of the graph in the prefab room, active the graph object and place it, then copy paste values here
    /// </summary>
    public Vector3 localGraphPosition = Vector3.zero;
    /// <summary>
    /// Layer mask use for graph collisions
    /// </summary>
    public LayerMask layers;
    /// <summary>
    /// Size of a node in world
    /// </summary>
    private static readonly float _nodeSize = 0.176775f;
    /// <summary>
    /// Size of an obstacle in the graph
    /// </summary>
    private static readonly float _obstacleSize = 1.5f;
    private bool _isColliding;
    private RoomData _collider;
    private RoomConnector[] _connectors;
    public float energy;

    private void Awake()
    {
        _connectors = GetComponentsInChildren<RoomConnector>();
    }


    /// <summary>
    /// Generate the Grid Graph of the room for astar pathfinding
    /// All mob are currently restrained to their rooms
    /// </summary>
    /// <param name="pos"> Room position in world</param>
    /// <returns>The new grid graph if needed</returns>
    public GridGraph BuildGraph(Vector3 pos)
    {
        GridGraph gg = AstarPath.active.data.AddGraph(typeof(GridGraph)) as GridGraph;
        gg.SetGridShape(InspectorGridMode.IsometricGrid);
        gg.isometricAngle = 60f;
        gg.rotation = new Vector3(-45f, 270f, 90f);
        gg.center = localGraphPosition + pos;
        gg.SetDimensions(width, height, _nodeSize);
        gg.collision.use2D = true;
        gg.collision.diameter = _obstacleSize;
        gg.collision.mask = layers;
        return gg;
    }

    public void SetMainRoom()
    {
        PlayerState.GetInstance().currentRoom = this;
        List<RoomData> rooms = _connectors.Select(x => x.targetRoom).ToList();
        GameController.GetInstance().activesRoom = rooms;
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        _isColliding= true;
        _collider = collision.gameObject.GetComponent<RoomData>();
    }

    public bool IsColliding()
    {
        return _isColliding;
    }

    public void OnDestroy()
    {
        if (_collider != null)
        {
            _collider._isColliding= false;
            _collider._collider = null;
            _collider = null;
        }
    }

    public RoomConnector[] GetConnectors()
    {
        return _connectors;
    }

    public List<RoomData> GetConnectedRooms()
    {
        List<RoomData> roomDatas = new List<RoomData>
        {
            this
        };
        return roomDatas.Union(_connectors.Select(x => x.targetRoom)).ToList();
    }
}

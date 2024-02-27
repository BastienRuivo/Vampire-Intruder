using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RoomData : MonoBehaviour
{
    public enum Type
    {
        HALLS,
        TREASURES,
        CORRIDORS,
        LIBRARIES,
        BEDROOMS,
        OFFICES,
        PRISONS,
        LIVINGROOMS,
        CHURCHES,
        STOCKAGES,
        NOONE
    }

    public static string GetStringFromType(Type type)
    {
        string res = type.ToString().ToLower();
        return char.ToUpper(res[0]) + res.Substring(1);
    }
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
    //private static readonly float _nodeSize = 0.176775f;
    ///// <summary>
    ///// Size of an obstacle in the graph
    ///// </summary>
    //private static readonly float _obstacleSize = 1.5f;
    private bool _isColliding;
    private RoomData _collider;
    private RoomConnector[] _connectors;
    public float energy;
    public List<GuardManager> guards;

    public Material defaultMtl;
    public Material transparentMtl;
    private IEnumerator _roomFadeAway = null;

    public Renderer wall;
    public Renderer wallDown;
    public Tilemap floor;
    private bool _fade = false;

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
    //public GridGraph BuildGraph(Vector3 pos)
    //{
    //    GridGraph gg = AstarPath.active.data.AddGraph(typeof(GridGraph)) as GridGraph;
    //    gg.SetGridShape(InspectorGridMode.IsometricGrid);
    //    gg.isometricAngle = 60f;
    //    gg.rotation = new Vector3(-45f, 270f, 90f);
    //    gg.center = localGraphPosition + pos;
    //    gg.SetDimensions(width, height, _nodeSize);
    //    gg.collision.use2D = true;
    //    gg.collision.diameter = _obstacleSize;
    //    gg.collision.mask = layers;
    //    return gg;
    //}

    public void SetMainRoom()
    {
        PlayerState.GetInstance().currentRoom = this;
        List<RoomData> rooms = _connectors.Select(x => x.targetRoom).ToList();
        GameController.GetInstance().activesRoom = rooms;
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

    public void SetVisbility(float value)
    {
        Debug.Log("Set room visibility for " + name + " = " + value.ToString());
        foreach (Renderer renderer in GetComponentsInChildren<Renderer>())
        {
            if (!renderer.material.HasColor("_Color"))
            {
                continue;
            }
            Color c = renderer.material.color;
            if (renderer.material.name.Contains(defaultMtl.name) || renderer.material.name.Contains("Glow"))
            {
                c.a = value * defaultMtl.color.a;
            }
            else
            {
                c.a = value * transparentMtl.color.a;
            }
            renderer.material.color = c;
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

    public void EnterToAdjacent(RoomData other, Direction dir)
    {
        if (_roomFadeAway != null)
        {
            StopCoroutine(_roomFadeAway);
        }

        other.SetVisbility(1f);

        switch (dir)
        {
            case Direction.NORTH:
            case Direction.WEST:
                wall.material.color = transparentMtl.color;
                break;
            case Direction.SOUTH: 
            case Direction.EAST:
                other.wall.material.color = transparentMtl.color;
                break;
            default: break;
        }
    }

    public void SetCurrent(RoomData target)
    {
        if (target._roomFadeAway != null)
        {
            target._fade = false;
            StopCoroutine(target._roomFadeAway);
        }
        SetVisbility(1f);

        _roomFadeAway = DisableRoom(target);
        _fade = true;
        StartCoroutine(_roomFadeAway);

        PlayerState.GetInstance().currentRoom = this;
        GameController.GetInstance().OnRoomChange(this);
    }

    public IEnumerator DisableRoom(RoomData target)
    {
        for (float alpha = 1f; alpha > 0.02f; alpha -= Time.deltaTime)
        {
            if (!_fade) break;
            alpha = Mathf.Max(alpha, 0.02f);
            target.SetVisbility(alpha);
            yield return null;
        }
    }
}

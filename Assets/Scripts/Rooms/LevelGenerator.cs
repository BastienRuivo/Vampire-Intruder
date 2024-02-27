using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Tilemaps;

[System.Serializable]
public class RoomsHandler
{
    public GameObject[] rooms;
    public int maxRoomPerLevel = -1;
    public RoomsHandler(GameObject[] rooms, int maxRoomPerLevel)
    {
        this.rooms = rooms;
        this.maxRoomPerLevel = maxRoomPerLevel;
    }
}

[System.Serializable]
public class RoomParameter
{
    public RoomData.Type type;
    public float cost;
    public int maxPerLvl;
    public RoomParameter(RoomData.Type type, float cost, int maxPerLvl)
    {
        this.type = type;
        this.cost = cost;
        this.maxPerLvl = maxPerLvl;
    }
}

public class LevelGenerator : Singleton<LevelGenerator>
{
    
    public Dictionary<RoomData.Type, RoomsHandler> roomMaps;

    public RoomParameter[] roomParameters =
    {
        // HALL ALWAYS COST 0
        new RoomParameter(RoomData.Type.HALLS, 0f, 1),
        // TREASURE ROOM COST
        new RoomParameter(RoomData.Type.TREASURES, 0.1f, 1),
        // CORRIDOR ROOM COST
        new RoomParameter(RoomData.Type.CORRIDORS, 0.2f, -1),
        // LIBRARY ROOM COST
        new RoomParameter(RoomData.Type.LIBRARIES, 0.3f, 2),
        // BEDROOM ROOM COST
        new RoomParameter(RoomData.Type.BEDROOMS, 0.1f, 4),
        // OFFICE ROOM COST
        new RoomParameter(RoomData.Type.OFFICES, 0.2f, 2),
        // PRISON ROOM COST
        new RoomParameter(RoomData.Type.PRISONS, 0.3f, 1),
        // LIVINGROOM ROOM COST
        new RoomParameter(RoomData.Type.LIVINGROOMS, 0.3f, 3),
        // CHURCH ROOM COST
        new RoomParameter(RoomData.Type.CHURCHES, 0.4f, 1),
        // STOCKAGE ROOM COST
        new RoomParameter(RoomData.Type.STOCKAGES, 0.15f, 3)
    };

    int completeRoomType = 0;

    public Tile[] replacement;

    public List<GameObject> instanciatedRooms;
    public List<RoomData> convexHull;
    public Queue<GameObject> roomToFill;

    public float startingEnergy = 5f;
    bool hasStart = false;

    private static float _maximumOverlapp = 0.3f;

    private string _mainObjective = "";
    private bool _hasGenerateMain = false;

    public string seed = "";

    

    ContactFilter2D filter;
    private IEnumerator _coroutine;

    /// <summary>
    /// Load an array of game object from ressources
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    void LoadRooms(RoomData.Type type)
    {
        RoomsHandler rm = new RoomsHandler(Resources.LoadAll<GameObject>("Rooms/" + RoomData.GetStringFromType(type)), -1);
        roomMaps.Add(type, rm);
    }

    /// <summary>
    ///  Load all rooms from ressource into memory
    /// </summary>
    public void LoadData()
    {
        roomMaps = new Dictionary<RoomData.Type, RoomsHandler>();
        for(RoomData.Type i = RoomData.Type.HALLS; i < RoomData.Type.NOONE; i++)
        {
            LoadRooms(i);
            Debug.Log("There is " + roomMaps[i].rooms.Length.ToString() + " maps of type " + i.ToString());
        }
    
        filter = new ContactFilter2D();
        filter.SetLayerMask(LayerMask.GetMask("RoomColl"));
    }

    /// <summary>
    /// Place the base, which is a Hall on the 0 0 of the map
    /// </summary>
    public void PlaceHall()
    {
        int rand = Random.Range(0, roomMaps[RoomData.Type.HALLS].rooms.Length);
        int selected = rand;
        var instance = Instantiate(roomMaps[RoomData.Type.HALLS].rooms[selected]);
        roomToFill= new Queue<GameObject>();
        roomToFill.Enqueue(instance);
    }

    /// <summary>
    /// Fill all empty connectors of a room
    /// </summary>
    /// <param name="room">Room game object with connectors to fill</param>
    public void FillRoom(GameObject room, float addEnergy = 0f)
    {
        instanciatedRooms.Add(room);

        RoomData roomData = room.GetComponent<RoomData>();

        GameObject graph = room.transform.Find("CustomPivot/Graph").gameObject;

        if (graph != null) Destroy(graph);

        var grid = roomData.floor.layoutGrid;
        roomData.energy += addEnergy;

        if(!_hasGenerateMain)
        {
            Interactible[] inters = roomData.GetComponentsInChildren<Interactible>();

            foreach (Interactible interactible in inters)
            {
                if (interactible.reference == _mainObjective)
                {
                    _hasGenerateMain = true;
                    break;
                }
            }
        }



        int connectionsNumber = roomData.GetConnectors().Length;
        //Debug.Log("Fill " + room.name + " with energy " + roomData.energy);
        // If energy is neg, then it's over
        if (!CanPlaceAnything() || roomData.energy <= 0f)
        {
            roomData.GetConnectors().Where(c => !c.isFilled).ToList().ForEach(c =>
            {
                GameObject go = c.dir == Direction.NORTH || c.dir == Direction.WEST ? roomData.wall.gameObject : roomData.wallDown.gameObject;
                Tilemap tm = go.gameObject.GetComponent<Tilemap>();
                Tile t;
                switch (c.dir)
                {
                    case Direction.NORTH:
                        t = replacement[2]; break;
                    case Direction.EAST:
                        t = replacement[3]; break;
                    case Direction.SOUTH:
                        t = replacement[0]; break;
                    case Direction.WEST:
                    default:
                        t = replacement[1]; break;
                }
                tm.SetTile(c.coordOnGrid, t);
                c.SetActiveState(false);
                connectionsNumber--;
            });
            convexHull.Add(roomData);
            return;
        }
        // For all connectors of the room, try to place somtheing
        roomData.GetConnectors().Where(c => !c.isFilled).ToList().ForEach(c =>
        {
            //Debug.Log("Filling connector " + c.name);
            Vector3Int directionOffset = DirectionHelper.DirectionOffsetGrid(c.dir) + new Vector3Int(1, 1, 0);
            Vector3 cPos = grid.CellToWorld(c.coordOnGrid + directionOffset);

            List<RoomData.Type> typeTested = new List<RoomData.Type>();

            // If energy is really low, avoid generating corridor
            if(roomData.energy - roomParameters[(int)RoomData.Type.CORRIDORS].cost <= 0f)
            {
                typeTested.Add(RoomData.Type.CORRIDORS);
            }

            foreach(RoomParameter param in roomParameters)
            {
                if(param.maxPerLvl == 0 && c.allowedBranch.Contains(param.type))
                {
                    typeTested.Add(param.type);
                }
            }

            RoomData.Type roomType = PickRoomType(c, typeTested);

            //Debug.Log("First type picked = " + roomType.ToString());
            bool generated = false;

            // Try all room type allowed till something can be spawned
            while(roomType != RoomData.Type.NOONE && !generated)
            {
                typeTested.Add(roomType);
                List<GameObject> roomTested = new List<GameObject>();
                GameObject instance = PickRoomToPlace(c, roomType, roomTested);

                //Debug.Log("Testin' type " + roomType.ToString());

                // Try all room prefab of this type till something can be spawner
                while(instance != null && !generated )
                {
                    roomTested.Add(instance);

                    instance.name += Random.Range(0, 42).ToString();

                    Direction invDir = DirectionHelper.Inverse(c.dir);
                    RoomData instData = instance.GetComponent<RoomData>();
                    RoomConnector[] instConnectors = instData.GetConnectors().Where(co => co.dir == invDir).ToArray();
                    if (instConnectors.Length <= 0)
                    {
                        // Usually when you put a wrong direction in one of the rooms prefab
                        //Debug.Log("Room " + instance.name + " is bugging for orientation " + invDir.ToString());
                        return;
                    }
                    // Place the instance in the scene, false if it can't be placed
                    bool findPlace = PlaceInstanceAtConnector(roomData, instData, c, instConnectors, cPos);

                    if (findPlace)
                    {
                        // Enqueue new instance to fill its connectors
                        roomToFill.Enqueue(instance);
                        generated = true;
                        // Update new instance energy
                        if (roomParameters[(int)roomType].maxPerLvl > 0)
                        {
                            roomParameters[(int)roomType].maxPerLvl--;
                        }
                        instance.GetComponent<RoomData>().energy = roomData.energy - roomParameters[(int)RoomData.Type.CORRIDORS].cost;
                        break;
                    }
                    else
                    {
                        // Destroy the instance that can't be placed
                        //if(instance.name != "CorridorsHorizontal(Clone)33")
                        instance.transform.position = new Vector3(1000, 1000, 1000);
                        Destroy(instance);
                        instance = PickRoomToPlace(c, roomType, roomTested);
                    }
                }

                // If no room of this type can be placed, then search another room type
                if(!generated)
                {
                    roomType = PickRoomType(c, typeTested);
                }
            }

            if(!generated)
            {
                Transform go = c.dir == Direction.NORTH || c.dir == Direction.WEST ? roomData.wall.transform : roomData.wallDown.transform;
                Tilemap tm = go.GetComponent<Tilemap>();
                Tile t;
                switch(c.dir)
                {
                    case Direction.NORTH:
                        t = replacement[2]; break;
                    case Direction.EAST:
                        t = replacement[3]; break;
                    case Direction.SOUTH:
                        t = replacement[0]; break;
                    case Direction.WEST:
                    default:
                        t = replacement[1]; break;
                }
                tm.SetTile(c.coordOnGrid, t);
                c.SetActiveState(false);
                connectionsNumber--;
            }
            
        });
        Debug.Log("Connection number for room " + room.name + " is " + connectionsNumber.ToString());
        if(connectionsNumber == 1)
        {
            if(roomData.type == RoomData.Type.CORRIDORS)
            {
                RoomConnector c = roomData.GetConnectors().Where(c => c.isFilled).FirstOrDefault();
                c = c.targetRoomConnector;
                Destroy(roomData.gameObject);
                instanciatedRooms.Remove(roomData.gameObject);
                GameObject go = c.dir == Direction.NORTH || c.dir == Direction.WEST ? c.room.wall.gameObject : c.room.wallDown.gameObject;
                Tilemap tm = go.gameObject.GetComponent<Tilemap>();
                Tile t;
                switch (c.dir)
                {
                    case Direction.NORTH:
                        t = replacement[2]; break;
                    case Direction.EAST:
                        t = replacement[3]; break;
                    case Direction.SOUTH:
                        t = replacement[0]; break;
                    case Direction.WEST:
                    default:
                        t = replacement[1]; break;
                }
                tm.SetTile(c.coordOnGrid, t);
                c.SetActiveState(false);
            }
            else
            {
                convexHull.Add(roomData);
            }
        }
    
    }

    /// <summary>
    /// Select a connector and place an istancied room at the connected room
    /// </summary>
    /// <param name="room">Current room that instanciate a new room</param>
    /// <param name="instance">Instancied room</param>
    /// <param name="c">Connector of the old room</param>
    /// <param name="connectors">Connectors of the new room</param>
    /// <param name="cPos">C connector pos is world</param>
    /// <returns>Boolean that indicate if the room has been place without collision</returns>
    public bool PlaceInstanceAtConnector(RoomData room, RoomData instance, RoomConnector c, RoomConnector[] instConnectors, Vector3 cPos)
    {
        // To see if 2 rooms overlap, we check the collider on the floor
        Grid inGrid = instance.floor.layoutGrid;
        var collider = instance.floor.GetComponent<TilemapCollider2D>();

        // We then iterate on each collider that are valid 
        for (int i = 0; i < instConnectors.Length; i++)
        {
            RoomConnector other = instConnectors[i];

            // Get the offset on the room to this connector to mach the placement
            Vector3 otherLocal = inGrid.CellToWorld(other.coordOnGrid + new Vector3Int(1, 1, 0));
            instance.transform.position = cPos - otherLocal;

            Physics2D.SyncTransforms();

            // Make a physics collision test
            Collider2D[] test = new Collider2D[8];
            int t = Physics2D.OverlapCollider(collider, filter, test);
            // If there is only one collision with the previous room, and it does not collide a lot, then proceed
            //FillConnectors(other, c, instance, room);
            bool _overlap = false;

            for(int j = 0; j < Mathf.Min(test.Length, t); j++)
            {

                //Debug.Log(Mathf.Abs(Physics2D.Distance(collider, test[j]).distance) + " < " + _maximumOverlapp);

                if (Mathf.Abs(Physics2D.Distance(collider, test[j]).distance) >= _maximumOverlapp)
                {
                    //Debug.Log(collider.transform.parent.parent.parent.name + " connector " + other.name  + " can't connect to " + other.name + " because of " + test[j].transform.parent.parent.parent.name);
                    _overlap = true;
                    break;
                }
            }
            if (_overlap) continue;

            // Fill the two now connected connectors
            FillConnectors(other, c, instance.GetComponent<RoomData>(), room.GetComponent<RoomData>());
            return true;
        }

        return false;
    }

    /// <summary>
    /// Pick a room type from a connector
    /// </summary>
    /// <param name="connector">Connector from chich we need a connection type</param>
    /// <param name="forbidden">List of already tried and therefore forbidden connection type</param>
    /// <returns></returns>
    public RoomData.Type PickRoomType(RoomConnector connector, List<RoomData.Type> forbidden)
    {

        RoomData.Type typeToCreate = RoomData.Type.NOONE;

        if(connector.allowedBranch.Length != connector.branchPercentage.Length)
        {
            Debug.LogError("WAH LA PUTAIN DE TA RACE" + connector.name);
            return RoomData.Type.NOONE;
        }


        // If we tried every type of room, then return noone
        if(forbidden.Count >= connector.allowedBranch.Length)
        {
            return typeToCreate;
        }

        // Generate a random number till we have an allowed type
        do
        {
            int roll = Random.Range(0, 100);
            typeToCreate= connector.GetFromDice(roll);
        }
        while((roomParameters[(int)typeToCreate].maxPerLvl == 0 || forbidden.Contains(typeToCreate)));


        return typeToCreate;
    }

    /// <summary>
    /// Pick a room from a list to place on map
    /// </summary>
    /// <param name="connector">Connector of the parent room</param>
    /// <param name="type">Type of the room to pick</param>
    /// <param name="forbidden">List of already tried rooms from this type</param>
    /// <returns></returns>
    public GameObject PickRoomToPlace(RoomConnector connector, RoomData.Type type, List<GameObject> forbidden)
    {
        GameObject prefab = null;
        prefab = RandomRoom(type, connector.dir, forbidden);
        if (prefab == null) return null;
        forbidden.Add(prefab);
        return Instantiate(prefab);
    }

    /// <summary>
    /// Fill two paired connectors
    /// </summary>
    /// <param name="A">Room connector of croom</param>
    /// <param name="B">Room connector of troom</param>
    /// <param name="croom">Game object of room A</param>
    /// <param name="troom">Game object of room B</param>
    private void FillConnectors(RoomConnector A, RoomConnector B, RoomData croom, RoomData troom)
    {
        // Fill the A connector
        A.isFilled = true;
        A.room = croom;
        A.targetRoom = troom;
        A.targetRoomConnector = B;

        // Fill the B Connector
        B.isFilled = true;
        B.room = troom;
        B.targetRoom = croom;
        B.targetRoomConnector = A;

        A.SetActiveState(true);
        B.SetActiveState(true);
    }
    
    /// <summary>
    /// Pick a random room prefab from the loaded files
    /// </summary>
    /// <param name="array">Array of prefab object loaded from resources</param>
    /// <param name="dir">Direction allowed</param>
    /// <returns>A random game object to instanciate</returns>
    public GameObject RandomRoom(RoomData.Type type, Direction dir, List<GameObject> forbidden)
    {
        var roomsInstatiable = roomMaps[type].rooms.Where(r => !forbidden.Contains(r) && r.GetComponent<RoomData>().connectableDir.Contains(dir));
        if(roomsInstatiable.Count() <= 0) return null;
        return roomsInstatiable.ElementAt(Random.Range(0, roomsInstatiable.Count()));

    }

    /// <summary>
    /// Load and generate a complete lvl.
    /// </summary>
    /// <returns>The hall room</returns>
    public RoomData Generate(AppState.Level level)
    {
        _mainObjective = level.mainRef;
        convexHull = new List<RoomData>();
        seed = level.seed;
        startingEnergy = level.startingEnergy;
        Debug.Log("Level is generating with seed " + seed);
        if(!string.IsNullOrEmpty(seed)) 
        { 
            Random.InitState(StringToInt(seed));
        }

        instanciatedRooms = new List<GameObject>();
        LoadData();
        PlaceHall();
        FillRoom(roomToFill.Dequeue(), startingEnergy);

        _coroutine = GenerateLevel();
        StartCoroutine(_coroutine);

        hasStart = true;

        return instanciatedRooms[0].GetComponent<RoomData>();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space) && roomToFill.Count() > 0)
        {
            if (!hasStart)
            {
                //Debug.Log("Starting " + startingEnergy.ToString());
                FillRoom(roomToFill.Dequeue(), startingEnergy);
                hasStart = true;
            }
            else
            {
                FillRoom(roomToFill.Dequeue());
            }
            //Debug.Log("Remain " + roomToFill.Count + " in queue");
        }
    }

    public static int StringToInt(string str)
    {
        int sum = 0;
        foreach(char c in str)
        {
            sum += (int)c;
        }
        return sum;
    }


    private IEnumerator GenerateLevel()
    {
        yield return null;
        while (roomToFill.Count > 0)
        {
            FillRoom(roomToFill.Dequeue());
            Debug.Log("To fill = " + roomToFill.Count);
            yield return null;
        }



        Debug.Log("ConvexHull size is " + convexHull.Count.ToString() + " has placed main ? " + _hasGenerateMain.ToString());

        if (!_hasGenerateMain)
        {
            Debug.Log("Try to generate main");
            List<GameObject> candidate = new List<GameObject>();
            roomMaps.Select(e => e.Value.rooms.ToList()).ToList().ForEach(rm => rm.ForEach(r => candidate.Add(r)));
            candidate = candidate.Where(c => c.GetComponentsInChildren<Interactible>().Any(a => a.reference == _mainObjective)).ToList();
            Debug.Log("There is " + candidate.Count.ToString() + " candidates");
            foreach (RoomData room in convexHull)
            {
                if (room.type == RoomData.Type.HALLS) continue;
                RoomConnector connector = room.GetConnectors().Where(c => c.isFilled).FirstOrDefault();
                if (connector == null) continue;
                connector = connector.targetRoomConnector;
                Grid grid = connector.room.floor.layoutGrid;
                Vector3Int directionOffset = DirectionHelper.DirectionOffsetGrid(connector.dir) + new Vector3Int(1, 1, 0);
                Vector3 cPos = grid.CellToWorld(connector.coordOnGrid + directionOffset);
                List<GameObject> localCandidates = candidate.Where(c => c.GetComponent<RoomData>().connectableDir.Contains(connector.dir)).ToList();
                bool canFit = false;
                Vector3 origin = room.transform.position;
                room.transform.position = new Vector3(10000, 10000);
                foreach (GameObject lc in localCandidates)
                {
                    GameObject instance = Instantiate(lc);
                    Physics2D.SyncTransforms();
                    RoomData instData = instance.GetComponent<RoomData>();
                    canFit = PlaceInstanceAtConnector(connector.room, instData, connector, instData.GetConnectors(), cPos);
                    Debug.Log(canFit);
                    if (canFit)
                    {
                        instanciatedRooms.Add(instance);
                        break;
                    }
                    else
                    {
                        instance.transform.position = new Vector3(10000, 10000);
                        Destroy(instance);
                    }
                    yield return new WaitForSeconds(0.5f);
                }
                if (canFit)
                {
                    _hasGenerateMain = true;
                    instanciatedRooms.Remove(room.gameObject);
                    Destroy(room);
                    break;
                }
                else
                {
                    room.transform.position = origin;
                }
            }
        }

        //foreach (RoomData rd in convexHull)
        //{
        //    rd.GetComponentsInChildren<Renderer>().ToList().ForEach(r => r.material.color = Color.red);
        //}

        Debug.Log("Has placed main ? " + _hasGenerateMain);

        GameController.GetInstance().OnLevelLoadComplete(instanciatedRooms.Select(x => x.GetComponent<RoomData>()).ToList());
        //Destroy(this);
    }

    public bool CanPlaceAnything()
    {
        if(completeRoomType >= (int)RoomData.Type.NOONE)
        {
            return false;
        }

        completeRoomType = 0;
        foreach(RoomParameter rp in roomParameters)
        {
            if (rp.maxPerLvl > 0) completeRoomType++;
        }

        return completeRoomType < (int)RoomData.Type.NOONE;
    }
}

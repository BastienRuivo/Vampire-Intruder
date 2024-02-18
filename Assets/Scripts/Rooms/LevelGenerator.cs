using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelGenerator : Singleton<LevelGenerator>
{
    public GameObject[] halls;
    public GameObject[] treasures;
    public GameObject[] decorations;
    public GameObject[] corridors;

    public Tile[] replacement;

    public List<GameObject> instanciatedRooms;
    public Queue<GameObject> roomToFill;

    public float startingEnergy = 5f;
    bool hasStart = false;

    private static string _pathToLayer = "CustomPivot/Props/";
    private static string _floor = _pathToLayer + "Floor";
    private static string _walls = _pathToLayer + "Walls";
    private static string _hiddenWalls = _pathToLayer + "WallsDown";
    private static float _maximumOverlapp = 0.3f;

    ContactFilter2D filter;

    private IEnumerator _coroutine;

    /// <summary>
    /// Load an array of game object from ressources
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    GameObject[] LoadRooms(string file)
    {
        return Resources.LoadAll<GameObject>(file);
    }

    /// <summary>
    ///  Load all rooms from ressource into memory
    /// </summary>
    public void LoadData()
    {
        halls = LoadRooms("Rooms/Halls");
        treasures = LoadRooms("Rooms/Treasures");
        decorations = LoadRooms("Rooms/Decorations");
        corridors = LoadRooms("Rooms/Corridors");

        Debug.Log("Halls has " + halls.Length + " rooms");
        Debug.Log("Treasures has " + halls.Length + " rooms");
        Debug.Log("Decorations has " + halls.Length + " rooms");
        Debug.Log("Corridors has " + corridors.Length + " rooms");

        filter = new ContactFilter2D();
        filter.SetLayerMask(LayerMask.GetMask("RoomColl"));
    }

    /// <summary>
    /// Place the base, which is a Hall on the 0 0 of the map
    /// </summary>
    public void PlaceHall()
    {
        int selected = Random.Range(0, halls.Length - 1);
        var instance = Instantiate(halls[selected]);
        roomToFill= new Queue<GameObject>();
        roomToFill.Enqueue(instance);
    }

    /// <summary>
    /// Fill all empty connectors of a room
    /// </summary>
    /// <param name="room">Room game object with connectors to fill</param>
    public void FillRoom(GameObject room, float addEnergy = 0f)
    {
        var connectors = room.GetComponentsInChildren<RoomConnector>();
        instanciatedRooms.Add(room);

        var grid = room.transform.Find(_floor).GetComponent<Tilemap>().layoutGrid;
        RoomData roomData = room.GetComponent<RoomData>();
        roomData.energy += addEnergy;

        //Debug.Log("Fill " + room.name + " with energy " + roomData.energy);
        // If energy is neg, then it's over
        if (roomData.energy <= 0f) return;
        // For all connectors of the room, try to place somtheing
        connectors.Where(c => !c.isFilled).ToList().ForEach(c =>
        {
            //Debug.Log("Filling connector " + c.name);
            Vector3Int directionOffset = DirectionHelper.DirectionOffsetGrid(c.dir) + new Vector3Int(1, 1, 0);
            Vector3 cPos = grid.CellToWorld(c.coordOnGrid + directionOffset);

            List<RoomData.Type> typeTested = new List<RoomData.Type>();

            // If energy is really low, avoid generating corridor
            if(roomData.energy - RoomData.TypeCost[(int)RoomData.Type.CORRIDOR] <= 0f)
            {
                typeTested.Add(RoomData.Type.CORRIDOR);
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
                    RoomConnector[] instConnectors = instance.GetComponentsInChildren<RoomConnector>().Where(co => co.dir == invDir).ToArray();
                    if (instConnectors.Length <= 0)
                    {
                        // Usually when you put a wrong direction in one of the rooms prefab
                        //Debug.Log("Room " + instance.name + " is bugging for orientation " + invDir.ToString());
                        return;
                    }
                    // Place the instance in the scene, false if it can't be placed
                    bool findPlace = PlaceInstanceAtConnector(room, instance, c, instConnectors, cPos);

                    if (!findPlace)
                    {
                        // Destroy the instance that can't be placed
                        //if(instance.name != "CorridorsHorizontal(Clone)33")
                        instance.transform.position = new Vector3(1000, 1000, 1000);
                        Destroy(instance);
                        instance = PickRoomToPlace(c, roomType, roomTested);
                    }
                    else
                    {
                        // Enqueue new instance to fill its connectors
                        roomToFill.Enqueue(instance);
                        generated = true;
                        // Update new instance energy
                        instance.GetComponent<RoomData>().energy = roomData.energy - RoomData.TypeCost[(int)roomType];
                        break;
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
                // TODO : CAN'T FILL THE INSTERSECTION WITH A ROOM, MASK IT
                //Debug.Log("I Can't fill this intersection batman");
                Transform go = c.dir == Direction.NORTH || c.dir == Direction.WEST ? room.transform.Find(_walls) : room.transform.Find(_hiddenWalls);
                Tilemap tm = go.gameObject.GetComponent<Tilemap>();
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
                c.SetInactive();


            }
            
        });
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
    public bool PlaceInstanceAtConnector(GameObject room, GameObject instance, RoomConnector c, RoomConnector[] instConnectors, Vector3 cPos)
    {
        // To see if 2 rooms overlap, we check the collider on the floor
        var floor = instance.transform.Find(_floor);
        Grid inGrid = floor.GetComponent<Tilemap>().layoutGrid;
        var collider = floor.GetComponent<TilemapCollider2D>();

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
            FillConnectors(other, c, instance, room);
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
        while(forbidden.Contains(typeToCreate));


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
        switch (type)
        {
            case RoomData.Type.TREASURE: prefab = RandomOnArray(treasures, connector.dir, forbidden); break;
            case RoomData.Type.DECORATION: prefab = RandomOnArray(decorations, connector.dir, forbidden); break;
            case RoomData.Type.CORRIDOR: prefab =  RandomOnArray(corridors, connector.dir, forbidden); break;
            default: Debug.Log("Achievement Get :: How did we get here ? Level Type Gen."); break;
        }
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
    private void FillConnectors(RoomConnector A, RoomConnector B, GameObject croom, GameObject troom)
    {
        // Fill the A connector
        A.isFilled = true;
        A.currentRoom = croom;
        A.targetRoom = troom;
        A.targetRoomConnector = B;

        // Fill the B Connector
        B.isFilled = true;
        B.currentRoom = troom;
        B.targetRoom = croom;
        B.targetRoomConnector = A;

        switch (A.dir)
        {
            case Direction.NORTH:
            case Direction.WEST:
                A.currentWalls = croom.transform.Find(_walls).gameObject;
                A.targetWalls = troom.transform.Find(_hiddenWalls).gameObject;
                break;
            case Direction.SOUTH:
            case Direction.EAST:
                A.currentWalls = croom.transform.Find(_hiddenWalls).gameObject;
                A.targetWalls = troom.transform.Find(_walls).gameObject;
                break;
        }
        B.currentWalls = A.targetWalls;
        B.targetWalls = A.currentWalls;
    }
    
    /// <summary>
    /// Pick a random room prefab from the loaded files
    /// </summary>
    /// <param name="array">Array of prefab object loaded from resources</param>
    /// <param name="dir">Direction allowed</param>
    /// <returns>A random game object to instanciate</returns>
    public GameObject RandomOnArray(GameObject[] array, Direction dir, List<GameObject> forbidden)
    {
        var roomsInstatiable = array.Where(r => !forbidden.Contains(r) && r.GetComponent<RoomData>().connectableDir.Contains(dir));
        if(roomsInstatiable.Count() <= 0) return null;
        return roomsInstatiable.ElementAt(Random.Range(0, roomsInstatiable.Count()));
    }

    /// <summary>
    /// Load and generate a complete lvl.
    /// </summary>
    /// <returns> List of all generated room gameObject</returns>
    public List<GameObject> Generate()
    {
        System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();

        instanciatedRooms = new List<GameObject>();
        LoadData();
        Random.InitState(249);
        PlaceHall();
        FillRoom(roomToFill.Dequeue(), startingEnergy);

        _coroutine = GenerateLevel();
        StartCoroutine(_coroutine);

        hasStart= true;




        //Debug.Log("There is " + instanciatedRooms.Count + " rooms generated in " + stopwatch.ElapsedMilliseconds + " ms");
        
        return instanciatedRooms;
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


    private IEnumerator GenerateLevel()
    {
        yield return null;
        while (roomToFill.Count > 0)
        {
            FillRoom(roomToFill.Dequeue());
            yield return null;
        }
        GameController.GetInstance().OnLevelLoadComplete();
        //Destroy(this);
    }
}

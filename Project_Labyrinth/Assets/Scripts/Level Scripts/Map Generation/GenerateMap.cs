/*The MIT License (MIT)

Copyright (c) 2019 Ryan Vazquez

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.*/

using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GenerateMap : MonoBehaviour
{
    public delegate void OnEventTrigger(int num);
    public static OnEventTrigger trigger;

    // Cell type used to identify
    // if space is used for room, hallway, or empty space
    enum CellType{
        Empty,
        Room,
        Hallway,
        Entrance
    }

    bool triangulationOn = false;

    [SerializeField]
    bool toggleItemSpawn;

    [SerializeField]
    bool toggleSeedUsage;

    [SerializeField]
    float mapSeed;

    float currentSeed;

    private int currentLevel;

    [SerializeField] public string nextScene;
    [SerializeField] public string previousScene;

    // Grid Related
    Vector2Int gridSize;

    [SerializeField]
    Vector2Int gridDimensions;

    [SerializeField]
    int worldScaleMultiplier;

    // Room Related
    [SerializeField]
    public int numberOfRooms;

    int roomsAdded;

    bool startingRoomAdded;
    bool endingRoomAdded;

    private GameObject player;

    [SerializeField] private GameObject killSwitchPrefab;


    // other
    [SerializeField]
    float PrimEdgeCycleChance;

    [SerializeField]
    GameObject playerPrefab;

    // Room Prefabs
    [SerializeField]
    List<GameObject> roomPrefabs;

    [SerializeField]
    bool includeBossRoom;

    [SerializeField]
    GameObject bossRoomPrefab;

    [SerializeField]
    List<GameObject> hallwayPrefabs;

    [SerializeField] private List<GameObject> enemiesToSpawn;
    [SerializeField] private int[] enemyCount; 


    // Map Instance Variables
    Grid<CellType> grid;
    List<Room> rooms;
    List<GameObject> spawnedRooms;
    Delaunay2D delaunay2D;

    GameObject parentRoom;
    GameObject parentLine;

    HashSet<Edge> mapEdges;

    public GameObject EnemySpawner;

    bool bossRoomIncluded;
    public bool spawnKillSwitch;


    private void Awake()
    {
        trigger = ChangeLevel;
    }

    void Start()
    {
        
        player = GameObject.FindWithTag("Player");
        CreateMap();
    }

    public void CreateMap()
    {
        resetMap();

        // Steps in creating the map
        SpawnRooms();
        DalaunayTriangulation();
        Prims();
        Pathfinding();
        OtherWork();
        SpawnItems();
        EnemySpawnerMaker();
    }


    // Will need to change these to read room data
    // from a .Json
    void SpawnRooms()
    {
        Vector2Int roomPos;
        roomsAdded = 0;


        // This code is to spawn the random rooms
        while(roomsAdded < numberOfRooms)
        {
            // Get Room positions
                roomPos = new Vector2Int(
                Random.Range(1, gridSize.x - 1),
                Random.Range(1, gridSize.y - 1));

            // Set Room type
            GameObject roomToSpawn = GetRoomType();

            bool canAdd = true;

            // Create buffer and room to be added if buffer goes through without collisions
            Room tempRoom = new Room(roomToSpawn, roomPos);
            Room bufferRoom = tempRoom.CloneBuffer(new Vector2Int(-1, -1), new Vector2Int(2, 2));

            // if Colliding with an existing room do not add
            if (bufferRoom.Colliding())
            {
                canAdd = false;
            }


            // If room exists outside of our grid do not add
            if (tempRoom.bounds.xMin < 0 || tempRoom.bounds.xMax >= gridSize.x
                || tempRoom.bounds.yMin < 0 || tempRoom.bounds.yMax >= gridSize.y)
            {
                canAdd = false;
            }

           // If conditions are met then add then add to collection of rooms
            if (canAdd)
            {
                if(roomToSpawn == roomPrefabs[0]) // ensures the starting room is added
                {
                    startingRoomAdded = true;
                }
                else if(roomToSpawn == roomPrefabs[1]) // ensures the ending room is added
                {
                    endingRoomAdded = true;
                }
                else if(roomToSpawn == bossRoomPrefab)
                {
                    bossRoomIncluded = true;
                }

                addRoom(tempRoom);
            }
        }
    }

    // Runs through Dalaunay triangulation and returns a List
    // of edges that satisfies the algorithms rules. This is then passed on to
    // Prim's to calculate the minumum spanding tree.
    void DalaunayTriangulation()
    {
        ClearLines();

        List<Vector2> coordinates = new List<Vector2>();

        foreach(Room room in rooms)
        {
            foreach(Vector2Int entrancePos in room.GetAllEntrances())
            {
                coordinates.Add((Vector2) entrancePos);
            }
        }

        delaunay2D = Delaunay2D.Triangulate(coordinates);
    }

    // Where Prim's is ran, will return a minimum spanning tree
    // in addition to some cycles
    void Prims()
    {
        List<Edge> prim_edges = new List<Edge>();
        List<Edge> prim_mst;

        // Adds each of the Dellaunay edges to a bank of Edges
        // to use in Prims
        foreach(Edge edge in delaunay2D.Edges)
        {
            prim_edges.Add(edge);
        }

        // Set the initial MST
        prim_mst = Prim_MST.MinimumSpanningTree(prim_edges, prim_edges[0].U);

        // Updates Hashsets to run through Prim's
        mapEdges = new HashSet<Edge>(prim_mst);
        HashSet<Edge> remaining_Edges = new HashSet<Edge>(prim_edges);
        remaining_Edges.ExceptWith(mapEdges);

        // Runs a random number generated to see which
        // Edges should be added to create cycles within the map
        foreach(Edge edge in remaining_Edges)
        {
            if(Random.Range(0.0f, 1.0f) < (float) PrimEdgeCycleChance/100)
            {
                mapEdges.Add(edge);
            }
        }

        // Visualization for the MST
        if(triangulationOn)
        {
            DrawGraph();
        }
        
    }

    // Where we run the A* algorithm to connect the rooms via hallways
    void Pathfinding()
    {
        A_Star_Pathfinding aStar = new A_Star_Pathfinding(gridSize);

        List<List<Vector2Int>> hallwayPaths = new List<List<Vector2Int>>();

        foreach (var edge in mapEdges)
        {
            // Get starting rooms
            Room startRoom = new Room(edge.U, Vector2Int.one);
            Room endRoom = new Room(edge.V, Vector2Int.one);

            var startPosf = startRoom.bounds.center;
            var endPosf = endRoom.bounds.center;

            // Start and end position of path
            var startPos = new Vector2Int((int)startPosf.x, (int)startPosf.y);
            var endPos = new Vector2Int((int)endPosf.x, (int)endPosf.y);


            var path = aStar.FindPath(startPos, endPos, (A_Star_Pathfinding.Node a, A_Star_Pathfinding.Node b) => {
                var pathCost = new A_Star_Pathfinding.PathCost();

                pathCost.cost = Vector2Int.Distance(b.Position, endPos);   

                
                if (grid[b.Position] == CellType.Hallway)
                {
                    pathCost.cost += 1;
                }
                else if (grid[b.Position] == CellType.Entrance)
                {
                    pathCost.cost += 1;
                }
                else if (grid[b.Position] == CellType.Empty)
                {
                    pathCost.cost += 5;
                }
                else if (grid[b.Position] == CellType.Room)
                {
                    pathCost.cost += 20;
                }


                pathCost.traversable = true;

                return pathCost;
            });

            // Add the created path to our list of paths
            hallwayPaths.Add(path);

            if (path != null)
            {
                for (int i = 0; i < path.Count; i++)
                {
                    var current = path[i];

                    if (grid[current] == CellType.Empty || grid[current] == CellType.Entrance)
                    {
                        grid[current] = CellType.Hallway;
                    }

                    if (i > 0)
                    {
                        var prev = path[i - 1];

                        var delta = current - prev;
                    }
                }
            }
        }

        // Re iterates through each path to determine which type of hallway node
        // each position on the grid will be
        HashSet<Vector2Int> uniqueHallways = new HashSet<Vector2Int>();

        // Creates a set of uni
        foreach (List<Vector2Int> path in hallwayPaths)
        {
            foreach (var pos in path)
            {
                uniqueHallways.Add(pos);
            }
            
        }

        foreach(Vector2Int hallway in uniqueHallways)
        {
            if (grid[hallway] == CellType.Hallway)
            {
                //Debug.Log(hallway);
                spawnHallway(hallway);
            }
        }
        

    }

    // Will Spawn Items in each room
    void SpawnItems()
    {
        foreach(GameObject room in spawnedRooms)
        {
            if(toggleItemSpawn)
            {
                room.GetComponent<Room_Controller>().SpawnItems();
            }
        }

        if (spawnKillSwitch)
        {
            var position = GameObject.FindObjectOfType<ItemSpawner>().gameObject.transform.position + 5 * Vector3.up;
            Instantiate(killSwitchPrefab, position, Quaternion.identity);
        }
    }

    // This is where all the setup that doesn't really have a label will go
    void OtherWork()
    {
        // This will rescale all of the rooms and hallways to a certain size
        GameObject _rooms = GameObject.Find("_rooms");

        _rooms.isStatic = true;
        _rooms.transform.localScale = new Vector3(worldScaleMultiplier, worldScaleMultiplier, worldScaleMultiplier);

        // This Bakes a new NavMesh after the map is created
        FindObjectOfType<NavMeshSurface>().BuildNavMesh();
        
        if(player != null)
            player.transform.position = GameObject.FindWithTag("PlayerStartPoint").transform.position + Vector3.up * 5f;
    }

    // Adds room to list of rooms and then spawns room in world space
    void addRoom(Room roomToAdd)
    {

        // Adds room to list of rooms
        rooms.Add(roomToAdd);

        // Spawns room in game space
        spawnRoom(roomToAdd.bounds.center, roomToAdd.bounds.size, roomToAdd.getRoomPrefab());

        roomsAdded++;

        // Updates the grid with positions that are considered to be
        // inside of a room.
        foreach (var pos in roomToAdd.bounds.allPositionsWithin)
        {
            grid[pos] = CellType.Room;
        }

        // Changes the cells to entraces to connect hallways
        foreach(Vector2Int entrancePos in roomToAdd.GetAllEntrances())
        {
            grid[entrancePos] = CellType.Entrance;
        }

    }

    // This function will need to be more complicated and
    // apply randomness into its runtime
    // in order to spawn all of the different types of rooms
    GameObject GetRoomType()
    {
        // Will account for every room besides the starting room
        int roomIndex = Random.Range(2, roomPrefabs.Count);

        //Debug.Log("" + roomIndex);

        if(!startingRoomAdded)
        {
            return roomPrefabs[0];
        }
        else if(!endingRoomAdded)
        {
            return roomPrefabs[1];
        }
        else if(includeBossRoom && !bossRoomIncluded)
        {
            return bossRoomPrefab;
        }
        else
        {
            return roomPrefabs[roomIndex];
        }
    }

    // This will spawn each individual room
    void spawnRoom(Vector2 position, Vector2Int size, GameObject room_type)
    {
        GameObject room;
        
        room = Instantiate(room_type, new Vector3(position.x, 0, position.y), Quaternion.identity);

        room.isStatic = true;
        room.transform.parent = parentRoom.transform;

        spawnedRooms.Add(room);
    }

    // Spawns the hallways between the rooms
    // Need to change to actually spawn hallways
    void spawnHallway(Vector2Int grid_position)
    {
        GameObject hall;
        GameObject hallwayType;

        float rotationAngle = 0.0f;

        bool north= false;
        bool south = false;
        bool east = false;
        bool west = false;

        if (grid_position.x < gridSize.x - 1)
        {
            north = grid[grid_position.x + 1, grid_position.y] == CellType.Hallway || grid[grid_position.x + 1, grid_position.y] == CellType.Room;
        }
        if(grid_position.x > 0)
        {
            south = grid[grid_position.x - 1, grid_position.y] == CellType.Hallway || grid[grid_position.x - 1, grid_position.y] == CellType.Room;
        }
        if(grid_position.y < gridSize.y - 1)
        {
            west = grid[grid_position.x, grid_position.y + 1] == CellType.Hallway || grid[grid_position.x, grid_position.y + 1] == CellType.Room;
        }
        if(grid_position.y > 0)
        {
            east = grid[grid_position.x, grid_position.y - 1] == CellType.Hallway || grid[grid_position.x, grid_position.y - 1] == CellType.Room;
        }

        // I am really not proud of this please look away
        if (west && east && north && south)
        {
            // Empty Hallway prefab
            hallwayType =  hallwayPrefabs[4];
        }
        else if (west && south && east)
        {
            //top single wall
            hallwayType =  hallwayPrefabs[3];
            rotationAngle = 90.0f;
        }
        else if (west && east && north)
        {
            //bottom single wall
            rotationAngle = 270.0f;
            hallwayType =  hallwayPrefabs[3];
        }
        else if (south && east && north)
        {
            // Left Single wall
            // no rotation
            hallwayType =  hallwayPrefabs[3];
        }
        else if (west && south && north)
        {
            // Right Single wall
            hallwayType =  hallwayPrefabs[3];
            rotationAngle = 180.0f;
        }
        else if (west && east)
        {
            // left to right parallel walls
            hallwayType = hallwayPrefabs[2];
            rotationAngle = 90.0f;
        }
        else if (south && north)
        {
            // down to up parallel walls
            hallwayType = hallwayPrefabs[2];
        }
        else if (south && east)
        {
            // corner one
            // no rotation
            hallwayType = hallwayPrefabs[1];
        }
        else if (west && south)
        {
            // corner two
            hallwayType = hallwayPrefabs[1];
            rotationAngle = 90.0f;
        }
        else if (west && north)
        {
            // corner three
            hallwayType = hallwayPrefabs[1];
            rotationAngle = 180.0f;
        }
        else if (east && north)
        {
            // corner two
            hallwayType = hallwayPrefabs[1];
            rotationAngle = 270.0f;
        }
        else
        {
            hallwayType = hallwayPrefabs[0];
        }

        hall = Instantiate(hallwayType, new Vector3(grid_position.x + 0.5f, 0, grid_position.y + 0.5f), Quaternion.identity);
        hall.transform.RotateAround(hall.transform.position, new Vector3(0,1,0), rotationAngle);

        hall.isStatic = true;
        hall.transform.parent = parentRoom.transform;

        
    }

    // Small function to help clean up scene when generating rooms
    void ClearRooms()
    {
        GameObject removeObject = GameObject.Find("_rooms");

        if (removeObject != null)
        {
            DestroyImmediate(removeObject);
        }

        parentRoom = new GameObject("_rooms");
        parentRoom.AddComponent<NavigationBaker>();

    }

    void DrawGraph()
    {
        foreach (Edge edge in mapEdges)
        {
            //Debug.Log(edge.GetHashCode());
            DrawLine(new Vector3(edge.U.x + 0.5f, 1, edge.U.y + 0.5f), new Vector3(edge.V.x + 0.5f, 1, edge.V.y + 0.5f), new Color(0.0f, 1.0f, 0.0f));
        }
    }

    // used to visual Delaunay Triangulization and Prim's MSTs
    void DrawLine(Vector3 start, Vector3 end, Color color, float duration = 0.2f)
    {
        GameObject myLine = new GameObject();
        myLine.transform.position = start;
        myLine.AddComponent<LineRenderer>();
        LineRenderer lr = myLine.GetComponent<LineRenderer>();
        //lr.SetColors(color, color);
        //lr.SetWidth(0.1f, 0.1f);
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        lr.transform.parent = parentLine.transform;

    }

    // Small function to clear lines when testing generation in editor
    // Not called at runtime
    void ClearLines()
    {
        GameObject removeObject = GameObject.Find("_lines");

        if (removeObject != null)
        {
            DestroyImmediate(removeObject);
        }

        parentLine = new GameObject("_lines");
    }

    void ClearAI()
    {
        GameObject removeObject = GameObject.FindWithTag("AI");

        if (removeObject != null)
        {
            DestroyImmediate(removeObject);
        }
    }

    public void ClearMap()
    {
        ClearRooms();
        ClearLines();
    }

    public void EnableTriangulation()
    {
        if(!triangulationOn)
        {
            DrawGraph();
            triangulationOn = true;
        }
        else
        {
            ClearLines();
            triangulationOn = false;
        }
    }

    // Function that resets all variables for each map generation
    void resetMap()
    {
        startingRoomAdded = false;
        endingRoomAdded = false;

        bossRoomIncluded = false;

        gridSize = gridDimensions;
        rooms = new List<Room>();
        spawnedRooms = new List<GameObject>();
        grid = new Grid<CellType>(gridSize, Vector2Int.zero);

        
        // Allows us to swap from random seeds to set seeds
        if(toggleSeedUsage)
        {
            Random.InitState((int)mapSeed + currentLevel);
        }
        else
        {
            Random.InitState((int) Random.Range(1, 100000));
        }
        
        // resets map per generation
        ClearRooms();
        ClearLines();
        ClearAI();
    }

    // Swaps scenes when the player wishes to exit the level
    void ChangeLevel(int levelNum)
    {
        currentLevel += levelNum;
        
        if(levelNum < 0)
            SceneManager.LoadScene(previousScene);
        else
        {
            SceneManager.LoadScene(nextScene);
        }
    }

    void EnemySpawnerMaker()
    {
        //Instantiate(EnemySpawner, Vector3.zero, Quaternion.identity);
        var waypointLists = GameObject.FindGameObjectsWithTag("Waypoint");
        var index = 0;
        var spawnIndex = 0;
        foreach (GameObject enemy in enemiesToSpawn)
        {
            
            for (int i = 0; i < enemyCount[index]; i++)
            {
                var newPosition = waypointLists[spawnIndex].transform.position;
                Instantiate(enemy, newPosition, Quaternion.identity);
                spawnIndex++;
            }

            index++;
        }

    }

}

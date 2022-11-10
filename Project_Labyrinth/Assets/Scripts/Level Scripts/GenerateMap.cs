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
using UnityEngine;

public class GenerateMap : MonoBehaviour
{
    // Cell type used to identify
    // if space is used for room, hallway, or empty space
    enum CellType{
        Empty,
        Room,
        Hallway
    }


    class Room
    {
        public RectInt bounds;

        

        public Room(Vector2 position, Vector2Int size)
        {
            bounds = new RectInt(new Vector2Int((int) position.x, (int) position.y), size);

        }

        // Used for intial generation of rooms
        public bool Colliding()
        {
            if(Physics.OverlapBoxNonAlloc(new Vector3(bounds.center.x, 1, bounds.center.y), new Vector3(bounds.size.x, 1, bounds.size.y), new Collider[1]) > 0)
            {
                return true;
            }

            return false;
        }
    };


    [SerializeField]
    public int numberOfRooms;

    [SerializeField]
    GameObject roomPrefab;

    [SerializeField]
    Vector2Int gridSize;

    [SerializeField]
    Vector2Int roomSizeRange;

    [SerializeField]
    float PrimEdgeCycleChance;

    [SerializeField]
    Material hallWayMat;

    [SerializeField]
    Material roomMat;

    Grid<CellType> grid;
    List<Room> rooms;
    Delaunay2D delaunay2D;

    GameObject parentRoom;
    GameObject parentLine;

    HashSet<Edge> mapEdges;

    bool triangulationOn = false;

    // Start is called before the first frame update
    void Start()
    {
        CreateMap();
    }

    public void CreateMap()
    {
        rooms = new List<Room>();

        grid = new Grid<CellType>(gridSize, Vector2Int.zero);

        Random.InitState((int)System.DateTime.Now.Ticks);

        // 
        ClearRooms();
        ClearLines();

        // Steps in creating the map
        SpawnRooms();
        DalaunayTriangulation();
        Prims();
        Pathfinding();
    }


    // Will need to change these to read room data
    // from a .Json
    void SpawnRooms()
    {
        int roomsSpawned = 0;

        while(roomsSpawned < numberOfRooms)
        {

            // Get Room positions
            Vector2Int roomPos = new Vector2Int(
                Random.Range(1, gridSize.x - 1),
                Random.Range(1, gridSize.y - 1));

            // Get Room Sizes
            Vector2Int roomSize = new Vector2Int(
                Random.Range(roomSizeRange.x, roomSizeRange.y),
                Random.Range(roomSizeRange.x, roomSizeRange.y));

            bool canAdd = true;

            Room tempRoom = new Room(roomPos, roomSize);
            Room bufferRoom = new Room(roomPos + new Vector2Int(-1, -1), roomSize + new Vector2Int(2, 2));

            // if Colliding with an existing room do not add
            if (bufferRoom.Colliding())
            {
                canAdd = false;
                //Debug.Log("Collision has Occured");
            }


            // If room exists outside of our grid do not add
            if (tempRoom.bounds.xMin < 0 || tempRoom.bounds.xMax >= gridSize.x
                || tempRoom.bounds.yMin < 0 || tempRoom.bounds.yMax >= gridSize.y)
            {
                canAdd = false;
                //Debug.Log("Room out of bounds");
            }

           // If conditions are met then add then add to collection of rooms
            if (canAdd)
            {
                rooms.Add(tempRoom);
                spawnRoom(tempRoom.bounds.center, tempRoom.bounds.size);

                roomsSpawned++;

                // Updates the grid with positions that are considered to be
                // inside of a room.
                foreach(var pos in tempRoom.bounds.allPositionsWithin)
                {
                    grid[pos] = CellType.Room;
                }
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
            coordinates.Add((Vector2) (room.bounds.center));
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

        foreach (var edge in mapEdges)
        {
            Room startRoom = new Room(edge.U, Vector2Int.one);
            Room endRoom = new Room(edge.V, Vector2Int.one);

            var startPosf = startRoom.bounds.center;
            var endPosf = endRoom.bounds.center;

            //Debug.Log("startPosf: " + startPosf);
            //Debug.Log("endPosf: " + endPosf);

            // Starting position of hall
            var startPos = new Vector2Int((int)startPosf.x, (int)startPosf.y);
            var endPos = new Vector2Int((int)endPosf.x, (int)endPosf.y);

            Debug.Log("startPos: " + startPos);
            Debug.Log("endPos: " + endPos);

            //Fine up to here

            var path = aStar.FindPath(startPos, endPos, (A_Star_Pathfinding.Node a, A_Star_Pathfinding.Node b) => {
                var pathCost = new A_Star_Pathfinding.PathCost();

                pathCost.cost = Vector2Int.Distance(b.Position, endPos);   

                
                if (grid[b.Position] == CellType.Hallway)
                {
                    pathCost.cost += 1;
                }
                else if (grid[b.Position] == CellType.Empty)
                {
                    pathCost.cost += 5;
                }
                else if (grid[b.Position] == CellType.Room)
                {
                    pathCost.cost += 10;
                }


                pathCost.traversable = true;

                return pathCost;
            });

            if (path != null)
            {
                for (int i = 0; i < path.Count; i++)
                {
                    var current = path[i];

                    if (grid[current] == CellType.Empty)
                    {
                        grid[current] = CellType.Hallway;
                    }

                    if (i > 0)
                    {
                        var prev = path[i - 1];

                        var delta = current - prev;
                    }
                }

                foreach (var pos in path)
                {
                    if (grid[pos] == CellType.Hallway)
                    {
                        spawnHallway(pos + new Vector2(0.5f,0.5f));
                    }
                }
            }
        }
    }

    // This will spawn each individual room
    // TODO: Will need to change to work with prefabs of rooms but that's work for later -- Nick
    void spawnRoom(Vector2 position, Vector2Int size)
    {
        GameObject room;
        
        room = Instantiate(roomPrefab, new Vector3(position.x, 0, position.y), Quaternion.identity);
        room.GetComponent<Transform>().localScale = new Vector3(size.x, 1, size.y);

        room.GetComponent<MeshRenderer>().material = roomMat;

        room.transform.parent = parentRoom.transform;
    }

    // Spawns the hallways between the rooms
    void spawnHallway(Vector2 position)
    {
        GameObject hall;

        hall = Instantiate(roomPrefab, new Vector3(position.x, 0, position.y), Quaternion.identity);
        hall.GetComponent<Transform>().localScale = new Vector3(1, 1, 1);

        hall.GetComponent<MeshRenderer>().material = hallWayMat;

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

    }

    void DrawGraph()
    {
        foreach (Edge edge in mapEdges)
        {
            //Debug.Log(edge.GetHashCode());
            DrawLine(new Vector3(edge.U.x, 1, edge.U.y), new Vector3(edge.V.x, 1, edge.V.y), new Color(0.0f, 1.0f, 0.0f));
        }
    }

    // used to visual Delaunay Triangulization and Prim's MSTs
    void DrawLine(Vector3 start, Vector3 end, Color color, float duration = 0.2f)
    {
        GameObject myLine = new GameObject();
        myLine.transform.position = start;
        myLine.AddComponent<LineRenderer>();
        LineRenderer lr = myLine.GetComponent<LineRenderer>();
        lr.SetColors(color, color);
        lr.SetWidth(0.1f, 0.1f);
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
}

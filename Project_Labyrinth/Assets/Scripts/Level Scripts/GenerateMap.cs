using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static Delaunay2D;

public class GenerateMap : MonoBehaviour
{
    enum CellType{
        Empty,
        Room,
        Hallway
    }

    class Room
    {
        public RectInt bounds;

        public Room(Vector2Int position, Vector2Int size)
        {
            bounds = new RectInt(position, size);
        }

        public bool Colliding()
        {
            /*return !((a.bounds.position.x >= (b.bounds.position.x + b.bounds.size.x)) || ((a.bounds.position.x + a.bounds.size.x) <= b.bounds.position.x)
                || (a.bounds.position.y >= (b.bounds.position.y + b.bounds.size.y)) || ((a.bounds.position.y + a.bounds.size.y) <= b.bounds.position.y));*/

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

    List<Room> rooms;
    Delaunay2D delaunay2D;

    GameObject parentRoom;
    GameObject parentLine;

    // Start is called before the first frame update
    void Start()
    {

    }

    public void CreateMap()
    {
        rooms = new List<Room>();

        Random.InitState((int)System.DateTime.Now.Ticks);

        ClearRooms();
        ClearLines();

        SpawnRooms();
        DalaunayTriangulation();
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
                Random.Range(0, gridSize.x),
                Random.Range(0, gridSize.y));

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
                Debug.Log("Collision has Occured");
            }


            // If room exists outside of our grid do not add
            if (tempRoom.bounds.min.x < 0 || tempRoom.bounds.max.x >= gridSize.x
                || tempRoom.bounds.min.y < 0 || tempRoom.bounds.min.y >= gridSize.y)
            {
                canAdd = false;
                Debug.Log("Room out of bounds");
            }

           // If conditions are met then add then add to collection of rooms
            if (canAdd)
            {
                rooms.Add(tempRoom);
                spawnRoom(tempRoom.bounds.position, tempRoom.bounds.size);

                roomsSpawned++;
            }
        }
    }

    void DalaunayTriangulation()
    {
        ClearLines();

        List<Vector2> coordinates = new List<Vector2>();

        foreach(Room room in rooms)
        {
            coordinates.Add((Vector2) (room.bounds.center));
        }

        delaunay2D = Delaunay2D.Triangulate(coordinates);

        foreach (Edge edge in delaunay2D.Edges)
        {
            Debug.Log(edge.GetHashCode());
            DrawLine(new Vector3(edge.U.x, 1, edge.U.y), new Vector3(edge.V.x, 1, edge.V.y), new Color(0.0f, 1.0f, 0.0f));
        }
    }

    void Prims()
    {

    }


    void Pathfinding()
    {

    }

    void spawnRoom(Vector2Int position, Vector2Int size)
    {
        GameObject room;
        
        room = Instantiate(roomPrefab, new Vector3(position.x, 0, position.y), Quaternion.identity);
        room.GetComponent<Transform>().localScale = new Vector3(size.x, 1, size.y);

        room.transform.parent = parentRoom.transform;
    }

    // Small function to help clean up scene when generating rooms
    public void ClearRooms()
    {
        GameObject removeObject = GameObject.Find("_rooms");

        if (removeObject != null)
        {
            DestroyImmediate(removeObject);
        }

        parentRoom = new GameObject("_rooms");
    }

    void DrawLine(Vector3 start, Vector3 end, Color color, float duration = 0.2f)
    {
        GameObject myLine = new GameObject();
        myLine.transform.position = start;
        myLine.AddComponent<LineRenderer>();
        LineRenderer lr = myLine.GetComponent<LineRenderer>();
        //lr.material = new Material();
        lr.SetColors(color, color);
        lr.SetWidth(0.1f, 0.1f);
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        lr.transform.parent = parentLine.transform;

        GameObject.Destroy(myLine, duration);
    }

    public void ClearLines()
    {
        GameObject removeObject = GameObject.Find("_lines");

        if (removeObject != null)
        {
            DestroyImmediate(removeObject);
        }

        parentLine = new GameObject("_lines");
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room
{
    public RectInt bounds;

    private string roomType;
    GameObject roomPrefab;
    Vector2Int position;

    // Default Constructor
    public Room()
    {
        roomType = "NULL";
        roomPrefab = null;
        position = Vector2Int.zero;
    }

    // Used for pathfinding rooms
    public Room(Vector2 pos, Vector2Int size)
    {
        bounds = new RectInt(new Vector2Int((int) pos.x, (int) pos.y), size);
    }

    // Constructor takes in room prefab and a position to be placed
    public Room(GameObject room, Vector2Int pos)
    {
        SetRoom(room);
        SetPosition(pos);
    }


    // Sets the room type and changes it into the specified game object
    // allows us to spawn rooms of different types
    public void SetRoom(GameObject room)
    {
        RoomInfo roominfo = room.GetComponent<Room_Controller>().roominfo;

        roomPrefab = room;
        roomType = roominfo.name;

        bounds = new RectInt(Vector2Int.zero, roominfo.dimensions);

    }

    public void SetPosition(Vector2Int pos)
    {
        bounds.position = pos;
    }

    // Returns a clone which is used to check for collisions 
    public Room CloneBuffer(Vector2Int offsetPos, Vector2Int offsetSize)
    {
        Room clone = new Room(this.bounds.position + offsetPos, this.bounds.size + offsetSize);

        return clone;
    }


    // Used for intial generation of rooms
    public bool Colliding()
    {
        if (Physics.OverlapBoxNonAlloc(new Vector3(bounds.center.x, 1, bounds.center.y), new Vector3(bounds.size.x, 1, bounds.size.y), new Collider[1]) > 0)
        {
            return true;
        }

        return false;
    }

    public GameObject getRoomPrefab()
    {
        return roomPrefab;
    }
}

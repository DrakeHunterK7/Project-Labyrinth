using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room
{
    public RectInt bounds;

    private string roomType;
    GameObject roomPrefab;
    public Vector2 position;
    List<Vector2Int> entracePositions; // positions of the entrance(s) for a room
    List<bool> entranceChosen;

    // Default Constructor
    public Room()
    {
        roomType = "NULL";
        roomPrefab = null;
        position = Vector2Int.zero;
        entracePositions = null;
    }

    // Used for pathfinding rooms
    public Room(Vector2 pos, Vector2Int size)
    {
        bounds = new RectInt(new Vector2Int((int) pos.x, (int) pos.y), size);
    }

    // Constructor takes in room prefab and a position to be placed
    public Room(GameObject room, Vector2Int pos)
    {
        entracePositions = new List<Vector2Int>();
        entranceChosen = new List<bool>();

        SetRoom(room);
        SetPosition(pos);
        SetEntrances(room, pos);

        position = bounds.center;
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

    // Adds the worls space position of an entrace 
    public void SetEntrances(GameObject room, Vector2Int roomPos)
    {
        RoomInfo roominfo = room.GetComponent<Room_Controller>().roominfo;

        // adds position of entrace to world space
        foreach(Vector2Int position in roominfo.relativeEntrancePos)
        {
            entracePositions.Add(position + roomPos);
            entranceChosen.Add(false);
        }
        
    }

    public List<Vector2Int> GetAllEntrances()
    {
        return entracePositions;
    }

    // Returns an entrance position for triangulation
    public Vector2Int GetEntracePositions()
    {
        // makes an attempt to fill in each entrace with a hallway
        for(int i = 0; i < entracePositions.Count; i++)
        {
            if(entranceChosen[i] == false)
            {
                entranceChosen[i] = true;
                return entracePositions[i];
            }
        }

        // will return a random entrace if all have at least been used once
        return entracePositions[Random.Range(0, entracePositions.Count)];
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

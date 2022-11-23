using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Room", menuName = "Room/Create New Room")]

public class RoomInfo : ScriptableObject
{
    public Vector2Int dimensions;
    public string roomName;
    public List<Vector2Int> relativeEntrancePos = new List<Vector2Int>();
}
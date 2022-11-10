using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomState : MonoBehaviour
{

    bool roomColliding;

    private void OnCollisionEnter(Collision collision)
    {
        roomColliding = true;
        Debug.Log("Collision Occured");
    }

    private void OnCollisionExit(Collision collision)
    {
        roomColliding = false;
    }

    public bool GetRoomState()
    {
        return roomColliding;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room_Controller : MonoBehaviour
{
    [SerializeField]
    public RoomInfo roominfo;

    [SerializeField]
    public List<GameObject> itemSpawners;

    public void SpawnItems()
    {
        // SHOULD ADD A CHANCE THAT THERE WILL BE NO ITEMS IN ROOMS
        // THIS IS SO THE PLAYER ISN'T GIVEN TOO MANY ITEMS DURING THE 
        // GAME.

        // Chooses which of the spawner prefabs will spawn their item
        // in each room.
        if (itemSpawners.Count > 0)
        {
            int chosenIndex = Random.Range(0, itemSpawners.Count + 1);

            // Allows for no items to spawn in a room
            if(chosenIndex != itemSpawners.Count)
            {
                itemSpawners[chosenIndex].GetComponent<ItemSpawner>().SpawnItem();
            }
        }


    }
}

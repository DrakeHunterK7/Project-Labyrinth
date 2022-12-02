using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    ItemManager itemManager;

    public void Awake()
    {
        itemManager = Object.FindObjectsOfType<ItemManager>()[0]; // Fine to do since there will only be one item manager
    }


    public void SpawnItem()
    {
        GameObject itemToSpawn = itemManager.GetItemToSpawn();
        GameObject item;

        if(itemToSpawn != null)
        {
            item = Instantiate(itemToSpawn, transform.position, Quaternion.identity);
        }
        
    }
}

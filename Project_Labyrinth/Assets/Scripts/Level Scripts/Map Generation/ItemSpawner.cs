using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [SerializeField]
    GameObject itemToSpawn;

    public void SpawnItem()
    {
        GameObject item;

        item = Instantiate(itemToSpawn, transform.position, Quaternion.identity);
    }
}

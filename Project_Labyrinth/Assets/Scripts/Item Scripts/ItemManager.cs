using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    

    [SerializeField]
    List<GameObject> itemsToSpawn;

    [SerializeField]
    List<int> numberOfEachItem;

    int[] currentItemCount;

    int chosenIndex;
    int numTries;

    private void Awake()
    {
        currentItemCount = new int[itemsToSpawn.Count];
    }

    public GameObject GetItemToSpawn()
    {
        numTries = 0;

        while(numTries < 3)
        {
            chosenIndex = Random.Range(0, itemsToSpawn.Count);

            if(currentItemCount[chosenIndex] < numberOfEachItem[chosenIndex])
            {
                currentItemCount[chosenIndex]++;
                return itemsToSpawn[chosenIndex];
            }
            numTries++;
        }

        return null;
    }
}

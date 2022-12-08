using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    

    [SerializeField]
    List<GameObject> itemsToSpawn;

    [SerializeField]
    GameObject keyPrefab;

    [SerializeField]
    List<int> numberOfEachItem;

    int[] currentItemCount;

    int chosenIndex;
    int numTries;
    bool keyPlaced;

    private void Awake()
    {
        currentItemCount = new int[itemsToSpawn.Count];
    }

    public GameObject GetItemToSpawn()
    {
        numTries = 0;

        if(!keyPlaced)
        {
            keyPlaced =true;
            return keyPrefab;
        }
        else
        {
            while (numTries < 3)
            {
                chosenIndex = Random.Range(0, itemsToSpawn.Count);

                if (currentItemCount[chosenIndex] < numberOfEachItem[chosenIndex])
                {
                    currentItemCount[chosenIndex]++;
                    return itemsToSpawn[chosenIndex];
                }
                numTries++;
            }
        }
        
        return null;
    }
}

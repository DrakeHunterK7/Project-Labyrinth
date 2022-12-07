using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject Minion;
    public GameObject Mutant;
    public List<GameObject> spawnerList = new List<GameObject> ();
    private int index = 0;
    public int enemyCap;
    private int temp;
    // Start is called before the first frame update
    void Start()
    {
        foreach (GameObject sp in GameObject.FindGameObjectsWithTag("EnemySpawner"))
        {
           spawnerList.Add(sp);
        }
        temp = enemyCap;
        Invoke("SpawnMutant", 2f);

    }
    

    // Update is called once per frame
    void Update()
    {
      
    }

    private void randomizeindex()
    {
        index = Random.Range(0, spawnerList.Count - 1);
    }

    private void SpawnMutant()
    {

        for (int i = 0; i < enemyCap; i++)
        {
            randomizeindex();
            Instantiate(Mutant, spawnerList[index].transform.position, Quaternion.identity);
            
        }

    }
}

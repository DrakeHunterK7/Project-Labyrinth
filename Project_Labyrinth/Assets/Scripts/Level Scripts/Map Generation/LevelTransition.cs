using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelTransition : MonoBehaviour
{
    [SerializeField]
    bool continueToNextLevel;

    [SerializeField]
    bool returnToPreviousLevel;

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag.Equals("Player"))
        {

            if(continueToNextLevel)
            {
                GenerateMap.trigger?.Invoke(1);
            }
            else if(returnToPreviousLevel)
            {
                GenerateMap.trigger?.Invoke(-1);
            }
            
        }
    }
}

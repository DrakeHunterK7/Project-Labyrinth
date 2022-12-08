using System;
using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    [SerializeField] private Item unlockKey;
    
    public bool Check(String key)
    {
        return key == unlockKey.itemName;
    }

    public abstract void Unlock();
    public abstract void DoAction();

}

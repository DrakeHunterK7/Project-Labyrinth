using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class MessageText : MonoBehaviour
{
    [SerializeField] private float activeTime = 5f;
    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating("Disappear", activeTime, 0f);
    }

    void Disappear()
    {
        Destroy(this.gameObject);
    }
}

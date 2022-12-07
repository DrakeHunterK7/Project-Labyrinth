using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMap : MonoBehaviour
{

    Transform player;

    public void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
    }

    private void LateUpdate()
    {
        if (player == null) return;
        Vector3 newPos = player.position;
        newPos.y = transform.position.y;

        transform.position = newPos;
    }
}

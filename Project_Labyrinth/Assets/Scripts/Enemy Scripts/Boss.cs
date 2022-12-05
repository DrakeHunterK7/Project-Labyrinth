using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Boss : MonoBehaviour
{
    public float bossdamage;
    private GameObject player;
    private NavMeshAgent agent;
    private bool coroutine = false;
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        agent = transform.GetComponent<NavMeshAgent>();

    }

    // Update is called once per frame
    void Update()
    {
        if (player.GetComponent<PlayerMovement>().smelly)
        {
            agent.SetDestination(player.transform.position);
        }
        if (Vector3.Distance(player.transform.position, this.transform.position) < 10f)
        {

            Destroy(player);
        }


    }

    public IEnumerator damageplayer()
    {
        coroutine = true;
        yield return new WaitForSeconds(1f);
        player.GetComponent<PlayerMovement>().Damage(bossdamage);
    }
}

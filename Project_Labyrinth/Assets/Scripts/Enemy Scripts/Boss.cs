using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class Boss : MonoBehaviour
{
    public float bossdamage;
    private GameObject player;
    private NavMeshAgent agent;
    private bool heard = false;
    public Vector3 soundposition = Vector3.zero;
    public Vector3 patrolLocation = Vector3.zero;

    private float checkingTime = 7f;
    private float patrolStopTime = 7f;
    
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

    private void OnDrawGizmos()
    {
        if (player == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 25f);

    }


    public IEnumerator damageplayer()
    {
        
        yield return new WaitForSeconds(1f);
        player.GetComponent<PlayerMovement>().Damage(bossdamage);
    }
    
}

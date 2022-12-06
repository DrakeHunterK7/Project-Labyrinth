using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class Boss : MonoBehaviour, IHearing
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
            patrolStopTime = 7f;
            checkingTime = 7f;
            agent.SetDestination(player.transform.position);
        }
        else if (heard)
        {
            agent.SetDestination(soundposition);
            Debug.Log("Sound Heard");
            if (Vector3.Distance(soundposition, this.transform.position) < 2f)
            {
                Debug.Log("At soundpos");
                checkingTime -= Time.deltaTime;

                if (checkingTime <= 0f)
                {
                    heard = false;
                    checkingTime = 7f;
                }
            }
        }
        else
        {
            if (agent.remainingDistance < 5f)
            {
                patrolStopTime -= Time.deltaTime;

                if (patrolStopTime <= 0f)
                {
                    patrolStopTime = 7f;
                    patrolLocation = GameObject.FindWithTag("Waypoint").transform.position;
                }
                
            }

            agent.SetDestination(patrolLocation);
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

    public void HeardSound(Vector3 soundPosition)
    {
        heard = true;
        soundposition = soundPosition;
    }

    public IEnumerator damageplayer()
    {
        
        yield return new WaitForSeconds(1f);
        player.GetComponent<PlayerMovement>().Damage(bossdamage);
    }
    
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyFov : MonoBehaviour
{
    public Transform player;
    private int index = 0;
    public float maxangle;
    private float maxradius = 75f;
    private bool isinFOV = false;
    private NavMeshAgent agent;
    public Animator animator;
    public List<GameObject> waypointlist = new List<GameObject>();
    private bool attack = false;
    private float seeTime = 7f;
    private bool sawPlayer = false;

    // Start is called before the first frame update
    void Start()
    {
        agent = transform.GetComponent<NavMeshAgent>();
        player = GameObject.FindWithTag("Player").transform;
        foreach(GameObject wp in GameObject.FindGameObjectsWithTag("Waypoint"))
        {
            waypointlist.Add(wp);
        }
        randomizeindex();
    }

    // Update is called once per frame
    void Update()
    {
        isinFOV = inFOV(transform, player, maxangle, maxradius);
        animator.SetFloat("speed", agent.speed);
        animator.SetBool("attack", attack);


        if(isinFOV)
        {
            seeTime = 7f;
            sawPlayer = true;
            agent.speed = 25;
            agent.SetDestination(player.position);
            if (Vector3.Distance(player.position, this.transform.position) < 10f)
            {
                attack = true;
            }
            else
            {
                attack = false;
            }
                
        }
        else
        {
            attack = false;
            if (sawPlayer)
                seeTime -= Time.deltaTime;

            if (seeTime <= 0)
            {
                sawPlayer = false;
                agent.speed = 10;
                agent.SetDestination(waypointlist[index].transform.position);
            }
            
            if (index > waypointlist.Count - 1)
            {
                index = 0;
            }
            trackDistance();
            Debug.Log(waypointlist[index].transform.position);
        }
        
    }

    private void OnDrawGizmos()
    {
        if (player == null) return;
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, maxradius);

        Vector3 fovLine1 = Quaternion.AngleAxis(maxangle, transform.up) * transform.forward * maxradius;
        Vector3 fovLine2 = Quaternion.AngleAxis(-maxangle, transform.up) * transform.forward * maxradius;

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, fovLine1);
        Gizmos.DrawRay(transform.position, fovLine2);

        if (!sawPlayer)
            Gizmos.color = Color.red;
        else
            Gizmos.color = Color.green;

        Gizmos.DrawRay(transform.position, (player.position - transform.position).normalized * maxradius);

        Gizmos.color = Color.black;
        Gizmos.DrawRay(transform.position, transform.forward * maxradius);
    }


    public static bool inFOV(Transform checkingObject, Transform target, float maxangle, float maxradius)
    {
        Collider[] overlaps = new Collider[100];
        Physics.OverlapSphereNonAlloc(checkingObject.position, maxradius, overlaps);

        foreach (Collider overlap in overlaps)
        {
            if (overlap != null)
            {
                if (overlap.transform == target)
                {
                    Vector3 directionbetween = (target.position - checkingObject.position).normalized;
                    directionbetween.y *= 0;

                    float angle = Vector3.Angle(checkingObject.forward, directionbetween);
                    if (angle <= maxangle)
                    {
                        Ray ray = new Ray(checkingObject.position, target.position - checkingObject.position);
                        RaycastHit hit;

                        if (Physics.Raycast(ray,out hit, maxradius))
                        {
                            if (hit.collider.gameObject.CompareTag("Player"))
                            {
                                return true;
                            }
                                
                        }
                    }
                }
            }
        }



        return false;
    }

    private void trackDistance()
    {
        if(Vector3.Magnitude(waypointlist[index].transform.position - transform.position) < 2f)
        {
            //agent.speed = 0;
            StartCoroutine("randomizeindex1");
        }

        
    }

    private void randomizeindex()
    {
        index = Random.Range(0, waypointlist.Count - 1);
    }

    private IEnumerator randomizeindex1()
    {
        yield return new WaitForSeconds(5f);
        if(agent.speed == 0)
        {
            agent.speed = 10;
        }
        index = Random.Range(0, waypointlist.Count - 1);
    }

}

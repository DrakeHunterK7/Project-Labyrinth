using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.AI;

public class EnemyFov : MonoBehaviour, IHearing
{
    public Transform player;
    private int index = 0;
    public float maxangle;
    private float maxradius = 75f;
    private bool isinFOV = false;
    private NavMeshAgent agent;
    public Animator animator;
    private bool attack = false;
    private float seeTime = 7f;
    private bool sawPlayer = false;
    private bool heard = false;
    private Vector3 soundposition = Vector3.zero;
    private float checkingTime = 7f;
    private float patrolStopTime = 7f;
    public Vector3 patrolLocation;
    public List<GameObject> waypointlist = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        agent = transform.GetComponent<NavMeshAgent>();
        player = GameObject.FindWithTag("Player").transform;
        foreach (GameObject wp in GameObject.FindGameObjectsWithTag("Waypoint"))
        {
            waypointlist.Add(wp);
        }

        randomizeindex();

        patrolLocation = waypointlist[index].transform.position;
        //randomizeindex();
    }
    

    // Update is called once per frame
    void Update()
    {
        if (GameDirector.instance.isPlayerDead) return;
        
        isinFOV = inFOV(transform, player, maxangle, maxradius);
        animator.SetFloat("speed", agent.velocity.magnitude);
        animator.SetBool("attack", attack);


        if(isinFOV)
        {
            heard = false;
            patrolStopTime = 7f;
            checkingTime = 7f;
            seeTime = 7f;
            sawPlayer = true;
            agent.speed = 20;
            agent.SetDestination(player.position);
            if (Vector3.Distance(player.position, this.transform.position) < 7f)
            {
                attack = true;
            }
            else
            {
                attack = false;
            }
                
        }
        else if (heard)
        {
            agent.speed = 20;
            agent.SetDestination(soundposition);
            
            if (Vector3.Distance(soundposition, this.transform.position) < 10f)
            {
                Debug.Log("At soundpos");
                checkingTime -= Time.deltaTime;

                if (checkingTime <= 0f)
                {
                    randomizeindex();
                    patrolLocation = waypointlist[index].transform.position;
                    agent.speed = 10;
                    heard = false;
                    checkingTime = 7f;
                }
            }
        }
        else
        {
            attack = false;
            if (sawPlayer)
            {
                seeTime -= Time.deltaTime;

                if (seeTime <= 0)
                {
                    sawPlayer = false;
                    agent.speed = 10;
                
                }
                else
                {
                    agent.SetDestination(player.position);
                }
            }
            else
            {
                if (Vector3.Distance(patrolLocation, this.transform.position) < 10f)
                {
                    patrolStopTime -= Time.deltaTime;

                    if (patrolStopTime <= 0f)
                    {
                        patrolStopTime = 7f;
                        randomizeindex();
                        patrolLocation = waypointlist[index].transform.position;
                    }
                }
                else
                {
                    agent.SetDestination(patrolLocation);
                }
            }
        }
    }

    public void HeardSound(Vector3 soundPosition)
    {
        if (sawPlayer) return;
        if(!heard)
            MessageManager.instance.DisplayMessage("It heard you!", Color.red);
        soundposition = soundPosition;
        heard = true;
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

        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position, (patrolLocation - transform.position));

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
                        else
                        {
                            if (Vector3.Distance(checkingObject.position, target.position) < 20f)
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

    private void randomizeindex()
    {
        index = Random.Range(0, waypointlist.Count - 1);
    }

    void DamagePlayer()
    {
        var player = GameObject.FindWithTag("Player");
        if (Vector3.Distance(player.transform.position, transform.position) < 10f)
        {
            if (player.GetComponent<MonoBehaviour>() is IDamageable script)
            {
                script.Damage(10f);
            }
        }
    }

}

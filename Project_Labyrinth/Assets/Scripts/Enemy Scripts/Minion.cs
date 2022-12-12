using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Minion : MonoBehaviour, IHearing
{
    public Transform player;
    private int index = 0;
    public float maxangle;
    private float maxradius = 75f;
    private bool isinFOV;
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
    private bool done = false;
    public List<GameObject> waypointlist = new List<GameObject>();


    // Start is called before the first frame update
    void Start()
    {
        agent = transform.GetComponent<NavMeshAgent>();
        player = GameObject.FindWithTag("Player").transform;
       
        foreach (GameObject wp in GameObject.FindGameObjectsWithTag("Waypoint"))
        {
            waypointlist.Add(wp);
        }


        randomizeindex();

        patrolLocation = waypointlist[index].transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        isinFOV = inFOV(transform, player, maxangle, maxradius);
        animator.SetFloat("speed", agent.velocity.magnitude);



        if (isinFOV)
        {
            patrolStopTime = 7f;
            checkingTime = 7f;
            seeTime = 7f;
            sawPlayer = true;
            agent.speed = 25;
            agent.SetDestination(player.position);
            if (Vector3.Distance(player.position, this.transform.position) < 10f && !done)
            {
                done = true;
                animator.Play("pounce");
                var player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>();
                player.smelly = true;
                player.statusText.text = "MARKED";
                MessageManager.instance.DisplayMessage("It's coming. Run.", Color.red);
                MessageManager.instance.DisplayMessage("You have been marked.", Color.red);
                StartCoroutine("destroyminion");
            }

        }
        else if (heard)
        {
            agent.speed = 25;
            agent.SetDestination(soundposition);

            if (Vector3.Distance(soundposition, this.transform.position) < 2f)
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
                    Debug.Log("Patrolling");
                }
            }
        }
    }

    public void HeardSound(Vector3 soundPosition)
    {
        if (heard || sawPlayer) return;
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

                        if (Physics.Raycast(ray, out hit, maxradius))
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

    private void randomizeindex()
    {
        index = Random.Range(0, waypointlist.Count - 1);
    }

    public IEnumerator destroyminion()
    {
        yield return new WaitForSeconds(1.25f);
        foreach (Minion minion in GameObject.FindObjectsOfType<Minion>())
        {
            Destroy(minion.gameObject);
        }
        Destroy(this.gameObject);
    }
}




using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

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
    private float seeTime = 3f;
    private bool sawPlayer = false;
    private bool heard = false;
    private Vector3 soundposition = Vector3.zero;
    private float checkingTime = 7f;
    private float patrolStopTime = 7f;
    public Vector3 patrolLocation;
    public AudioSource detectionSound;

    private GameObject[] waypointList;

    [Header("Mutant Parameters")]
    [SerializeField] public AudioSource[] mutantAttackSounds;
    [SerializeField] public AudioSource[] mutantIdleSounds;
    [SerializeField] public float idleSoundTime;
    private float nextIdleSound = 0f;
    private bool attackSoundPlaying = false;

    private float chaseTimeOut = 0f;
    private bool addedToChaseList = false;
    private NavMeshPath agentPath;
    

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        agent = transform.GetComponent<NavMeshAgent>();
        player = GameObject.FindWithTag("Player").transform;
        waypointList = GameObject.FindGameObjectsWithTag("Waypoint");
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
            chaseTimeOut = 10f;
            heard = false;
            patrolStopTime = 7f;
            checkingTime = 7f;
            seeTime = 3f;
            sawPlayer = true;
            
            if (!addedToChaseList)
            {
                GameDirector.instance.AddChaser();
                addedToChaseList = true;
            }
            
            agent.speed = 15;
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
            detectionSound.Play();
            agent.speed = 20;
            agent.SetDestination(soundposition);
            
            if (Vector3.Distance(soundposition, this.transform.position) < 10f)
            {
                checkingTime -= Time.deltaTime;

                if (checkingTime <= 0f)
                {
                    patrolLocation = waypointList[getRandomWaypointIndex()].transform.position;
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
                    if (addedToChaseList)
                    {
                        GameDirector.instance.RemoveChaser();
                        addedToChaseList = false;
                    }
                
                }
                else
                {
                    agent.SetDestination(player.position);
                }
            }
            else
            {
                if (chaseTimeOut > 0)
                    chaseTimeOut -= Time.deltaTime;
                if (Vector3.Distance(patrolLocation, this.transform.position) < 10f || !agent.CalculatePath(patrolLocation, new NavMeshPath()))
                {
                    patrolStopTime -= Time.deltaTime;

                    if (patrolStopTime <= 0f)
                    {
                        patrolStopTime = 7f;
                        patrolLocation = waypointList[getRandomWaypointIndex()].transform.position;
                    }
                }
                else
                {
                    agent.SetDestination(patrolLocation);
                }
            }
        }

        if (Random.Range(0, 10) > 5 && nextIdleSound < Time.time && attack == false)
        {
            mutantIdleSounds[Random.Range(0, mutantIdleSounds.Length)].Play();
            nextIdleSound = idleSoundTime + Time.time;
        }
    }

    public void HeardSound(Vector3 soundPosition)
    {
        if (sawPlayer || chaseTimeOut > 0) return;
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

        Gizmos.DrawRay(transform.position+Vector3.up*agent.height/2, ((player.position+Vector3.up*agent.height/2) - (transform.position+Vector3.up*agent.height/2)).normalized * maxradius);

        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position, (patrolLocation - transform.position));

        Gizmos.color = Color.black;
        Gizmos.DrawRay(transform.position, transform.forward * maxradius);
    }


    public bool inFOV(Transform checkingObject, Transform target, float maxangle, float maxradius)
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
                        Debug.Log("3");
                        Ray ray = new Ray(checkingObject.position+Vector3.up*agent.height/2, (target.position+Vector3.up*agent.height/2) - (checkingObject.position+Vector3.up*agent.height/2));
                        RaycastHit hit;

                        if (Physics.Raycast(ray,out hit, maxradius))
                        {
                            Debug.Log("4");
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
    void DamagePlayer()
    {
        var player = GameObject.FindWithTag("Player");
        if (Vector3.Distance(player.transform.position, transform.position) < 8f)
        {
            if (player.GetComponent<MonoBehaviour>() is IDamageable script)
            {
                script.Damage(10f);
            }
        }
    }
    
    private int getRandomWaypointIndex()
    {
        return Random.Range(0, waypointList.Length);
    }
}

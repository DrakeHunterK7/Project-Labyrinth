using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyFov : MonoBehaviour
{
    public Transform player;
    public float maxangle;
    public float maxradius;
    private bool isinFOV = false;
    private NavMeshAgent agent;


    // Start is called before the first frame update
    void Start()
    {
        agent = transform.GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        isinFOV = inFOV(transform, player, maxangle, maxradius);

        if (isinFOV)
        {
            agent.SetDestination(player.position);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, maxradius);

        Vector3 fovLine1 = Quaternion.AngleAxis(maxangle, transform.up) * transform.forward * maxradius;
        Vector3 fovLine2 = Quaternion.AngleAxis(-maxangle, transform.up) * transform.forward * maxradius;

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, fovLine1);
        Gizmos.DrawRay(transform.position, fovLine2);

        if (!isinFOV)
            Gizmos.color = Color.red;
        else
            Gizmos.color = Color.green;

        Gizmos.DrawRay(transform.position, (player.position - transform.position).normalized * maxradius);

        Gizmos.color = Color.black;
        Gizmos.DrawRay(transform.position, transform.forward * maxradius);
    }


    public static bool inFOV(Transform checkingObject, Transform target, float maxangle, float maxradius)
    {
        Collider[] overlaps = new Collider[10];
        int count = Physics.OverlapSphereNonAlloc(checkingObject.position, maxradius, overlaps);

        for (int i = 0; i < count + 1; i++)
        {
            if (overlaps[i] != null)
            {
                if (overlaps[i].transform == target)
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
                            if (hit.transform == target)
                                return true;
                        }
                    }
                }
            }
        }



        return false;
    }

}

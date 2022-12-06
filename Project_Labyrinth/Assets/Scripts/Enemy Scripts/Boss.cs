using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Boss : MonoBehaviour
{
    public float bossdamage;
    private GameObject player;
    private NavMeshAgent agent;
    private bool heard = false;
    public Vector3 soundposition = Vector3.zero;
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
        if (heardsound())
        {
            agent.SetDestination(soundposition);
            Debug.Log("Sound Heard");
            if (Vector3.Distance(soundposition, this.transform.position) < 2f)
            {

                heard = false;
                Debug.Log("At soundpos");
            }
        }
       



    }

    public IEnumerator damageplayer()
    {
        
        yield return new WaitForSeconds(1f);
        player.GetComponent<PlayerMovement>().Damage(bossdamage);
    }

    public bool heardsound()

    {
        heard = true;
        return heard;
    }
}

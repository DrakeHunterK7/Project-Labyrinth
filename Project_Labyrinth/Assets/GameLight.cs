using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLight : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField] private Light light1;
    [SerializeField] private Material off;
    [SerializeField] private Material on;
    [SerializeField] private MeshRenderer lightModel;
    void Start()
    {
        var x = Random.Range(1, 11);
        if (x < 2)
        {
            
        }
        else if (x < 4)
        {
            InvokeRepeating("FlickerLights", 0f, 0.1f);
        }
        else
        {
            light1.enabled = false;
            lightModel.material = off;
        }
    }
    
    void FlickerLights()
    {
        if (Vector3.Distance(GameObject.FindWithTag("Player").transform.position, transform.position) > 300f) return;
        
        if (Random.Range(1, 10f) < 2f)
        {
            if (light1.enabled)
            {
                light1.enabled = false;
                lightModel.material = off;
            }
            else
            {
                light1.enabled = true;
                lightModel.material = on;
            }
        }
    }
}

using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Inventory_System.Item_Scripts
{
    public class ThrowBottle : MonoBehaviour, IEquippableItemAction
    {
        [SerializeField] private float throwForce = 25f;
        private float soundradius = 1000f;
        public GameObject bottle;

            
        public void Use(PlayerMovement player)
        {
            Debug.Log("Threw the Bottle!");
            
            
            
            transform.SetParent(null);

            var rb = transform.GetComponent<Rigidbody>();
        
            rb.useGravity = true;
        
            rb.AddForce(player.mainCam.transform.forward.normalized*throwForce, ForceMode.Impulse);
            StartCoroutine("wait");
            
        
        }

        public void alertenemies()
        {
            Collider[] colliders = Physics.OverlapSphere(bottle.transform.position, soundradius);
            foreach(var collider in colliders)
            {
                if (collider.gameObject.tag=="Enemy"){
                    collider.GetComponent<Boss>().soundposition = bottle.transform.position;
                    collider.GetComponent<Boss>().heardsound();



                }
            }

        }

       public IEnumerator wait()
        {
            yield return new WaitForSeconds(5f);
                alertenemies();

        }
   
    }
}

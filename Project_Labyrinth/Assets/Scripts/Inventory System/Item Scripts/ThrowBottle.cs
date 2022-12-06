using System;
using System.Collections;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

namespace Inventory_System.Item_Scripts
{
    public class ThrowBottle : MonoBehaviour, IEquippableItemAction
    {
        [SerializeField] private float throwForce = 50f;
        private float soundradius = 1000f;
        public GameObject bottle;
        private Rigidbody rb;
        private bool isThrown = false;

        void Start()
        {
            rb = GetComponent<Rigidbody>();
        }
        
        public void Use(PlayerMovement player)
        {
            Debug.Log("Threw the Bottle!");

            transform.SetParent(null);
            
            rb.useGravity = true;
            rb.AddForce(player.mainCam.transform.forward.normalized*throwForce, ForceMode.Impulse);
            isThrown = true;

        }

        private void OnCollisionEnter(Collision collision)
        {
            if (rb == null) return;
            
            if (rb.velocity.magnitude > 5f && isThrown)
            {
                alertenemies();
                Destroy(this.gameObject);
            }
        }

        public void alertenemies()
        {
            Collider[] colliders = Physics.OverlapSphere(bottle.transform.position, soundradius);
            foreach(var collider in colliders)
            {
                if (collider.gameObject.CompareTag("Enemy"))
                {
                    foreach (MonoBehaviour script in collider.gameObject.GetComponents<MonoBehaviour>())
                    {
                        if (script is IHearing targetScript)
                        {
                            targetScript.HeardSound(bottle.transform.position);
                        }
                    }
                }
            }
        }
    }
}

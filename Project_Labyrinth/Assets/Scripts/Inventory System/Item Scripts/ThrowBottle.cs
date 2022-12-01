using System.Runtime.CompilerServices;
using UnityEngine;

namespace Inventory_System.Item_Scripts
{
    public class ThrowBottle : MonoBehaviour, IEquippableItemAction
    {
        [SerializeField] private float throwForce = 25f;
            
        public void Use(PlayerMovement player)
        {
            Debug.Log("Threw the Bottle!");
            
            
            
            transform.SetParent(null);

            var rb = transform.GetComponent<Rigidbody>();
        
            rb.useGravity = true;
        
            rb.AddForce(player.mainCam.transform.forward.normalized*throwForce, ForceMode.Impulse);
            
        
        }
   
    }
}

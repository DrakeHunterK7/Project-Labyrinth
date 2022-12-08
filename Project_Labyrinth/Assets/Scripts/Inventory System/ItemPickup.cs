
using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public Item Item;
    private int i;

    public static bool refeshAdd = false;

    public void Pickup()
    {
        //Debug.Log(GameObject.FindGameObjectWithTag("Inventory").activeSelf);
        bool active = false;
        if(GameObject.FindGameObjectWithTag("Inventory") != null)
        {
            active = GameObject.FindGameObjectWithTag("Inventory").activeInHierarchy; 
        }
        if(!active){
            InventoryManager.instance.Add(Item);
            Destroy(gameObject);
        }
    }
    
    

    public void PickupItem(){
        Pickup();   
        refeshAdd = true;
    }
}

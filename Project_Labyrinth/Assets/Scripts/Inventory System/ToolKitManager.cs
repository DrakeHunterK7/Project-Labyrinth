using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToolKitManager : MonoBehaviour
{
    public static InventoryManager instance;
    public List<Item> Items = new List<Item>();
    //private List<Item> OldItems = new List<Item>();

    public Transform ToolKitContent;
    public Transform ItemContent;
    public GameObject InventoryItem;

    public InventoryItemController[] InventoryItems;

    public int capacity = 9;
    private int itemCount = 0;

    void Start()
    {
        ToolKitContent = GameObject.FindWithTag("Toolkit").GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log("Deletng" + InventoryItemController.refreshDelete);
        //Debug.Log("Addng" + ItemPickup.refeshAdd);
        //Update the content of the items list whenever some items got addedd or deleted:
        Items = InventoryManager.instance.Items;
        if(InventoryItemController.refreshDelete || ItemPickup.refeshAdd)
        {
            //First clear all existed items
            foreach (Transform child in ToolKitContent.transform) {
                GameObject.Destroy(child.gameObject);
            }
            foreach (var item in Items)
            {
                // When the number of items in the toolkit is less than its capacity
                // we can keep adding new items
                if(itemCount < capacity)
                {
                    GameObject obj = Instantiate(InventoryItem, ToolKitContent);
                    var itemName = obj.transform.Find("ItemName").GetComponent<Text>();
                    var itemIcon = obj.transform.Find("ItemIcon").GetComponent<Image>();
                    var removeButton = obj.transform.Find("RemoveItem").GetComponent<Button>();

                    itemName.text = item.itemName;
                    itemIcon.sprite = item.icon;
                    itemCount ++;
                }
            }
            //After refreshing, set the bool value back to false:
            //if(InventoryItemController.refreshDelete)
                ItemPickup.refeshAdd = false;
            //else if(ItemPickup.refeshAdd)
                InventoryItemController.refreshDelete = false;
                itemCount = 0;
        }
       
    }

}

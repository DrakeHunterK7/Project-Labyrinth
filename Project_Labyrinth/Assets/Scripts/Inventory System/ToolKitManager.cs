using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToolKitManager : MonoBehaviour
{
    public List<Item> Items;

    public List<Image> ItemImages;
    //private List<Item> OldItems = new List<Item>();

    public Transform ToolKitContent;
    public Transform ItemContent;
    public GameObject InventoryItem;

    public InventoryItemController[] InventoryItems;

    public int capacity;
    private int itemCount = 0;

    void Start()
    {
        ToolKitContent = GameObject.FindWithTag("Toolkit").GetComponent<RectTransform>();
        capacity = InventoryManager.instance.capacity;
    }

    // Update is called once per frame
    void Update()
    {
        Items = InventoryManager.instance.Items;
    }
    

    public void AddToToolkit(Item item)
    {
        if(itemCount < capacity)
        {
            GameObject obj = Instantiate(InventoryItem, ToolKitContent);

            var itemName = obj.transform.Find("ItemName").GetComponent<Text>();
            var itemIcon = obj.transform.Find("ItemIcon").GetComponent<Image>();

            itemName.text = item.itemName;
            itemIcon.sprite = item.icon;
            itemCount++;

            var img = obj.transform.GetComponent<Image>();
            ItemImages.Add(img);
        }
    }
    
    public void RemoveFromToolkit(int index)
    {
        var obj = ToolKitContent.GetChild(index).gameObject;

        itemCount--;

        var img = obj.transform.GetComponent<Image>();
        ItemImages.Remove(img);
            
        Destroy(obj);
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Unity.VisualScripting;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance;
    public List<Item> Items = new List<Item>();

    public ToolKitManager toolkit;

    public Transform ItemContent;
    public GameObject InventoryItem;

    public Toggle EnableRemove;
    public InventoryItemController[] InventoryItems;
    
    private int InventoryIndex = 0;
    public Item selectedItem;

    public int capacity;

    [SerializeField] public PlayerMovement player;

    private void Awake()
    {
        instance = this;
    }

    void Update()
    {
        InventoryScrolling();
    }

    public bool Add(Item item)
    {
        if (Items.Count < toolkit.capacity)
        {
            Items.Add(item);
            toolkit.AddToToolkit(item);
            return true;
        }
        else
        {
            return false;
        }
    }

    public void Remove(Item item)
    {
        Items.Remove(item);
        toolkit.RemoveFromToolkit(InventoryIndex);
        selectedItem = null;
        if (InventoryIndex == 0)
        {
            if (Items.Count != 0)
                InventoryIndex = Items.Count - 1;
            
        }
        else
        {
            InventoryIndex--;
        }
        
    }

    public void ListItems()
    {
        //Freeze current scene:
        Time.timeScale = 0;

        //Debug.Log("inside listItem method");
        foreach(Transform item in ItemContent)
        {
            Destroy(item.gameObject);
        }
        //Debug.Log("second step");
        foreach (var item in Items)
        {
            GameObject obj = Instantiate(InventoryItem, ItemContent);
            var itemName = obj.transform.Find("ItemName").GetComponent<Text>();
            var itemIcon = obj.transform.Find("ItemIcon").GetComponent<Image>();
            var removeButton = obj.transform.Find("RemoveItem").GetComponent<Button>();

            //Debug.Log(itemName.text);
            itemName.text = item.itemName;
            //Debug.Log(item.itemName);
            //Debug.Log(itemName.text);
            itemIcon.sprite = item.icon;

            if(EnableRemove.isOn)
            {
                removeButton.gameObject.SetActive(true);
            }
        }

        SetInventoryItems();
    }
    
    private void InventoryScrolling()
    {
        var scroll = Input.mouseScrollDelta;
        var scrollValue = scroll.y;

        if (scroll.y != 0)
        {
            if (Items.Count == 0) return;
            
            if (selectedItem == null && (InventoryIndex < Items.Count && InventoryIndex >= 0))
            {
                selectedItem = Items[InventoryIndex];
                var image = toolkit.ItemImages[InventoryIndex];
                image.color = Color.yellow;
            }
            else
            {
                if (InventoryIndex + (int) scrollValue == Items.Count)
                {
                    var image = toolkit.ItemImages[InventoryIndex];
                    image.color = Color.grey;
                    InventoryIndex = 0;
                    selectedItem = toolkit.Items[InventoryIndex];
                    image = toolkit.ItemImages[InventoryIndex];
                    image.color = Color.yellow;
                }
                else if (InventoryIndex + (int) scrollValue < 0)
                {
                    var image = toolkit.ItemImages[InventoryIndex];
                    image.color = Color.grey;
                    InventoryIndex = toolkit.Items.Count - 1;
                    selectedItem = toolkit.Items[InventoryIndex];
                    image = toolkit.ItemImages[InventoryIndex];
                    image.color = Color.yellow;
                }
                else if (InventoryIndex < Items.Count && InventoryIndex >= 0)
                {
                    var image = toolkit.ItemImages[InventoryIndex];
                    image.color = Color.grey;
                    InventoryIndex += (int) scrollValue;
                    selectedItem = toolkit.Items[InventoryIndex];
                    image = toolkit.ItemImages[InventoryIndex];
                    image.color = Color.yellow;
                }
            }
        }
    }
    
    public void Use()
    {
        Debug.Log("Trying to use....");
        switch (selectedItem.type)
        {
            case ItemType.Consumable:
                Debug.Log("Used!");
                Remove(selectedItem);
                selectedItem = null;
                break;
            case ItemType.Equippable:
                break;
        }
    }

    public void SetInventoryItems()
    {
        InventoryItems = ItemContent.GetComponentsInChildren<InventoryItemController>();

        for(var i = 0; i < Items.Count; i++)
        {
            InventoryItems[i].AddInventoryitem(Items[i]);
        }
    }

    public void Unfreeze()
    {
        Time.timeScale = 1.0f;
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance;
    public List<Item> Items = new List<Item>();

    public Transform ItemContent;
    public GameObject InventoryItem;

    public Toggle EnableRemove;

    public InventoryItemController[] InventoryItems;

    private void Awake()
    {
        instance = this;
    }

    public void Add(Item item)
    {
        Items.Add(item);
    }

    public void Remove(Item item)
    {
        Items.Remove(item);
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

    public void EnableItemsRemove()
    {
        if(EnableRemove.isOn)
        {
            foreach (Transform item in ItemContent)
            {
                item.Find("RemoveItem").gameObject.SetActive(true);
            }
        }
        else
        {
            foreach (Transform item in ItemContent)
            {
                item.Find("RemoveItem").gameObject.SetActive(false);
            }
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

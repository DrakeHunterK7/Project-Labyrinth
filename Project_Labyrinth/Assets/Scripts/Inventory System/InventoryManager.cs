using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Unity.VisualScripting;
using ColorUtility = UnityEngine.ColorUtility;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance;
    public List<Item> Items = new List<Item>();

    public ToolKitManager toolkit;

    public int InventoryIndex = 0;
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

            item.script = item.prefab.GetComponent<ItemController>();
            item.script.Item = item;

            MessageManager.instance.DisplayMessage("Picked up " + item.name, Color.cyan);
            return true;
        }
        else
        {
            return false;
        }
        
        
    }

    public void Remove(Item item)
    {
        Items.RemoveAt(InventoryIndex);
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
    
    public void Drop()
    {
        Items.RemoveAt(InventoryIndex);
        toolkit.RemoveFromToolkit(InventoryIndex);
        
        var obj = Instantiate(selectedItem.prefab, player.rayHitLocation, Quaternion.identity);

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
    
    private void InventoryScrolling()
    {
        var scroll = Input.mouseScrollDelta;
        var scrollValue = scroll.y;

        if (scroll.y != 0)
        {
            if (Items.Count == 0) return;
            
            if (InventoryIndex - (int) scrollValue == Items.Count)
                {
                    var image = toolkit.ItemImages[InventoryIndex];
                    image.color = new Color(85/255f, 111/255f, 123/255f);
                    InventoryIndex = 0;
                    selectedItem = toolkit.Items[InventoryIndex];
                    image = toolkit.ItemImages[InventoryIndex];
                    image.color = new Color(218/255f, 242/255f, 255/255f);
                }
                else if (InventoryIndex - (int) scrollValue < 0)
                {
                    var image = toolkit.ItemImages[InventoryIndex];
                    image.color = new Color(85/255f, 111/255f, 123/255f);
                    InventoryIndex = toolkit.Items.Count - 1;
                    selectedItem = toolkit.Items[InventoryIndex];
                    image = toolkit.ItemImages[InventoryIndex];
                    image.color = new Color(218/255f, 242/255f, 255/255f);
                }
                else if (InventoryIndex < Items.Count && InventoryIndex >= 0)
                {
                    var image = toolkit.ItemImages[InventoryIndex];
                    image.color = new Color(85/255f, 111/255f, 123/255f);
                    InventoryIndex -= (int) scrollValue;
                    selectedItem = toolkit.Items[InventoryIndex];
                    image = toolkit.ItemImages[InventoryIndex];
                    image.color = new Color(218/255f, 242/255f, 255/255f);
                }
            
        }
    }
    
    public void Use()
    {
        Debug.Log("Trying to use....");
        switch (selectedItem.type)
        {
            case ItemType.Consumable:
                
                if (selectedItem.script.ApplyEffectOnPlayer(player))
                {
                    Debug.Log("Used!");
                    Remove(selectedItem);
                    selectedItem = null;
                }
                else
                {
                    Debug.Log("Can't use!");
                }
               
                break;
            case ItemType.Equippable:
                break;
        }
    }
}

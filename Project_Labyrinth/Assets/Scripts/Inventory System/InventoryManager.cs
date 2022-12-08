
using System.Collections.Generic;
using UnityEngine;
using Inventory_System;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance;
    public List<Item> Items;

    public ToolKitManager toolkit;

    public int InventoryIndex = 0;
    public Item selectedItem;

    public int capacity;

    [SerializeField] public PlayerMovement player;
    [SerializeField] public AudioClip error;
    [SerializeField] public AudioSource itemSound;

    private void Awake()
    {
        instance = this;
    }

    void Update()
    {
        if (GameDirector.instance.isPlayerDead || GameDirector.instance.isLoadingNextLevel) return;
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
            
            player.UnEquipItem();
            
                if (InventoryIndex - (int) scrollValue == Items.Count)
                {
                    var image = toolkit.ItemImages[InventoryIndex];
                    image.color = new Color(85/255f, 111/255f, 123/255f);
                    InventoryIndex = 0;
                    selectedItem = Items[InventoryIndex];
                    image = toolkit.ItemImages[InventoryIndex];
                    image.color = new Color(218/255f, 242/255f, 255/255f);
                }
                else if (InventoryIndex - (int) scrollValue < 0)
                {
                    var image = toolkit.ItemImages[InventoryIndex];
                    image.color = new Color(85/255f, 111/255f, 123/255f);
                    InventoryIndex = toolkit.Items.Count - 1;
                    selectedItem = Items[InventoryIndex];
                    image = toolkit.ItemImages[InventoryIndex];
                    image.color = new Color(218/255f, 242/255f, 255/255f);
                }
                else if (InventoryIndex < Items.Count && InventoryIndex >= 0)
                {
                    var image = toolkit.ItemImages[InventoryIndex];
                    image.color = new Color(85/255f, 111/255f, 123/255f);
                    InventoryIndex -= (int) scrollValue;
                    selectedItem = Items[InventoryIndex];
                    image = toolkit.ItemImages[InventoryIndex];
                    image.color = new Color(218/255f, 242/255f, 255/255f);
                }
            
        }
    }

    void playItemSound(bool success)
    {
        
        if(itemSound.isPlaying)
            itemSound.Stop();
        
        if (!success)
        {
            itemSound.clip = error;
            itemSound.Play();
        }
        else
        {
            itemSound.clip = selectedItem.useSound;
            itemSound.Play();
        }
        
        
    }
    
    public void Use()
    {
        switch (selectedItem.type)
        {
            case ItemType.Consumable:
                
                if (selectedItem.script.ApplyEffectOnPlayer(player))
                {
                    playItemSound(true);
                    Debug.Log("Used!");
                    Remove(selectedItem);
                    selectedItem = null;
                }
                else
                {
                    playItemSound(false);
                }
               
                break;
            case ItemType.Equippable:

                if (player.isEquipped)
                {
                    foreach (MonoBehaviour script in player.equippedObjectLocation.GetChild(0).gameObject.GetComponents<MonoBehaviour>())
                    {
                        if (script is IEquippableItemAction targetScript)
                        {
                            targetScript.Use(player);
                        }
                    }
                    
                    Remove(selectedItem);
                    player.isEquipped = false;
                }
                else
                {
                    player.EquipItem(selectedItem.prefab);
                }
                
                break;
            
            case ItemType.TaskItem:
                if (player.interactable == null)
                {
                    MessageManager.instance.DisplayMessage("Nothing to use " + selectedItem.itemName + " on!", Color.yellow);
                    playItemSound(false);
                }
                    
                else
                {
                    foreach (MonoBehaviour script in player.interactable.GetComponentsInChildren<MonoBehaviour>())
                    {
                        if (script is Interactable targetScript)
                        {
                            if (targetScript.Check(selectedItem.itemName))
                            {
                                targetScript.Unlock();
                                playItemSound(true);
                                Debug.Log("Used!");
                                Remove(selectedItem);
                                selectedItem = null;
                            }
                            else
                            {
                                MessageManager.instance.DisplayMessage("Can't use " + selectedItem.itemName + " here!", Color.yellow);
                                playItemSound(false);
                            }
                        }
                    }
                }
                

                break;
        }
    }
}

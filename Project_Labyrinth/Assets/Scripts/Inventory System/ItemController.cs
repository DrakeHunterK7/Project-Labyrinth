using System;
using System.Collections;
using System.Collections.Generic;
using Inventory_System;
using UnityEngine;

public class ItemController : MonoBehaviour
{
    public Item Item;
   
    public bool ApplyEffectOnPlayer(PlayerMovement player)
    {

        if (Item.type != ItemType.Consumable)
        {
            Debug.Log("1");
            return false;
        }
        
        switch (Item.playerAttributeAffected)
        {
            case PlayerAttributeAffected.Health:
                if (player.HealthRestore(Item.value))
                    return true;
                else
                {
                    Debug.Log(Item.value);
                    Debug.Log("2");
                    return false;
                }
                    
                
            case PlayerAttributeAffected.Stamina:
                break;
            
            case PlayerAttributeAffected.MovementSpeed:
                if (Item.effectLength == EffectLength.Temporary)
                {
                    ApplyTemporaryEffect(player);
                    return true;
                }
                else
                {
                    player.speedBoostMultiplier = Item.value;
                    return true;
                }
                
        }

        Debug.Log("3");
        return false;
    }
    void ApplyTemporaryEffect(PlayerMovement player)
    {
       switch (Item.playerAttributeAffected)
        {
            case PlayerAttributeAffected.Health:
                player.health = Item.value;
                break;
            case PlayerAttributeAffected.Stamina:
                break;
            case PlayerAttributeAffected.MovementSpeed:
                player.speedBoostMultiplier = Item.value;
                break;
        }

       player.StartCoroutine(player.RestoreVariable(Item.playerAttributeAffected, Item.effectDuration));

    }
    
}

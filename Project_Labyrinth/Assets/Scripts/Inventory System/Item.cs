
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Item/Create New Item")]

public class Item : ScriptableObject
{
    public int id;
    public string itemName;
    public float value;
    public Sprite icon; 
    public GameObject prefab;
    public ItemType type;
    public PlayerAttributeAffected playerAttributeAffected;
    public EffectLength effectLength;
    public float effectDuration;
    public ItemController script;
    public AudioClip useSound;

    private void OnEnable()
    {
        
    }

    
}

public enum ItemType
{
    Consumable, 
    Equippable,
    TaskItem,
}

public enum PlayerAttributeAffected
{
    Health, 
    Stamina,
    MovementSpeed,
}

public enum EffectLength
{
    Temporary,
    Permanent
}




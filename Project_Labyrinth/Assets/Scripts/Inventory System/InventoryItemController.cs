using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryItemController : MonoBehaviour
{
    public Item item;

    public Button RemoveItem;

    private Transform _playerTransform;

    public static bool refreshDelete = false;

    //public GameObject prefab;

    public void RemoveInventoryItem()
    {
        InventoryManager.instance.Remove(item);
        Destroy(gameObject);

    }

    public void AddInventoryitem(Item newItem)
    {
        item = newItem;
    }

    public void putBack()
    {
        _playerTransform = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();

        if (_playerTransform == null || item == null) return;
        Instantiate(item.prefab, _playerTransform.position + _playerTransform.forward * 1.0f
        , Quaternion.identity);
    }

    public void refreshToolKit()
    {
        refreshDelete = true;
    }

}

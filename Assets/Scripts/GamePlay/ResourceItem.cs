using UnityEngine;

public class ResourceItem : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(GameManager.PlayerTag)) return;

        InventorySystem inventory = other.GetComponent<InventorySystem>();
        if (inventory == null) return;

        inventory.CollectResource();
        Destroy(gameObject);
    }
}
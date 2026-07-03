using UnityEngine;

public class InventorySystem : MonoBehaviour
{
    private int _count;
    
    public void CollectResource()
    {
        if (_count >= GameManager.RequiredResources) return;  // 이미 다 모은 경우 무시
        _count++;
        EventBus.Publish(new ResourceCollectedEvent { totalCollected = _count });
    }
}
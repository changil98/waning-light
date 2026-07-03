using UnityEngine;

public class EnemyContactCapture : MonoBehaviour
{
    private bool _triggered;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_triggered) return;
        if (!other.CompareTag(GameManager.PlayerTag)) return;
        _triggered = true;
        EventBus.Publish(new PlayerCapturedEvent());
    }
}
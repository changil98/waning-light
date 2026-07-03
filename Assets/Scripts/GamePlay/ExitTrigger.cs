using UnityEngine;

public class ExitTrigger : MonoBehaviour
{
    [SerializeField] private Sprite _openSprite;

    private bool _isOpen;
    private bool _triggered;
    private SpriteRenderer _spriteRenderer;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        EventBus.Subscribe<ResourceCollectedEvent>(OnResourceCollected);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<ResourceCollectedEvent>(OnResourceCollected);
    }

    private void OnResourceCollected(ResourceCollectedEvent e)
    {
        if (e.totalCollected >= GameManager.RequiredResources)
        {
            _spriteRenderer.sprite = _openSprite;
            _isOpen = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_triggered || !_isOpen) return;
        if (!other.CompareTag(GameManager.PlayerTag)) return;
        _triggered = true;
        EventBus.Publish(new PlayerReachedExitEvent());
    }
}
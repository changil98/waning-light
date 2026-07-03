using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed;
    [SerializeField] private float sprintMultiplier;
    [SerializeField] private Light2D flashlight;

    [SerializeField] private InputAction _moveAction;
    [SerializeField] private InputAction _sprintAction;
    [SerializeField] private InputAction _flashlightAction;

    private Rigidbody2D _rb;
    private Animator _animator;
    private SpriteRenderer _spriteRenderer;
    private Vector2 _facingDirection = Vector2.down;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        _moveAction.Enable();
        _sprintAction.Enable();
        _flashlightAction.Enable();
        _flashlightAction.performed += OnFlashlightToggle;
    }

    private void OnDisable()
    {
        _moveAction.Disable();
        _sprintAction.Disable();
        _flashlightAction.Disable();
        _flashlightAction.performed -= OnFlashlightToggle;
    }

    private void OnDestroy()
    {
        _moveAction.Dispose();
        _sprintAction.Dispose();
        _flashlightAction.Dispose();
    }

    private void Start()
    {
        EventBus.Publish(new FlashlightToggledEvent { isOn = flashlight.enabled });
    }

    private void FixedUpdate()
    {
        Vector2 moveInput = _moveAction.ReadValue<Vector2>();
        bool isSprinting = _sprintAction.IsPressed();

        if (Mathf.Abs(moveInput.x) > 0.01f)
            _spriteRenderer.flipX = moveInput.x < 0;

        if (moveInput.sqrMagnitude > 0.01f)
        {
            _facingDirection = moveInput.normalized;
            float angle = Mathf.Atan2(_facingDirection.y, _facingDirection.x) * Mathf.Rad2Deg - 90f;
            flashlight.transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        _animator.SetBool("IsMoving", moveInput.sqrMagnitude > 0.01f);

        _rb.linearVelocity = moveInput * moveSpeed * (isSprinting ? sprintMultiplier : 1f);

        if (moveInput.magnitude > 0.1f)
            EventBus.Publish(new NoiseEvent 
            {
                Position = _rb.position,
                IsSprinting = isSprinting
            });
    }

    private void OnFlashlightToggle(InputAction.CallbackContext context)
    {
        flashlight.enabled = !flashlight.enabled;
        EventBus.Publish(new FlashlightToggledEvent { isOn = flashlight.enabled });
    }
}
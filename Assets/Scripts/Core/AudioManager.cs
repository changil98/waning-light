using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;       // 재생 담당 컴포넌트
    [SerializeField] private AudioSource alertAudioSource;  // 감지음 전용
    [SerializeField] private AudioClip footstepWalk;        // 걷기 발걸음
    [SerializeField] private AudioClip footstepSprint;      // 뛰기 발검음
    [SerializeField] private AudioClip resourcePickup;      // 자원 수집음
    [SerializeField] private AudioClip detectionAlert;      // 적 감지음

    private float _footstepCooldown;
    private bool _isAlertPlaying;

    private void OnEnable()
    {
        EventBus.Subscribe<NoiseEvent>(OnNoiseEvent);
        EventBus.Subscribe<ResourceCollectedEvent>(OnResourceCollectedEvent);
        EventBus.Subscribe<PlayerDetectedEvent>(OnPlayerDetectedEvent);
        EventBus.Subscribe<PlayerEscapedEvent>(OnPlayerEscapedEvent);
        EventBus.Subscribe<PlayerCapturedEvent>(OnPlayerCapturedEvent);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<NoiseEvent>(OnNoiseEvent);
        EventBus.Unsubscribe<ResourceCollectedEvent>(OnResourceCollectedEvent);
        EventBus.Unsubscribe<PlayerDetectedEvent>(OnPlayerDetectedEvent);
        EventBus.Unsubscribe<PlayerEscapedEvent>(OnPlayerEscapedEvent);
        EventBus.Unsubscribe<PlayerCapturedEvent>(OnPlayerCapturedEvent);
    }

    private void OnNoiseEvent(NoiseEvent e)
    {
        if (Time.time < _footstepCooldown) return;
        AudioClip clip = e.IsSprinting ? footstepSprint : footstepWalk;
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
            _footstepCooldown = Time.time + clip.length;
        }
    }

    private void OnResourceCollectedEvent(ResourceCollectedEvent e)
    {
        if (e.totalCollected > 0 && resourcePickup != null && audioSource != null)
            audioSource.PlayOneShot(resourcePickup);
    }

    private void OnPlayerDetectedEvent(PlayerDetectedEvent e)
    {
        if (_isAlertPlaying) return;
        if (detectionAlert != null && alertAudioSource != null)
        {
            alertAudioSource.clip = detectionAlert;
            alertAudioSource.loop = true;
            alertAudioSource.Play();
            _isAlertPlaying = true;
        }
    }

    private void OnPlayerEscapedEvent(PlayerEscapedEvent e)
    {
        alertAudioSource?.Stop();
        _isAlertPlaying = false;
    }

    private void OnPlayerCapturedEvent(PlayerCapturedEvent e)
    {
        alertAudioSource?.Stop();
        _isAlertPlaying = false;
    }
}
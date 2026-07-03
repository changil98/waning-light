using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI resourceText;
    [SerializeField] private Image flashlightIcon;
    [SerializeField] private Sprite flashlightOnSprite;
    [SerializeField] private Sprite flashlightOffSprite;
    [SerializeField] private Image captureWarning;
    [SerializeField] private Image loadingPanel;
    [SerializeField] private TextMeshProUGUI loadingText;

    private Coroutine _warningCoroutine;

    private void OnEnable()
    {
        EventBus.Subscribe<ResourceCollectedEvent>(OnResourceCollected);
        EventBus.Subscribe<FlashlightToggledEvent>(OnFlashlightToggled);
        EventBus.Subscribe<PlayerDetectedEvent>(OnPlayerDetected);
        EventBus.Subscribe<PlayerEscapedEvent>(OnPlayerEscaped);
        EventBus.Subscribe<PlayerCapturedEvent>(OnPlayerCaptured);
        EventBus.Subscribe<LevelGeneratedEvent>(OnLevelGenerated);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<ResourceCollectedEvent>(OnResourceCollected);
        EventBus.Unsubscribe<FlashlightToggledEvent>(OnFlashlightToggled);
        EventBus.Unsubscribe<PlayerDetectedEvent>(OnPlayerDetected);
        EventBus.Unsubscribe<PlayerEscapedEvent>(OnPlayerEscaped);
        EventBus.Unsubscribe<PlayerCapturedEvent>(OnPlayerCaptured);
        EventBus.Unsubscribe<LevelGeneratedEvent>(OnLevelGenerated);
    }

    private void Start()
    {
        loadingPanel.color = new Color(0, 0, 0, 1);
    }

    private void OnLevelGenerated(LevelGeneratedEvent e)
    {
        StartCoroutine(Fade(loadingPanel, 0f, 1f));
        loadingText.color = new Color(loadingText.color.r, loadingText.color.g, loadingText.color.b, 0f);
    }

    private void OnResourceCollected(ResourceCollectedEvent e)
    {
        resourceText.text = $"{e.totalCollected} / {GameManager.RequiredResources}";
    }

    private void OnFlashlightToggled(FlashlightToggledEvent e)
    {
        flashlightIcon.sprite = e.isOn ? flashlightOnSprite : flashlightOffSprite;
    }

    private void OnPlayerDetected(PlayerDetectedEvent e)
    {
        if (_warningCoroutine != null) StopCoroutine(_warningCoroutine);
        _warningCoroutine = StartCoroutine(Fade(captureWarning, 0.4f, 0.5f));
    }

    private void OnPlayerEscaped(PlayerEscapedEvent e)
    {
        if (_warningCoroutine != null) StopCoroutine(_warningCoroutine);
        _warningCoroutine = StartCoroutine(Fade(captureWarning, 0f, 0.5f));
    }

    private void OnPlayerCaptured(PlayerCapturedEvent e)
    {
        if (_warningCoroutine != null) StopCoroutine(_warningCoroutine);
        _warningCoroutine = StartCoroutine(Fade(captureWarning, 0f, 0.5f));
    }

    IEnumerator Fade(Image image, float targetAlpha, float duration)
    {
        float startAlpha = image.color.a;
        Color baseColor = image.color;
        for (float t = 0; t < duration; t += Time.unscaledDeltaTime)
        {
            float a = Mathf.Lerp(startAlpha, targetAlpha, t / duration);
            image.color = new Color(baseColor.r, baseColor.g, baseColor.b, a);
            yield return null;
        }
        image.color = new Color(baseColor.r, baseColor.g, baseColor.b, targetAlpha);
    }
}
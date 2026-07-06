using UnityEngine;

public enum GameState
{
    Playing,
    Won,
    Lost
}

public struct LevelGeneratedEvent { }
public struct NoiseEvent { public Vector2 Position; public bool IsSprinting; }
public struct ResourceCollectedEvent { public int totalCollected; }
public struct PlayerReachedExitEvent { }
public struct PlayerCapturedEvent { }
public struct FlashlightToggledEvent { public bool isOn; }
public struct PlayerDetectedEvent { }
public struct PlayerEscapedEvent { }
public struct GameStateChangedEvent { public GameState newState; public float elapsedTime; }

public class GameManager : MonoBehaviour
{
    private int _collectedResources;
    private GameState _currentState;
    private float _startTime;

    public const int RequiredResources = 3;
    public const string PlayerTag = "Player";
    public const string GameSceneName = "GameScene";

    private void OnEnable()
    {
        EventBus.Subscribe<ResourceCollectedEvent>(OnCollected);
        EventBus.Subscribe<PlayerReachedExitEvent>(OnPlayerReached);
        EventBus.Subscribe<PlayerCapturedEvent>(OnPlayerCaptured);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<ResourceCollectedEvent>(OnCollected);
        EventBus.Unsubscribe<PlayerReachedExitEvent>(OnPlayerReached);
        EventBus.Unsubscribe<PlayerCapturedEvent>(OnPlayerCaptured);
    }

    private void Start()
    {
        _startTime = Time.time;
        _currentState = GameState.Playing;
        EventBus.Publish(new ResourceCollectedEvent { totalCollected = _collectedResources });
    }

    private void OnCollected(ResourceCollectedEvent e)
    {
        if (_currentState != GameState.Playing) return;
        _collectedResources = e.totalCollected;
    }

    private void OnPlayerReached(PlayerReachedExitEvent e)
    {
        if (_currentState != GameState.Playing) return;
        if (_collectedResources >= RequiredResources)
            SetState(GameState.Won);
    }

    private void OnPlayerCaptured(PlayerCapturedEvent e)
    {
        if (_currentState != GameState.Playing) return;
        SetState(GameState.Lost);
    }

    private void SetState(GameState newState)
    {
        _currentState = newState;
        EventBus.Publish(new GameStateChangedEvent { newState = newState, elapsedTime = Time.time - _startTime });
    }

    public static string FormatTime(float seconds)
    {
        int totalSeconds = Mathf.FloorToInt(seconds);
        return $"{totalSeconds / 60:00}:{totalSeconds % 60:00}";
    }
}
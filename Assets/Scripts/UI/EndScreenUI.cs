using UnityEngine;
using UnityEngine.SceneManagement;

public class EndScreenUI : MonoBehaviour
{
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;

    private void OnEnable()
    {
        EventBus.Subscribe<GameStateChangedEvent>(OnOpenPanel);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<GameStateChangedEvent>(OnOpenPanel);
    }

    private void Start()
    {
        winPanel.SetActive(false);
        losePanel.SetActive(false);
    }

    private void OnOpenPanel(GameStateChangedEvent e)
    {
        if (e.newState == GameState.Won) OpenPanel(winPanel);
        else if (e.newState == GameState.Lost) OpenPanel(losePanel);
    }

    private void OpenPanel(GameObject panel)
    {
        panel.SetActive(true);
        Time.timeScale = 0;
    }

    public void OnRestartButton()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(GameManager.GameSceneName);
    }
}
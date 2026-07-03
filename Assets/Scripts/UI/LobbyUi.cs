using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyUi : MonoBehaviour
{
    [SerializeField] private GameObject controlsPanel;

    public void OnStartButton()
    {
        SceneManager.LoadScene(GameManager.GameSceneName);
    }

    public void OnQuitButton()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void OnHelpButton()
    {
        controlsPanel.SetActive(true);
    }

    public void OnCloseHelpButton()
    {
        controlsPanel.SetActive(false);
    }
}
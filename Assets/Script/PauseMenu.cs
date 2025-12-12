using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pauseOverlay;

    private bool isPaused = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!isPaused) ShowPauseMenu();
            else HidePauseMenu();
        }
    }

    public void ShowPauseMenu()
    {
        pauseOverlay.SetActive(true);
        isPaused = true;
        Time.timeScale = 0f;  // 让画面停住（可选）
    }

    public void HidePauseMenu()
    {
        pauseOverlay.SetActive(false);
        isPaused = false;
        Time.timeScale = 1f;  // 恢复（可选）
    }

    public void OnResumeClicked()
    {
        HidePauseMenu();
    }

    public void OnBackToMenuClicked()
    {
        SceneManager.LoadScene("MainMenuScene");
    }
}

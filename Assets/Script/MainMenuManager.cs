using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject instructionsOverlay;

    // START 按钮：进入 Level 1
    public void StartGame()
    {
        SceneManager.LoadScene("Level1Scene");
    }

    // INSTRUCTIONS 按钮：显示说明层
    public void ShowInstructions()
    {
        if (instructionsOverlay != null)
            instructionsOverlay.SetActive(true);
    }

    // 说明界面的 CLOSE / BACK 按钮
    public void HideInstructions()
    {
        if (instructionsOverlay != null)
            instructionsOverlay.SetActive(false);
    }

    // EXIT 按钮
    public void QuitGame()
    {
        Debug.Log("Quit Game");
        Application.Quit();
    }
}

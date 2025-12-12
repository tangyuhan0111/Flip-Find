using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelectMenu : MonoBehaviour
{
    public string level1Scene = "Level1Scene";
    public string level2Scene = "Level2Scene";
    public string level3Scene = "Level3Scene";

    public void OnLevel1Clicked()
    {
        SceneManager.LoadScene(level1Scene);
    }

    public void OnLevel2Clicked()
    {
        SceneManager.LoadScene(level2Scene);
    }

    public void OnLevel3Clicked()
    {
        SceneManager.LoadScene(level3Scene);
    }

    public void OnBackClicked()
    {
        SceneManager.LoadScene("MainMenuScene");
    }
}

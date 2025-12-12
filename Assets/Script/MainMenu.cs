using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // 拖入：Canvas 下的 InstructionsOverlay（整块遮罩+说明面板）
    [SerializeField] private GameObject instructionsOverlay;

    // 如果有 LevelSelectScene 你可以在 Inspector 填
    [SerializeField] private string levelSelectSceneName = "LevelSelectScene";

    private void Start()
    {
        // 启动时隐藏说明界面
        if (instructionsOverlay != null)
        {
            instructionsOverlay.SetActive(false);
        }
    }

    // START 按钮
    public void OnStartButtonClicked()
    {
        SceneManager.LoadScene(levelSelectSceneName);
    }

    // INSTRUCTIONS 按钮
    public void OnInstructionsButtonClicked()
    {
        if (instructionsOverlay != null)
        {
            instructionsOverlay.SetActive(true);
        }
    }

    // BACK 按钮
    public void OnBackButtonClicked()
    {
        if (instructionsOverlay != null)
        {
            instructionsOverlay.SetActive(false);
        }
    }

    // EXIT 按钮
    public void OnExitButtonClicked()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}

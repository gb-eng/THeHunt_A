using UnityEngine;
using UnityEngine.UI;

public class GameBackButton : MonoBehaviour
{
    public Button backButton;
    public string targetSceneName = "F_gameScreen";

    private void Start()
    {
        if (backButton == null)
            backButton = GetComponent<Button>();

        if (backButton != null)
            backButton.onClick.AddListener(HandleBack);
        else
            Debug.LogError("MinigameBackButton: No Button component found.");
    }

    private void HandleBack()
    {
        var loader = Object.FindFirstObjectByType<ui_sceneLoader>();
        if (loader != null)
        {
            loader.LoadScene(targetSceneName);
        }
        else
        {
            Debug.LogError("MinigameBackButton: ui_sceneLoader not found in scene.");
        }
    }
}
using UnityEngine;
using UnityEngine.UIElements;

public class MainScreenUIController : MonoBehaviour
{
    private VisualElement root;
    private Button exitButton;

    public script_popup popupManager;
    public script_login loginManager;

    void Awake()
    {
        var uiDoc = GetComponent<UIDocument>();
        if (uiDoc == null)
        {
            Debug.LogError("[MainScreen] UIDocument missing on this GameObject.");
            return;
        }

        root = uiDoc.rootVisualElement;
        if (root == null)
        {
            Debug.LogError("[MainScreen] rootVisualElement is null.");
            return;
        }

        exitButton = root.Q<Button>("exitButton");
        if (exitButton == null)
        {
            Debug.LogWarning("[MainScreen] exitButton not found in UXML.");
        }
        else
        {
            exitButton.clicked += OnExitButtonClicked;
        }

        // IMPORTANT: do NOT call DontDestroyOnLoad here for full-screen UI.
        // Leaving the main screen persistent will block the AR scene when loading with LoadSceneMode.Single.
    }

    private void OnExitButtonClicked()
    {
        Debug.Log("[MainScreen] Exit button clicked!");

        if (popupManager == null)
        {
            Debug.LogError("[MainScreen] popupManager not assigned in Inspector!");
            return;
        }

        Debug.Log("[MainScreen] popupManager found, calling ShowPopup...");
        popupManager.ShowPopup(PopupType.pop_logout, () =>
        {
            Debug.Log("[MainScreen] Logout confirmed!");
            if (loginManager != null)
            {
                loginManager.LogoutUser();
            }
            else
            {
                Debug.LogError("[MainScreen] loginManager not assigned in Inspector!");
            }
        });
    }
}
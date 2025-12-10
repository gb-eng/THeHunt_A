using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class GameInstructionsUI : MonoBehaviour
{
    public UIDocument uiDocument;

    [System.Obsolete]
    void OnEnable()
    {
        // Auto-link UIDocument if not manually assigned
        if (uiDocument == null)
            uiDocument = GetComponent<UIDocument>();

        if (uiDocument == null)
        {
            Debug.LogError("❌ No UIDocument assigned to GameInstructionsUI!");
            return;
        }

        uiDocument.rootVisualElement.RegisterCallback<GeometryChangedEvent>(OnUILoaded);
    }

    [System.Obsolete]
    void OnUILoaded(GeometryChangedEvent evt)
    {
        var root = uiDocument.rootVisualElement;
        if (root == null)
        {
            Debug.LogError("❌ rootVisualElement is null — UXML not loaded!");
            return;
        }

        var playButton = root.Q<Button>("playButton");
        var backButton = root.Q<Button>("backbutton_F");
        var instructionsLabel = root.Q<Label>("instructions");

        // ✅ Display selected title & instructions
        if (instructionsLabel != null && script_gameManager.Instance != null)
        {
            instructionsLabel.text =
                $"<b>{script_gameManager.Instance.selectedTitle}</b>\n\n{script_gameManager.Instance.selectedInstructions}";
            Debug.Log($"✅ Loaded instructions for: {script_gameManager.Instance.selectedTitle}");
        }
        else
        {
            Debug.LogWarning("⚠️ Missing GameManager or instructions label.");
        }

        // ✅ PLAY BUTTON — loads minigame and clears leftover UI
        if (playButton != null)
        {
            playButton.clicked += () =>
{
    string nextScene = script_gameManager.Instance?.selectedGame;
    if (string.IsNullOrEmpty(nextScene))
    {
        Debug.LogError("❌ No selected game found in GameManager!");
        return;
    }

    Debug.Log($"▶ Loading minigame: {nextScene}");

    // 🧹 STEP 1: Clean up leftover UIDocuments and Panels (UI Toolkit)
    foreach (var doc in FindObjectsOfType<UIDocument>(true))
    {
        if (doc.name.Contains("ui_") || doc.name.Contains("gameScreen") || doc.name.Contains("Instruct"))
        {
            Debug.Log($"🧨 Removing UI Toolkit document: {doc.name}");
            doc.rootVisualElement.Clear();
            Destroy(doc.gameObject);
        }
    }

    // 🧹 STEP 2: Clean up leftover Canvases (in case there’s a mix of UGUI)
    foreach (var c in FindObjectsOfType<Canvas>(true))
    {
        if (c.name.Contains("ui_") || c.name.Contains("gameScreen") || c.name.Contains("Instruct"))
        {
            Debug.Log($"🧨 Removing leftover Canvas: {c.name}");
            Destroy(c.gameObject);
        }
    }

    // 🧹 STEP 3: Clean up persistent objects inside DontDestroyOnLoad
    var dontDestroyScene = SceneManager.GetSceneByName("DontDestroyOnLoad");
    if (dontDestroyScene.IsValid())
    {
        foreach (var obj in dontDestroyScene.GetRootGameObjects())
        {
            if (obj.name.Contains("ui_") || obj.name.Contains("gameScreen") || obj.name.Contains("Instruct"))
            {
                Debug.Log($"🔥 Force-destroying object from DontDestroyOnLoad: {obj.name}");
                Destroy(obj);
            }
        }
    }

    // 🧹 STEP 4: Clear leftover cameras (the UI Toolkit sometimes leaves a ghost one)
    foreach (var cam in Camera.allCameras)
    {
        if (cam.name.Contains("UI") || cam.name.Contains("TextureBufferCamera"))
        {
            Debug.Log($"🎯 Destroying leftover UI camera: {cam.name}");
            Destroy(cam.gameObject);
        }
    }

    // 🚀 STEP 5: Load new scene cleanly
    Debug.Log("🌍 Loading minigame scene...");
    SceneManager.LoadScene(nextScene, LoadSceneMode.Single);
};

        }
        else
        {
            Debug.LogWarning("⚠️ Play button not found.");
        }

        // ✅ BACK BUTTON — returns to game menu
        if (backButton != null)
        {
            backButton.clicked += () =>
            {
                Debug.Log("🔙 Returning to F_gameScreen...");
                SceneManager.LoadScene("F_gameScreen", LoadSceneMode.Single);
            };
        }
        else
        {
            Debug.LogWarning("⚠️ Back button not found.");
        }
    }
}

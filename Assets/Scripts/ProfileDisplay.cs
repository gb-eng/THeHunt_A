using UnityEngine;
using UnityEngine.UIElements;

public class ProfileDisplay : MonoBehaviour
{
    [Header("UI Settings")]
    public UIDocument uiDocument;
    // ✅ Make sure this matches the name you typed in UI Builder Inspector
    public string labelName = "UserNameLabel"; 

    void OnEnable()
    {
        if (uiDocument == null) uiDocument = GetComponent<UIDocument>();
        var root = uiDocument.rootVisualElement;

        // 1. Find the Label
        Label nameLabel = root.Q<Label>(labelName);

        if (nameLabel != null)
        {
            // 2. Get Name from Memory
            // If no name is saved yet, it defaults to "Guest Explorer"
            string playerName = PlayerPrefs.GetString("player_name", "Guest Explorer");
            
            // 3. Update Text
            nameLabel.text = playerName;
            Debug.Log($"Updated Profile Name to: {playerName}");
        }
        else
        {
            Debug.LogWarning($"❌ Could not find Label named '{labelName}' in the UXML. Check the name in UI Builder.");
        }
    }
}
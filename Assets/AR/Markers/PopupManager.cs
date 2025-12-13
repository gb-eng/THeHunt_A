using UnityEngine;
using UnityEngine.UIElements;
using System.Globalization;
using System; 
using System.Collections; 

public class PopupManager : MonoBehaviour // ✅ Renamed from RewardPopup
{
    public static PopupManager Instance { get; private set; }

    [Header("UI Reference")]
    public UIDocument uiDocument; 

    private VisualElement root;
    private VisualElement popupContainer;
    private Label titleLabel;
    private Label nameLabel;
    private Image iconImage;
    private Button confirmButton;

    private Action onPopupClose; 

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (uiDocument == null) uiDocument = GetComponent<UIDocument>();
        
        if (uiDocument != null)
        {
            root = uiDocument.rootVisualElement;
            popupContainer = root.Q<VisualElement>("popup_scan");
            titleLabel = root.Q<Label>("window_text");
            nameLabel = root.Q<Label>("item_name");
            iconImage = root.Q<Image>("reward_icon");
            confirmButton = root.Q<Button>("window_button");

            // Ensure it is hidden on start
            if (popupContainer != null) 
            {
                popupContainer.style.display = DisplayStyle.None;
                popupContainer.style.opacity = 0;
            }

            if (confirmButton != null) confirmButton.clicked += ClosePopup;
        }
    }

    public void ShowReward(string assetId, string title = "Item Unlocked!", Action onClose = null)
    {
        this.onPopupClose = onClose;

        if (popupContainer == null) return;

        // ✅ AUTO-FIX: Ensure we look for the Thumbnail "T_" version
        string thumbName = assetId;
        if (!thumbName.StartsWith("T_")) thumbName = "T_" + thumbName;

        Sprite thumb = Resources.Load<Sprite>($"Thumbnails/{thumbName}");

        // Cleanup name for display (Remove "T_", "BAS_", etc.)
        // e.g. "T_BAS_Basilica" -> "Basilica"
        string cleanName = assetId.Replace("T_", "").Replace("M_", "");
        
        // Remove Area Code if present (BAS_, MAR_, etc.)
        if (cleanName.Length > 4 && cleanName[3] == '_') 
            cleanName = cleanName.Substring(4);

        string displayName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(cleanName);

        if (titleLabel != null) titleLabel.text = title;
        if (nameLabel != null) nameLabel.text = displayName;
        
        if (iconImage != null) 
        {
            if (thumb != null) 
            {
                iconImage.sprite = thumb;
                iconImage.style.display = DisplayStyle.Flex;
            }
            else 
            {
                // Try loading without "T_" just in case
                thumb = Resources.Load<Sprite>($"Thumbnails/{assetId}");
                if (thumb != null)
                {
                    iconImage.sprite = thumb;
                    iconImage.style.display = DisplayStyle.Flex;
                }
                else
                {
                    iconImage.style.display = DisplayStyle.None; 
                }
            }
        }

        // Start Animation
        StartCoroutine(AnimatePopupOpen());
    }

    private void ClosePopup()
    {
        if (popupContainer != null) 
            popupContainer.style.display = DisplayStyle.None;

        if (onPopupClose != null)
        {
            onPopupClose.Invoke();
            onPopupClose = null; 
        }
    }

    IEnumerator AnimatePopupOpen()
    {
        if (popupContainer == null) yield break;

        // Reset State
        popupContainer.style.display = DisplayStyle.Flex;
        popupContainer.style.opacity = 1;
        
        // Initialize Scale to 0
        popupContainer.style.scale = new Scale(Vector3.zero);

        float duration = 0.4f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            float scaleAmount = BackOut(t); 
            popupContainer.style.scale = new Scale(Vector3.one * scaleAmount);
            
            yield return null;
        }

        popupContainer.style.scale = new Scale(Vector3.one);
    }

    float BackOut(float t)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1;
        return 1 + c3 * Mathf.Pow(t - 1, 3) + c1 * Mathf.Pow(t - 1, 2);
    }
}
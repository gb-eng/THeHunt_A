using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements.Experimental;

public enum PopupType
{
    pop_error,
    pop_logout,
    pop_reserve,
    pop_success,
    pop_block
}

public class script_popup : MonoBehaviour
{
    private VisualElement root;
    private VisualElement popupOverlay;
    private Label popupTitle;
    private Label popupMessage;
    private Button confirmButton;
    private Button cancelButton; // ✅ NEW

    private Dictionary<PopupType, (string title, string message, string button)> popupPresets;
    private int fadeDuration = 250;

    private bool isInitialized = false;
    
    // Queue for popups that need to show after activation
    private class PendingPopup
    {
        public PopupType type;
        public Action onConfirm;
        public string confirmLabel;
    }
    private PendingPopup pendingPopup = null;

    private void Awake()
    {
        InitializePresets();
    }

    private void OnEnable()
    {
        Debug.Log("[Popup] OnEnable called");
        StartCoroutine(DelayedInitialize());
    }

    private void Update()
    {
        if (isInitialized && pendingPopup != null && gameObject.activeInHierarchy)
        {
            Debug.Log("[Popup] Found pending popup in Update, showing now...");
            var pending = pendingPopup;
            pendingPopup = null;
            ShowPopup(pending.type, pending.onConfirm, pending.confirmLabel);
        }
    }

    private IEnumerator DelayedInitialize()
    {
        yield return null;
        InitializePopup();

        if (!isInitialized)
        {
            Debug.Log("[Popup] Not initialized yet — running InitializePopup manually.");
            InitializePopup();
        }

        if (pendingPopup != null)
        {
            Debug.Log("[Popup] Found pending popup in DelayedInitialize");
            var pending = pendingPopup;
            pendingPopup = null;
            ShowPopup(pending.type, pending.onConfirm, pending.confirmLabel);
        }
    }

    private IEnumerator ShowPendingPopupAfterActivation()
    {
        Debug.Log("[Popup] Waiting for initialization after activation...");
        yield return null;
        yield return null;
        
        if (pendingPopup != null)
        {
            Debug.Log("[Popup] Showing pending popup after activation");
            var pending = pendingPopup;
            pendingPopup = null;
            ShowPopup(pending.type, pending.onConfirm, pending.confirmLabel);
        }
    }

    public void InitializePopup()
    {
        var uiDoc = GetComponent<UIDocument>();
        if (uiDoc == null)
        {
            Debug.LogError("[Popup] UIDocument not found! Attach this script to the same GameObject as your popup UIDocument.");
            return;
        }

        root = uiDoc.rootVisualElement;
        if (root == null)
        {
            Debug.LogWarning("[Popup] rootVisualElement is null! Waiting for UIDocument to initialize...");
            return;
        }

        popupOverlay = root.Q<VisualElement>("popup-overlay");
        popupTitle = root.Q<Label>("popup-title");
        popupMessage = root.Q<Label>("popup-message");
        confirmButton = root.Q<Button>("popup-confirm");
        cancelButton = root.Q<Button>("popup-cancel"); // ✅ NEW

        if (popupOverlay == null)
            Debug.LogError("[Popup] 'popup-overlay' not found in UXML. Check the element name.");
        if (popupTitle == null)
            Debug.LogError("[Popup] 'popup-title' not found in UXML. Check the element name.");
        if (popupMessage == null)
            Debug.LogError("[Popup] 'popup-message' not found in UXML. Check the element name.");
        if (confirmButton == null)
            Debug.LogError("[Popup] 'popup-confirm' not found in UXML. Check the element name.");
        if (cancelButton == null)
            Debug.LogWarning("[Popup] 'popup-cancel' not found — optional but recommended.");

        if (popupOverlay == null || popupTitle == null || popupMessage == null || confirmButton == null)
        {
            Debug.LogError("[Popup] Missing one or more required elements in popup.uxml.");
            return;
        }

        // ✅ Hook cancel button (optional)
        if (cancelButton != null)
        {
            cancelButton.clicked += () =>
            {
                Debug.Log("[Popup] Cancel button clicked — closing popup.");
                FadeOutPopup();
            };
        }

        popupOverlay.style.opacity = 0f;
        popupOverlay.style.display = DisplayStyle.None;

        isInitialized = true;
        Debug.Log("[Popup] PopupManager initialized successfully!");
    }

    private void InitializePresets()
    {
        popupPresets = new Dictionary<PopupType, (string, string, string)>
        {
            { PopupType.pop_error, ("Error", "Something went wrong. Please try again.", "OK") },
            { PopupType.pop_logout, ("Confirm", "Are you sure you want to log out?", "Log Out") },
            { PopupType.pop_reserve, ("Error", "You need an active reservation to use this feature.", "OK") },
            { PopupType.pop_success, ("Success", "Action completed successfully!", "OK") },
            { PopupType.pop_block, ("Error", "You haven't unlocked this content yet!", "OK") }
        };
    }

    public void ShowPopup(PopupType type, Action onConfirm = null, string confirmLabel = null)
    {
        Debug.Log($"[Popup] ShowPopup called for type: {type}");
        Debug.Log($"[Popup] GameObject active: {gameObject.activeInHierarchy}");
        Debug.Log($"[Popup] Is initialized: {isInitialized}");
        
        if (!gameObject.activeInHierarchy)
        {
            Debug.Log("[Popup] GameObject was inactive, activating and queuing popup...");
            pendingPopup = new PendingPopup
            {
                type = type,
                onConfirm = onConfirm,
                confirmLabel = confirmLabel
            };
            gameObject.SetActive(true);
            
            if (gameObject.activeInHierarchy)
            {
                StartCoroutine(ShowPendingPopupAfterActivation());
            }
            return;
        }

        if (!isInitialized || root == null || popupOverlay == null)
        {
            Debug.LogWarning("[Popup] UI not ready yet. Retrying after initialization...");
            StartCoroutine(RetryShowPopup(type, onConfirm, confirmLabel));
            return;
        }

        if (!popupPresets.TryGetValue(type, out var preset))
        {
            Debug.LogWarning("[Popup] Preset not found for type: " + type);
            return;
        }

        popupTitle.text = preset.title;
        popupMessage.text = preset.message;
        confirmButton.text = confirmLabel ?? preset.button;

        popupOverlay.style.display = DisplayStyle.Flex;

        confirmButton.clickable = new Clickable(() =>
        {
            onConfirm?.Invoke();
            FadeOutPopup();
        });

        FadeInPopup();
    }

    private IEnumerator RetryShowPopup(PopupType type, Action onConfirm, string confirmLabel)
    {
        yield return null;
        yield return null;
        
        if (isInitialized)
        {
            ShowPopup(type, onConfirm, confirmLabel);
        }
        else
        {
            Debug.LogError("[Popup] Failed to initialize after retry. Check your UXML element names.");
        }
    }

    private void FadeInPopup()
    {
        confirmButton.SetEnabled(false);
        popupOverlay.style.display = DisplayStyle.Flex;
        popupOverlay.experimental.animation
            .Start(new StyleValues { opacity = 1f }, fadeDuration)
            .Ease(Easing.Linear)
            .OnCompleted(() => confirmButton.SetEnabled(true));
    }

    private void FadeOutPopup()
    {
        confirmButton.SetEnabled(false);
        popupOverlay.experimental.animation
            .Start(new StyleValues { opacity = 0f }, fadeDuration)
            .Ease(Easing.Linear)
            .OnCompleted(() =>
            {
                popupOverlay.style.display = DisplayStyle.None;
            });
    }
}

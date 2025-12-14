using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class IntroController : MonoBehaviour
{
    [Header("Configuration")]
    public float typeSpeed = 0.04f;
    public string nextSceneName = "D_mainScreen";

    private VisualElement root;
    
    // Story Elements
    private VisualElement storyPanel;
    private Label storyTextLabel;
    private Button tapArea;
    private Label tapToContinueHint;

    // Tutorial Elements
    private VisualElement tutorialPanel;
    private VisualElement slideContainer; 
    private Label slideTitle;
    private Label slideDesc;
    private Button nextSlideBtn;
    
    // Data
    private int currentDialogIndex = 0;
    private bool isTyping = false;
    private bool userPressedNext = false; // ✅ New Flag to replace Input.GetMouseButton
    private string currentFullText = "";
    
    private string[] storyLines = new string[]
    {
        "Decades ago, an American journalist named Thomas Hargrove walked these streets...",
        "He saw something unique... a 'Fourth Circle' of defense, built not of stone, but of spirit.",
        "Taal is not just a town. It is the Vatican of the Philippines. A testament to resilience.",
        "But history is fading. The 'Talaan ng Katatagan' has been shattered into fragments.",
        "It is up to you to find the markers, unlock the memories, and rebuild the legacy."
    };

    private int currentSlideIndex = 0;
    private (string, string)[] tutorialSlides = new (string, string)[]
    {
        ("SCAN & DISCOVER", "Use the AR Camera to scan historical markers around Taal. Look for the special plaques!"),
        ("PLAY & UNLOCK", "Uncovering artifacts unlocks unique Minigames. Win them to earn rewards for your collection."),
        ("TRACK & READ", "Visit your Progress Page to view your Personal Museum and read the 'Missing Chronicle'.")
    };

    void OnEnable()
    {
        var uiDoc = GetComponent<UIDocument>();
        if (uiDoc == null) return;
        root = uiDoc.rootVisualElement;

        storyPanel = root.Q<VisualElement>("story-panel");
        storyTextLabel = root.Q<Label>("story-text");
        tapArea = root.Q<Button>("story-tap-area");
        tapToContinueHint = root.Q<Label>("tap-hint");

        tutorialPanel = root.Q<VisualElement>("tutorial-panel");
        slideTitle = root.Q<Label>("slide-title");
        slideDesc = root.Q<Label>("slide-desc");
        nextSlideBtn = root.Q<Button>("next-slide-btn");

        if (tutorialPanel != null) tutorialPanel.style.display = DisplayStyle.None;
        if (storyPanel != null) storyPanel.style.display = DisplayStyle.Flex;

        // ✅ BIND EVENTS
        if (tapArea != null) tapArea.clicked += OnStoryTap;
        if (nextSlideBtn != null) nextSlideBtn.clicked += OnNextSlide;

        StartCoroutine(PlayStorySequence());
    }

    // --- STORY LOGIC ---
    IEnumerator PlayStorySequence()
    {
        currentDialogIndex = 0;
        while (currentDialogIndex < storyLines.Length)
        {
            // Reset trigger
            userPressedNext = false; 

            // Start Typing
            yield return StartCoroutine(TypeWriter(storyLines[currentDialogIndex]));
            
            // ✅ WAIT FOR UI TAP (Instead of Input.GetMouseButton)
            yield return new WaitUntil(() => userPressedNext); 
            
            currentDialogIndex++;
        }

        ShowTutorial();
    }

    IEnumerator TypeWriter(string text)
    {
        isTyping = true;
        currentFullText = text;
        storyTextLabel.text = "";
        if(tapToContinueHint != null) tapToContinueHint.style.opacity = 0;

        foreach (char c in text)
        {
            // Check if user skipped
            if (!isTyping) 
            {
                storyTextLabel.text = currentFullText;
                break; 
            }

            storyTextLabel.text += c;
            yield return new WaitForSeconds(typeSpeed);
        }

        // Finished Typing
        storyTextLabel.text = currentFullText; // Ensure full text is there
        isTyping = false;
        if(tapToContinueHint != null) tapToContinueHint.style.opacity = 1; 
    }

    void OnStoryTap()
    {
        if (isTyping)
        {
            // SKIP TYPING: Set flag so Coroutine breaks loop
            isTyping = false;
            storyTextLabel.text = currentFullText; // Instant finish
            if(tapToContinueHint != null) tapToContinueHint.style.opacity = 1;
        }
        else
        {
            // NEXT DIALOG: Trigger the WaitUntil in PlayStorySequence
            userPressedNext = true;
        }
    }

    // --- TUTORIAL LOGIC ---
    void ShowTutorial()
    {
        storyPanel.style.display = DisplayStyle.None;
        tutorialPanel.style.display = DisplayStyle.Flex;
        currentSlideIndex = 0;
        UpdateSlide();
    }

    void OnNextSlide()
    {
        currentSlideIndex++;
        if (currentSlideIndex >= tutorialSlides.Length)
        {
            FinishIntro();
        }
        else
        {
            UpdateSlide();
        }
    }

    void UpdateSlide()
    {
        var data = tutorialSlides[currentSlideIndex];
        slideTitle.text = data.Item1;
        slideDesc.text = data.Item2;

        if (currentSlideIndex == tutorialSlides.Length - 1)
            nextSlideBtn.text = "LET'S BEGIN";
        else
            nextSlideBtn.text = "NEXT";
    }

    void FinishIntro()
    {
        int userId = PlayerPrefs.GetInt("user_id", 0);
        if (userId != 0)
        {
            PlayerPrefs.SetInt("HasSeenIntro_" + userId, 1);
            PlayerPrefs.Save();
        }

        SceneManager.LoadScene(nextSceneName);
    }
}
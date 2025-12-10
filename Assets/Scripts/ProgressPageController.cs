using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;
using System.Collections.Generic;
using System.Linq; 

public class ProgressPageController : MonoBehaviour
{
    [Header("UI Reference")]
    public UIDocument uiDocument;

    // UI Elements
    private ProgressBar progressBar;
    private Label areaNameLabel;
    private VisualElement artifactDisplay;
    private Label artifactNameLabel; 
    
    private Button nextArtifactBtn, prevArtifactBtn;
    private Button nextAreaBtn, prevAreaBtn;
    private Button backButton; 
    
    // Data
    private List<string> allUnlockedIds = new List<string>();
    private int currentArtifactIndex = 0;
    private int currentAreaIndex = 0;

    // DICTIONARY: Maps Areas to Content
    private Dictionary<string, string[]> areaContentMap = new Dictionary<string, string[]>
    {
        { "Taal Basilica", new[] { "basilica", "basilicajesus", "stoup" } },
        { "Agoncillo Museum", new[] { "MarcelaHouse", "MarcelaVase", "marceladrawer", "MarcelaStitching" } },
        { "Apacible Museum", new[] { "Apaciblehouse", "sumbreroApacible", "LeonApacible" } },
        { "Casa Real", new[] { "CasaReal", "MariaRosa" } },
        { "Taal Market", new[] { "empanadas", "longganisa" } }
    };

    private List<string> areaNames; 
    
    // ✅ CHANGED: This now holds ALL items in the area, not just unlocked ones
    private List<string> currentAreaItems = new List<string>();

    [System.Serializable]
    public class ProgressResponse { public string[] unlocked_ids; public float progress_value; }
    [System.Serializable]
    public class UserPayload { public int user_id; }

    void OnEnable()
    {
        if (uiDocument == null) uiDocument = GetComponent<UIDocument>();
        var root = uiDocument.rootVisualElement;

        // Find Elements
        progressBar = root.Q<ProgressBar>("completion-bar");
        areaNameLabel = root.Q<Label>("area-name");
        artifactDisplay = root.Q<VisualElement>("artifact-display");
        artifactNameLabel = root.Q<Label>("artifact-item-name"); 
        
        nextArtifactBtn = root.Q<Button>("artifact-next");
        prevArtifactBtn = root.Q<Button>("artifact-prev");
        nextAreaBtn = root.Q<Button>("area-next");
        prevAreaBtn = root.Q<Button>("area-prev");
        backButton = root.Q<Button>("backbutton_D");

        // Setup Buttons
        if (nextArtifactBtn != null) nextArtifactBtn.clicked += () => NavigateArtifact(1);
        if (prevArtifactBtn != null) prevArtifactBtn.clicked += () => NavigateArtifact(-1);
        if (nextAreaBtn != null) nextAreaBtn.clicked += () => NavigateArea(1);
        if (prevAreaBtn != null) prevAreaBtn.clicked += () => NavigateArea(-1);
        
        if (backButton != null) backButton.clicked += () => UnityEngine.SceneManagement.SceneManager.LoadScene("D_mainScreen");

        areaNames = new List<string>(areaContentMap.Keys);

        StartCoroutine(LoadProgressData());
    }

    IEnumerator LoadProgressData()
    {
        int userId = PlayerPrefs.GetInt("user_id", 0);
        if (userId == 0) yield break;

        UserPayload payload = new UserPayload { user_id = userId };

        if (APIManager.Instance != null)
        {
            yield return StartCoroutine(APIManager.Instance.PostRequest("/api/mobile-get-progress", payload,
                (json) => {
                    ProgressResponse data = JsonUtility.FromJson<ProgressResponse>(json);
                    allUnlockedIds.Clear();
                    if (data.unlocked_ids != null) allUnlockedIds.AddRange(data.unlocked_ids);
                    UpdateAreaUI();
                },
                (error) => Debug.LogError("Progress Error: " + error)
            ));
        }
    }

    void UpdateAreaUI()
    {
        if (areaNames.Count == 0) return;

        string currentArea = areaNames[currentAreaIndex];
        if (areaNameLabel != null) areaNameLabel.text = currentArea;

        string[] areaItems = areaContentMap[currentArea];
        int totalItems = areaItems.Length;

        // ✅ 1. Fill the carousel list with ALL items (Locked & Unlocked)
        currentAreaItems.Clear();
        currentAreaItems.AddRange(areaItems);

        // ✅ 2. Calculate Progress (Logic remains separate)
        int unlockedCount = 0;
        foreach(string id in areaItems)
        {
            if (allUnlockedIds.Contains(id)) unlockedCount++;
        }

        if (progressBar != null)
        {
            float percent = totalItems > 0 ? ((float)unlockedCount / totalItems) * 100f : 0;
            progressBar.value = percent;
            progressBar.title = Mathf.RoundToInt(percent) + "%";
        }

        currentArtifactIndex = 0;
        ShowArtifact();
    }

    void NavigateArea(int direction)
    {
        currentAreaIndex = (currentAreaIndex + direction + areaNames.Count) % areaNames.Count;
        UpdateAreaUI();
    }

    void NavigateArtifact(int direction)
    {
        if (currentAreaItems.Count == 0) return;
        currentArtifactIndex = (currentArtifactIndex + direction + currentAreaItems.Count) % currentAreaItems.Count;
        ShowArtifact();
    }

    void ShowArtifact()
    {
        if (artifactDisplay == null || artifactNameLabel == null) return;

        if (currentAreaItems.Count == 0) 
        {
            artifactDisplay.style.backgroundImage = null;
            artifactNameLabel.text = "";
            return;
        }

        string itemId = currentAreaItems[currentArtifactIndex];
        
        // ✅ CHECK IF UNLOCKED
        bool isUnlocked = allUnlockedIds.Contains(itemId);

        if (isUnlocked)
        {
            // --- SHOW ITEM ---
            Sprite thumb = Resources.Load<Sprite>($"Thumbnails/{itemId}");
            if (thumb != null)
            {
                artifactDisplay.style.backgroundImage = new StyleBackground(thumb);
                artifactDisplay.style.backgroundSize = new BackgroundSize(BackgroundSizeType.Contain);
                artifactDisplay.style.rotate = new Rotate(new Angle(-90, AngleUnit.Degree)); 
            }
            
            string displayName = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(itemId.Replace("_", " "));
            artifactNameLabel.text = displayName;
            artifactNameLabel.style.color = new StyleColor(new Color(0.33f, 0.07f, 0.1f)); // Normal Text Color
        }
        else
        {
            // --- SHOW LOCKED STATUS ---
            artifactDisplay.style.backgroundImage = null; // Clear image (or set a lock icon here)
            artifactNameLabel.text = "UNOBTAINED";
            artifactNameLabel.style.color = new StyleColor(Color.gray); // Grey Text for locked
        }
    }
}
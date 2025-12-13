using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;
using System.Collections.Generic;
using System.Linq; 
using System.Globalization;

public class ProgressController : MonoBehaviour
{
    [Header("UI Reference")]
    public UIDocument uiDocument;

    // ✅ THE FIX: Accepts raw Texture files (Drag your images here!)
    [Header("⚠️ DRAG ALL IMAGES HERE ⚠️")]
    public List<Texture2D> allThumbnails; 

    private ProgressBar progressBar;
    private Label areaNameLabel;
    private VisualElement artifactDisplay;
    private Label artifactNameLabel; 
    private Button nextArtifactBtn, prevArtifactBtn;
    private Button nextAreaBtn, prevAreaBtn;
    private Button backButton; 

    private Label storyTitle;
    private ScrollView storyList;
    private VisualElement readerPopup;
    private Button readerCloseBtn;
    private Label readerTitle, readerContent;

    private List<string> allUnlockedIds = new List<string>();
    private int currentArtifactIndex = 0;
    private int currentAreaIndex = 0;

    // Internal Cache
    private Dictionary<string, Sprite> spriteLookup = new Dictionary<string, Sprite>();

    private Dictionary<string, string[]> areaContentMap = new Dictionary<string, string[]>
    {
        { "Taal Basilica", new[] { "mrkr_basilica-min", "mrkr_basilica_statue", "basilicastoup" } },
        { "Agoncillo Museum", new[] { "mrkr_agoncillo_flag", "mrkr_agoncillo-min", "mrkr_agoncillo_drawer", "mrkr_agoncillo_vase" } },
        { "Apacible Museum", new[] { "mrkr_apacible2-min", "mrkr_apacible_sumbrero", "mrkr_apacible_leon" } },
        { "Taal Market", new[] { "mrkr_taalmarketplace_empanada", "mrkr_taalmarketplace_longganisa", "mrkr_taalmarketplace2" } },
        { "Casa Real", new[] { "mrkr_casareal-min", "mrkr_real", "mrkr_casereal2-min" } }
    };

    private string GetThumbnailName(string vuforiaID)
    {
        switch (vuforiaID)
        {
            case "mrkr_basilica-min":       return "T_BAS_Basilica";
            case "mrkr_basilica_statue":    return "T_BAS_Jesus";
            case "basilicastoup":           return "T_BAS_Stoup";
            case "mrkr_agoncillo_flag":     return "T_MAR_Sewing";
            case "mrkr_agoncillo-min":      return "T_MAR_House";
            case "mrkr_agoncillo_drawer":   return "T_MAR_Drawer";
            case "mrkr_agoncillo_vase":     return "T_MAR_Vase";
            case "mrkr_apacible2-min":      return "T_APA_House";
            case "mrkr_apacible_sumbrero":  return "T_APA_Sumbrero";
            case "mrkr_apacible_leon":      return "T_APA_Leon";
            case "mrkr_taalmarketplace_empanada":   return "T_MKT_Empanadas";
            case "mrkr_taalmarketplace_longganisa": return "T_MKT_Longganisa";
            case "mrkr_taalmarketplace2":           return "T_MKT_Scene";
            case "mrkr_casareal-min":       return "T_CAS_CasaReal";
            case "mrkr_real":               return "T_CAS_MariaRosa";
            case "mrkr_casereal2-min":      return "T_CAS_Marker";
            default: return vuforiaID;
        }
    }

    private Dictionary<string, string> retroactiveTriggers = new Dictionary<string, string>
    {
        { "mrkr_agoncillo_flag", "GAME_FLAG" },
        { "mrkr_basilica-min", "GAME_TRIVIA" },
        { "mrkr_apacible2-min", "GAME_ADVENTURE" },
        { "mrkr_casareal-min", "GAME_RESTORE" },
        { "mrkr_taalmarketplace_empanada", "GAME_FLAVORS" }
    };

    private List<string> areaNames; 
    private List<string> currentAreaItems = new List<string>();

    [System.Serializable] public class ProgressResponse { public string[] unlocked_ids; public float progress_value; }
    [System.Serializable] public class UserPayload { public int user_id; }

    void OnEnable()
    {
        // ✅ AUTO-CONVERT TEXTURES TO SPRITES
        spriteLookup.Clear();
        foreach (Texture2D tex in allThumbnails)
        {
            if (tex != null && !spriteLookup.ContainsKey(tex.name.ToUpper()))
            {
                // Create a sprite from the texture
                Sprite newSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                newSprite.name = tex.name; // Keep the T_Name
                
                spriteLookup.Add(tex.name.ToUpper(), newSprite);
            }
        }
        Debug.Log($"✅ Manual Database Ready: Loaded {spriteLookup.Count} thumbnails.");

        if (uiDocument == null) uiDocument = GetComponent<UIDocument>();
        var root = uiDocument.rootVisualElement;

        progressBar = root.Q<ProgressBar>("completion-bar");
        areaNameLabel = root.Q<Label>("area-name");
        artifactDisplay = root.Q<VisualElement>("artifact-display");
        artifactNameLabel = root.Q<Label>("artifact-item-name"); 
        
        nextArtifactBtn = root.Q<Button>("artifact-next");
        prevArtifactBtn = root.Q<Button>("artifact-prev");
        nextAreaBtn = root.Q<Button>("area-next");
        prevAreaBtn = root.Q<Button>("area-prev");
        backButton = root.Q<Button>("backbutton_D");

        storyTitle = root.Q<Label>("story-title");
        storyList = root.Q<ScrollView>("story-list");

        readerPopup = root.Q<VisualElement>("reader-popup");
        readerCloseBtn = root.Q<Button>("reader-close");
        readerTitle = root.Q<Label>("reader-title");
        readerContent = root.Q<Label>("reader-content");

        if (nextArtifactBtn != null) nextArtifactBtn.clicked += () => NavigateArtifact(1);
        if (prevArtifactBtn != null) prevArtifactBtn.clicked += () => NavigateArtifact(-1);
        if (nextAreaBtn != null) nextAreaBtn.clicked += () => NavigateArea(1);
        if (prevAreaBtn != null) prevAreaBtn.clicked += () => NavigateArea(-1);
        if (backButton != null) backButton.clicked += () => UnityEngine.SceneManagement.SceneManager.LoadScene("D_mainScreen");
        if (readerCloseBtn != null) readerCloseBtn.clicked += CloseReader;

        areaNames = new List<string>(areaContentMap.Keys);
        if(areaNames.Count > 0) { currentAreaIndex = 0; UpdateAreaUI(); }

        StartCoroutine(LoadProgressData());
    }

    Sprite FindSprite(string name)
    {
        if (spriteLookup.ContainsKey(name.ToUpper()))
            return spriteLookup[name.ToUpper()];
        return null;
    }

    IEnumerator LoadProgressData()
    {
        int userId = PlayerPrefs.GetInt("user_id", 0);
        if (userId == 0) yield break;

        if (APIManager.Instance != null)
        {
            yield return StartCoroutine(APIManager.Instance.PostRequest("/api/mobile-get-progress", new UserPayload { user_id = userId },
                (json) => {
                    ProgressResponse data = JsonUtility.FromJson<ProgressResponse>(json);
                    allUnlockedIds.Clear();
                    if (data.unlocked_ids != null) 
                    {
                        foreach(string dbId in data.unlocked_ids) allUnlockedIds.Add(dbId); 
                    }
                    if (StoryManager.Instance != null)
                        StoryManager.Instance.RefreshChapterProgress(allUnlockedIds);
                    SyncMinigameUnlocks(allUnlockedIds);
                    UpdateAreaUI();
                },
                (error) => Debug.LogError("Progress Error: " + error)
            ));
        }
    }

    void SyncMinigameUnlocks(List<string> unlockedItems)
    {
        foreach(var trigger in retroactiveTriggers)
        {
            string requiredItem = trigger.Key; 
            string gameToUnlock = trigger.Value; 
            if (unlockedItems.Exists(id => id.ToLower().Contains(requiredItem.ToLower()) || requiredItem.ToLower().Contains(id.ToLower())))
            {
                if (PlayerPrefs.GetInt("HasUnlocked_" + gameToUnlock, 0) == 0)
                {
                    PlayerPrefs.SetInt("HasUnlocked_" + gameToUnlock, 1);
                    string areaPrefix = "MAR";
                    if (gameToUnlock == "GAME_TRIVIA") areaPrefix = "BAS";
                    else if (gameToUnlock == "GAME_ADVENTURE") areaPrefix = "APA";
                    else if (gameToUnlock == "GAME_RESTORE") areaPrefix = "CAS";
                    else if (gameToUnlock == "GAME_FLAVORS") areaPrefix = "MKT";
                    PlayerPrefs.SetInt("HasUnlocked_" + areaPrefix, 1);
                }
            }
        }
        PlayerPrefs.Save();
    }

    void NavigateArea(int direction) 
    { 
        if (areaNames.Count == 0) return;
        currentAreaIndex += direction;
        if (currentAreaIndex >= areaNames.Count) currentAreaIndex = 0;
        if (currentAreaIndex < 0) currentAreaIndex = areaNames.Count - 1;
        UpdateAreaUI(); 
    }

    void NavigateArtifact(int direction) 
    { 
        if (currentAreaItems.Count == 0) return;
        currentArtifactIndex += direction;
        if (currentArtifactIndex >= currentAreaItems.Count) currentArtifactIndex = 0;
        if (currentArtifactIndex < 0) currentArtifactIndex = currentAreaItems.Count - 1;
        ShowArtifact(); 
    }

    void UpdateAreaUI()
    {
        if (areaNames.Count == 0) return;
        string currentArea = areaNames[currentAreaIndex];
        if (areaNameLabel != null) areaNameLabel.text = currentArea;

        string[] areaItems = areaContentMap[currentArea];
        currentAreaItems.Clear();
        currentAreaItems.AddRange(areaItems);
        currentArtifactIndex = 0; 

        int unlockedCount = 0;
        foreach(string id in areaItems)
        {
            if (allUnlockedIds.Exists(unlocked => id.ToLower().Contains(unlocked.ToLower()) || unlocked.ToLower().Contains(id.ToLower())))
                unlockedCount++;
        }

        if (progressBar != null)
        {
            float percent = areaItems.Length > 0 ? ((float)unlockedCount / areaItems.Length) * 100f : 0;
            progressBar.value = percent;
            progressBar.title = Mathf.RoundToInt(percent) + "%";
        }
        
        ShowArtifact();
        UpdateStoryList(currentArea);
    }

    void ShowArtifact()
    {
        if (artifactDisplay == null || artifactNameLabel == null) return;
        if (currentAreaItems.Count == 0) { artifactDisplay.style.backgroundImage = null; artifactNameLabel.text = ""; return; }

        string rawVuforiaID = currentAreaItems[currentArtifactIndex]; 
        
        bool isUnlocked = allUnlockedIds.Exists(unlocked => rawVuforiaID.ToLower().Contains(unlocked.ToLower()) || unlocked.ToLower().Contains(rawVuforiaID.ToLower()));

        if (isUnlocked)
        {
            string targetName = GetThumbnailName(rawVuforiaID);
            Sprite thumb = FindSprite(targetName);
            
            if (thumb != null) {
                artifactDisplay.style.backgroundImage = new StyleBackground(thumb);
                artifactDisplay.style.backgroundSize = new BackgroundSize(BackgroundSizeType.Contain);
                artifactDisplay.style.rotate = new Rotate(new Angle(0, AngleUnit.Degree)); 
            }
            else 
            {
                Debug.LogWarning($"❌ Still Missing: {targetName}. Did you drag it into the inspector?");
            }

            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            string cleanName = rawVuforiaID.Replace("mrkr_", "").Replace("-min", "").Replace("_", " ");
            artifactNameLabel.text = textInfo.ToTitleCase(cleanName);
            artifactNameLabel.style.color = new StyleColor(new Color(0.33f, 0.07f, 0.1f));
        }
        else
        {
            artifactDisplay.style.backgroundImage = null;
            artifactNameLabel.text = "UNOBTAINED";
            artifactNameLabel.style.color = new StyleColor(Color.gray);
        }
    }

    void UpdateStoryList(string areaName)
    {
        if (StoryManager.Instance == null || storyList == null) return;
        var chapter = StoryManager.Instance.chapters.FirstOrDefault(c => c.locationName == areaName);
        if (chapter == null) return;

        if (storyTitle != null) storyTitle.text = chapter.title;
        storyList.Clear();

        foreach (var fragment in chapter.fragments)
        {
            string reqCore = fragment.associatedItemID;
            if(reqCore.Contains("_")) reqCore = reqCore.Split('_')[1];

            bool hasFragment = allUnlockedIds.Exists(id => id.ToLower().Contains(reqCore.ToLower()));

            Button fragBtn = new Button();
            fragBtn.style.marginBottom = 10;
            fragBtn.style.paddingTop = 15;
            fragBtn.style.paddingBottom = 15;
            fragBtn.style.paddingLeft = 20;
            fragBtn.style.paddingRight = 20;
            fragBtn.style.borderTopLeftRadius = 10;
            fragBtn.style.borderTopRightRadius = 10;
            fragBtn.style.borderBottomLeftRadius = 10;
            fragBtn.style.borderBottomRightRadius = 10;
            fragBtn.style.borderTopWidth = 0;
            fragBtn.style.borderBottomWidth = 0;
            fragBtn.style.borderLeftWidth = 0;
            fragBtn.style.borderRightWidth = 0;
            fragBtn.style.fontSize = 24;
            fragBtn.style.whiteSpace = WhiteSpace.Normal;
            fragBtn.style.unityTextAlign = TextAnchor.MiddleLeft;

            if (hasFragment)
            {
                string preview = fragment.text.Length > 50 ? fragment.text.Substring(0, 47) + "..." : fragment.text;
                fragBtn.text = "📜 " + preview;
                fragBtn.style.backgroundColor = new Color(1f, 1f, 1f, 0.9f);
                fragBtn.style.color = new Color(0.2f, 0.2f, 0.2f);
                fragBtn.clicked += () => OpenReader(chapter.title, fragment.text);
            }
            else
            {
                fragBtn.text = "🔒 Locked Fragment";
                fragBtn.style.backgroundColor = new Color(0.8f, 0.8f, 0.8f, 0.5f);
                fragBtn.style.color = new Color(0.5f, 0.5f, 0.5f);
                fragBtn.SetEnabled(false);
            }
            storyList.Add(fragBtn);
        }
    }

    void OpenReader(string title, string content)
    {
        if (readerPopup != null)
        {
            readerTitle.text = title;
            readerContent.text = content;
            readerPopup.style.display = DisplayStyle.Flex;
        }
    }

    void CloseReader()
    {
        if (readerPopup != null) readerPopup.style.display = DisplayStyle.None;
    }
}
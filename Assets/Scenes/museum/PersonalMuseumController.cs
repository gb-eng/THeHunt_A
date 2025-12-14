using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PersonalMuseumController : MonoBehaviour
{
    [Header("Scene References")]
    public Transform[] slotAnchors; 

    [Header("Global Defaults")]
    public Vector3 defaultScale = Vector3.one;
    public Vector3 defaultRotation = Vector3.zero;

    [Header("UI References")]
    public UIDocument uiDocument;

    // ✅ MANUAL FIX (Keep this list filled!)
    [Header("⚠️ DRAG ALL THUMBNAILS HERE ⚠️")]
    public List<Texture2D> allThumbnails; 

    private VisualElement root;
    private ScrollView carousel;
    private Button backBtn;
    
    // Internal Cache
    private Dictionary<string, Sprite> spriteLookup = new Dictionary<string, Sprite>();

    [System.Serializable] public class ProgressResponse { public string[] unlocked_ids; }
    [System.Serializable] public class UserPayload { public int user_id; }

    void OnEnable()
    {
        if (uiDocument == null) uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null) { Debug.LogError("❌ UI Document is NULL!"); return; }
        root = uiDocument.rootVisualElement;

        // --- 1. PRIORITY FIX: BACK BUTTON ---
        // We bind this FIRST so it works 100% of the time.
        backBtn = root.Q<Button>("backbutton_D");
        if (backBtn != null)
        {
            backBtn.clicked += () => UnityEngine.SceneManagement.SceneManager.LoadScene("D_mainScreen");
        }

        // --- 2. PREPARE IMAGES ---
        spriteLookup.Clear();
        if (allThumbnails != null)
        {
            foreach (Texture2D tex in allThumbnails)
            {
                if (tex != null && !spriteLookup.ContainsKey(tex.name.ToUpper()))
                {
                    Sprite newSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                    newSprite.name = tex.name; 
                    spriteLookup.Add(tex.name.ToUpper(), newSprite);
                }
            }
        }

        // --- 3. START CONTENT LOADING ---
        carousel = root.Q<ScrollView>("carousel");
        if (carousel != null)
        {
            carousel.Clear();
            StartCoroutine(LoadUnlockedContent());
        }
    }

    IEnumerator LoadUnlockedContent()
    {
        int userId = PlayerPrefs.GetInt("user_id", 0);
        if (userId == 0) yield break;

        UserPayload payload = new UserPayload { user_id = userId };

        if (APIManager.Instance != null)
        {
            yield return StartCoroutine(APIManager.Instance.PostRequest("/api/mobile-get-progress", payload,
                (json) => {
                    ProgressResponse data = JsonUtility.FromJson<ProgressResponse>(json);
                    if (data.unlocked_ids != null) PopulateScrollList(data.unlocked_ids);
                },
                (error) => Debug.LogError("Museum Fetch Failed: " + error)
            ));
        }
    }

    private string GetCleanId(string rawId)
    {
        switch (rawId)
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
            case "MKT_Empanadas":           return "T_MKT_Empanadas"; 
            case "MKT_Longganisa":          return "T_MKT_Longganisa";
            case "mrkr_taalmarketplace2":   return "T_MKT_Scene"; 
            case "mrkr_casareal-min":       return "T_CAS_CasaReal";
            case "mrkr_real":               return "T_CAS_MariaRosa";
            case "mrkr_casereal2-min":      return "T_CAS_Marker"; 
            default: return rawId;
        }
    }

    private void PopulateScrollList(string[] unlockedIds)
    {
        carousel.Clear(); 

        foreach (string rawId in unlockedIds)
        {
            string cleanId = GetCleanId(rawId);
            string modelName = cleanId.StartsWith("T_") ? "M_" + cleanId.Substring(2) : "M_" + cleanId;
            
            // Check Model
            if (Resources.Load<GameObject>($"Models/{modelName}") == null) continue; 

            // Check Thumbnail
            Sprite thumb = null;
            if (spriteLookup.ContainsKey(cleanId.ToUpper())) thumb = spriteLookup[cleanId.ToUpper()];

            // --- UI CARD ---
            var itemContainer = new VisualElement();
            itemContainer.AddToClassList("carousel-item"); 
            itemContainer.style.flexShrink = 0; 
            
            if (thumb != null) 
            {
                itemContainer.style.backgroundImage = new StyleBackground(thumb);
                itemContainer.style.backgroundSize = new BackgroundSize(BackgroundSizeType.Contain);
                itemContainer.style.backgroundRepeat = new BackgroundRepeat(Repeat.NoRepeat, Repeat.NoRepeat);
                itemContainer.style.backgroundColor = Color.white;
            }
            else
            {
                // If this turns RED, it means the ID matches, but the image is missing from the Inspector list
                itemContainer.style.backgroundColor = Color.red; 
            }

            var buttonRow = new VisualElement();
            buttonRow.AddToClassList("button-row"); 

            var placeBtn = new Button(() => TryPlaceItem(modelName, itemContainer));
            placeBtn.text = "PLACE";
            placeBtn.AddToClassList("btn-place"); 
            placeBtn.name = "btn-place";

            var removeBtn = new Button(() => RemoveItem(modelName, itemContainer));
            removeBtn.text = "REMOVE";
            removeBtn.AddToClassList("btn-remove"); 
            removeBtn.style.display = DisplayStyle.None; 

            buttonRow.Add(placeBtn);
            buttonRow.Add(removeBtn);
            itemContainer.Add(buttonRow);
            
            if (IsItemAlreadyPlaced(modelName))
            {
                placeBtn.style.display = DisplayStyle.None;
                removeBtn.style.display = DisplayStyle.Flex;
            }

            carousel.Add(itemContainer);
        }
    }

    // --- 3D LOGIC ---
    private void TryPlaceItem(string modelName, VisualElement uiItem)
    {
        if (IsItemAlreadyPlaced(modelName)) return;
        foreach (var slot in slotAnchors)
        {
            if (!SlotHasModel(slot)) { SpawnModel(modelName, slot, uiItem); return; }
        }
    }

    private void SpawnModel(string modelName, Transform slot, VisualElement uiItem)
    {
        GameObject prefab = Resources.Load<GameObject>($"Models/{modelName}");
        if (prefab != null)
        {
            Transform targetParent = slot;
            foreach(Transform child in slot) if (child.name.Contains("Pedestal")) { targetParent = child; break; }

            GameObject instance = Instantiate(prefab, targetParent);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localScale = defaultScale;
            instance.transform.localRotation = Quaternion.Euler(defaultRotation);
            instance.name = modelName;
            instance.tag = "PlacedModel";
            ToggleButtons(uiItem, true);
        }
    }

    private void RemoveItem(string modelName, VisualElement uiItem)
    {
        foreach (var slot in slotAnchors)
        {
            Transform target = slot.Find(modelName);
            if (target == null) foreach(Transform c in slot) { target = c.Find(modelName); if(target!=null) break; }
            if (target != null) { Destroy(target.gameObject); ToggleButtons(uiItem, false); return; }
        }
    }

    private void ToggleButtons(VisualElement uiItem, bool isPlaced)
    {
        var placeBtn = uiItem.Q<Button>(className: "btn-place");
        var removeBtn = uiItem.Q<Button>(className: "btn-remove");
        if(placeBtn!=null) placeBtn.style.display = isPlaced ? DisplayStyle.None : DisplayStyle.Flex;
        if(removeBtn!=null) removeBtn.style.display = isPlaced ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private bool IsItemAlreadyPlaced(string modelName)
    {
        foreach (var slot in slotAnchors)
        {
            if (slot.Find(modelName) != null) return true;
            foreach(Transform child in slot) if (child.Find(modelName) != null) return true;
        }
        return false;
    }

    private bool SlotHasModel(Transform slot)
    {
        foreach (Transform child in slot)
        {
            if (child.CompareTag("PlacedModel")) return true;
            foreach(Transform grandChild in child) if (grandChild.CompareTag("PlacedModel")) return true;
        }
        return false;
    }
}
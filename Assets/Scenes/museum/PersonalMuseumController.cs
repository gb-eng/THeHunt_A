using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System.Globalization;

public class PersonalMuseumController : MonoBehaviour
{
    [Header("Scene References")]
    // Drag Slot1, Slot2, Slot3, Slot4 here
    public Transform[] slotAnchors; 

    [Header("Global Defaults")]
    public Vector3 defaultScale = Vector3.one;
    public Vector3 defaultRotation = Vector3.zero;

    [Header("Per-Item Overrides")]
    // Add items here (e.g. "basilica") to fix their specific size/rotation
    public List<ArtifactAdjustment> modelAdjustments; 

    [System.Serializable]
    public struct ArtifactAdjustment
    {
        public string itemId;         // e.g. "basilica"
        public Vector3 localScale;    // e.g. (0.1, 0.1, 0.1)
        public Vector3 localRotation; // e.g. (0, 180, 0)
    }

    [Header("UI References")]
    public UIDocument uiDocument;

    private VisualElement root;
    private ScrollView carousel;
    
    [System.Serializable]
    public class ProgressResponse { public string[] unlocked_ids; }
    [System.Serializable]
    public class UserPayload { public int user_id; }

    void OnEnable()
    {
        if (uiDocument == null) uiDocument = GetComponent<UIDocument>();
        root = uiDocument.rootVisualElement;
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

    private void PopulateScrollList(string[] unlockedIds)
    {
        carousel.Clear(); 

        foreach (string id in unlockedIds)
        {
            Sprite thumb = Resources.Load<Sprite>($"Thumbnails/{id}");

            // Container for one card
            var itemContainer = new VisualElement();
            itemContainer.AddToClassList("carousel-item");
            
            if (thumb != null) 
            {
                itemContainer.style.backgroundImage = new StyleBackground(thumb);
                itemContainer.style.backgroundSize = new BackgroundSize(BackgroundSizeType.Contain);
                itemContainer.style.backgroundRepeat = new BackgroundRepeat(Repeat.NoRepeat, Repeat.NoRepeat);
            }

            // Button Row
            var buttonRow = new VisualElement();
            buttonRow.AddToClassList("button-row");

            var placeBtn = new Button(() => TryPlaceItem(id, itemContainer));
            placeBtn.text = "PLACE";
            placeBtn.AddToClassList("btn-place");
            placeBtn.name = "btn-place";

            var removeBtn = new Button(() => RemoveItem(id, itemContainer));
            removeBtn.text = "REMOVE";
            removeBtn.AddToClassList("btn-remove");
            removeBtn.name = "btn-remove";
            removeBtn.style.display = DisplayStyle.None;

            buttonRow.Add(placeBtn);
            buttonRow.Add(removeBtn);
            itemContainer.Add(buttonRow);
            
            // Update State if already placed
            if (IsItemAlreadyPlaced(id))
            {
                placeBtn.style.display = DisplayStyle.None;
                removeBtn.style.display = DisplayStyle.Flex;
            }

            carousel.Add(itemContainer);
        }
    }

    // --- 3D LOGIC ---

    private void TryPlaceItem(string itemId, VisualElement uiItem)
    {
        if (IsItemAlreadyPlaced(itemId)) return;

        foreach (var slot in slotAnchors)
        {
            if (!SlotHasModel(slot))
            {
                SpawnModel(itemId, slot, uiItem);
                return;
            }
        }
        Debug.Log("All slots full!");
    }

    private void SpawnModel(string itemId, Transform slot, VisualElement uiItem)
    {
        GameObject prefab = Resources.Load<GameObject>($"Models/{itemId}");
        
        if (prefab != null)
        {
            // ✅ FIX: Find the specific "Pedestal" child (e.g. Pedestal1, Pedestal2)
            Transform targetParent = slot; // Default to slot if nothing found
            
            foreach(Transform child in slot)
            {
                if (child.name.Contains("Pedestal"))
                {
                    targetParent = child;
                    break;
                }
            }

            // Instantiate attached to the Pedestal
            GameObject instance = Instantiate(prefab, targetParent);
            
            // Reset Local Position so it sits ON TOP of the pedestal anchor
            instance.transform.localPosition = Vector3.zero;
            
            // Apply specific overrides from Inspector List
            ArtifactAdjustment adj = modelAdjustments.Find(x => x.itemId == itemId);
            
            if (!string.IsNullOrEmpty(adj.itemId))
            {
                // Use specific settings
                instance.transform.localScale = adj.localScale;
                instance.transform.localRotation = Quaternion.Euler(adj.localRotation);
            }
            else
            {
                // Use defaults
                instance.transform.localScale = defaultScale;
                instance.transform.localRotation = Quaternion.Euler(defaultRotation);
            }
            
            instance.name = itemId;
            instance.tag = "PlacedModel";

            // Update UI
            ToggleButtons(uiItem, true);
        }
        else
        {
            Debug.LogError($"❌ Model not found: Resources/Models/{itemId}");
        }
    }

    private void RemoveItem(string itemId, VisualElement uiItem)
    {
        foreach (var slot in slotAnchors)
        {
            // Search inside Slot OR inside Pedestal
            Transform target = slot.Find(itemId);
            
            // If not found directly, check children (Pedestals)
            if (target == null)
            {
                foreach(Transform child in slot)
                {
                    target = child.Find(itemId);
                    if (target != null) break;
                }
            }

            if (target != null)
            {
                Destroy(target.gameObject);
                ToggleButtons(uiItem, false);
                return;
            }
        }
    }

    private void ToggleButtons(VisualElement uiItem, bool isPlaced)
    {
        var placeBtn = uiItem.Q<Button>("btn-place");
        var removeBtn = uiItem.Q<Button>("btn-remove");

        if (placeBtn != null) placeBtn.style.display = isPlaced ? DisplayStyle.None : DisplayStyle.Flex;
        if (removeBtn != null) removeBtn.style.display = isPlaced ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private bool IsItemAlreadyPlaced(string itemId)
    {
        foreach (var slot in slotAnchors)
        {
            // Check slot direct child
            if (slot.Find(itemId) != null) return true;
            
            // Check pedestal child
            foreach(Transform child in slot)
            {
                if (child.Find(itemId) != null) return true;
            }
        }
        return false;
    }

    private bool SlotHasModel(Transform slot)
    {
        // Check slot direct child
        foreach (Transform child in slot)
        {
            if (child.CompareTag("PlacedModel")) return true;
            
            // Check grandchildren (items inside pedestal)
            foreach(Transform grandChild in child)
            {
                if (grandChild.CompareTag("PlacedModel")) return true;
            }
        }
        return false;
    }
}
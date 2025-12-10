using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;
using System.Collections.Generic;

public class ArtifactCarouselController : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private Transform[] slotAnchors; // Assign Slot1–Slot4 in Inspector

    private List<Sprite> thumbnails = new();
    private int currentIndex = 0;

    private VisualElement display;
    private Button prevButton;
    private Button nextButton;
    private Button selectButton;

    void Start()
    {
        StartCoroutine(WaitForUIDocument());
    }

    IEnumerator WaitForUIDocument()
    {
        yield return new WaitUntil(() => uiDocument != null && uiDocument.rootVisualElement != null);
        InitializeCarousel();
    }

    void InitializeCarousel()
    {
        var root = uiDocument.rootVisualElement;

        display = root.Q<VisualElement>("artifact-display");
        prevButton = root.Q<Button>("artifact-prev");
        nextButton = root.Q<Button>("artifact-next");
        selectButton = root.Q<Button>("artifact-select");

        if (display == null || prevButton == null || nextButton == null || selectButton == null)
        {
            Debug.LogError("UI elements not found. Check UXML names.");
            return;
        }

        Sprite[] loadedSprites = Resources.LoadAll<Sprite>("Thumbnails");
        foreach (var sprite in loadedSprites)
        {
            if (sprite != null)
            {
                thumbnails.Add(sprite);
                Debug.Log($"Loaded sprite: {sprite.name}");
            }
            else
            {
                Debug.LogWarning("Encountered null sprite during load.");
            }
        }

        Debug.Log($"Final sprite count: {thumbnails.Count}");

        UpdateDisplay();

        prevButton.clicked += () => Navigate(-1);
        nextButton.clicked += () => Navigate(1);
        selectButton.clicked += () => PlaceModelForCurrentSprite();
    }

    void Navigate(int direction)
    {
        if (thumbnails.Count == 0) return;

        currentIndex = (currentIndex + direction + thumbnails.Count) % thumbnails.Count;
        UpdateDisplay();
    }

    void UpdateDisplay()
    {
        if (thumbnails.Count == 0 || thumbnails[currentIndex] == null)
        {
            Debug.LogWarning("No sprite to display.");
            return;
        }

        Texture2D texture = thumbnails[currentIndex].texture;
        if (texture == null)
        {
            Debug.LogWarning("Sprite texture is null.");
            return;
        }

        display.style.backgroundImage = new StyleBackground(texture);
        display.style.width = 160;
        display.style.height = 160;
        display.style.backgroundColor = Color.white;
        display.style.display = DisplayStyle.Flex;
        display.style.visibility = Visibility.Visible;

        display.style.backgroundSize = new BackgroundSize(BackgroundSizeType.Contain);
        display.style.backgroundRepeat = new BackgroundRepeat { x = Repeat.NoRepeat, y = Repeat.NoRepeat };

        Debug.Log($"Displaying sprite {currentIndex}: {thumbnails[currentIndex].name}");
    }

    void PlaceModelForCurrentSprite()
    {
        string modelId = thumbnails[currentIndex].name; // Assumes sprite name matches model prefab name
        GameObject prefab = Resources.Load<GameObject>($"Models/{modelId}");

        if (prefab == null)
        {
            Debug.LogWarning($"Model '{modelId}' not found in Resources/Models.");
            return;
        }

        foreach (Transform slot in slotAnchors)
        {
            Transform pedestal = slot.childCount > 0 ? slot.GetChild(0) : null;
            if (pedestal == null) continue;

            bool hasModel = false;
            foreach (Transform child in pedestal)
            {
                if (child.CompareTag("PlacedModel"))
                {
                    hasModel = true;
                    break;
                }
            }

            if (!hasModel)
            {
                GameObject instance = Instantiate(prefab, pedestal);
                instance.transform.localPosition = Vector3.zero;
                instance.transform.localRotation = Quaternion.identity;
                instance.transform.localScale = Vector3.one * 0.5f; // Adjust scale as needed
                instance.tag = "PlacedModel";

                Debug.Log($"Placed model '{modelId}' in {pedestal.name}");
                return;
            }
        }

        Debug.Log("All pedestals are filled.");
    }
}
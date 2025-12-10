using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SewingPoint : MonoBehaviour, IPointerClickHandler
{
    [Header("Pin visuals")]
    public Image pinImage;
    public Image sewedPinImage;
    public Sprite perfectSewSprite;
    public Sprite normalSewSprite;

    private FlagController flagController;
    private SewGameManager gameManager;
    private int pointIndex;
    private bool isSewed = false;
    private bool waitingForNeedle = false;
    private bool inPerfectTiming = false;

    public void Initialize(FlagController controller, SewGameManager manager, int index)
    {
        flagController = controller;
        gameManager = manager ?? SewGameManager.Instance;
        pointIndex = index;
        isSewed = false;
        waitingForNeedle = false;
        inPerfectTiming = false;

        if (pinImage)
        {
            pinImage.enabled = true;
            pinImage.raycastTarget = true;
        }
        if (sewedPinImage) sewedPinImage.enabled = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!gameObject.activeInHierarchy) return;
        if (isSewed) return;
        if (waitingForNeedle) return;
        if (flagController == null) return;

        bool accepted = flagController.RequestSew(this);
        if (accepted)
        {
            waitingForNeedle = true;
        }
    }

    public void ConfirmSew()
    {
        if (isSewed) return;
        isSewed = true;
        waitingForNeedle = false;

        if (pinImage) pinImage.enabled = false;
        if (sewedPinImage)
        {
            sewedPinImage.enabled = true;
            if (perfectSewSprite != null && normalSewSprite != null)
                sewedPinImage.sprite = inPerfectTiming ? perfectSewSprite : normalSewSprite;
        }

        gameManager = gameManager ?? SewGameManager.Instance;

        if (gameManager != null)
        {
            gameManager.AddScore(inPerfectTiming ? 20 : 10);
        }
        else
        {
            Debug.LogError("[SewingPoint] gameManager missing when confirming sew.");
        }

        if (flagController != null)
        {
            flagController.PointSewed(pointIndex);
        }

        Destroy(gameObject, 0.25f);
    }

    public void SetPerfectTiming(bool isPerfect) => inPerfectTiming = isPerfect;

    private void OnDestroy()
    {
        if (flagController != null)
            flagController.OnSewingPointDestroyed(this.gameObject);
    }
}
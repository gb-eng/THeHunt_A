using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class BadSewingPoint : MonoBehaviour, IPointerClickHandler
{
    [Header("Visual")]
    public Image badPinImage;
    public Color hitColor = Color.red;

    private FlagController flagController;
    private SewGameManager gameManager;
    private bool isClicked = false;
    private bool lifetimeExpired = false;
    private Coroutine lifetimeCoroutine = null;

    public float autoLifetime = 2f;

    void Awake()
    {
        if (badPinImage == null)
        {
            badPinImage = GetComponent<Image>();
        }

        if (badPinImage != null)
        {
            badPinImage.raycastTarget = true;
            Debug.Log($"[BadSewingPoint] Raycast enabled on {gameObject.name}");
        }
        else
        {
            Debug.LogError($"[BadSewingPoint] NO IMAGE on {gameObject.name}");
        }
    }

    public void Initialize(FlagController controller, SewGameManager manager)
    {
        flagController = controller;
        gameManager = manager;
        isClicked = false;
        lifetimeExpired = false;

        if (gameManager == null)
        {
            gameManager = SewGameManager.Instance;
            if (gameManager == null)
                Debug.LogWarning("[BadSewingPoint] GameManager NULL in Initialize, trying to find one.");
        }

        Debug.Log($"[BadSewingPoint] Initialized on {gameObject.name} (GM assigned: {gameManager != null})");

        if (lifetimeCoroutine != null) StopCoroutine(lifetimeCoroutine);
        lifetimeCoroutine = StartCoroutine(AutoLifetimeCoroutine());
    }

    private IEnumerator AutoLifetimeCoroutine()
    {
        float elapsed = 0f;
        while (elapsed < autoLifetime)
        {
            if (isClicked)
            {
                Debug.Log($"[BadSewingPoint] Lifetime cancelled because it was clicked: {gameObject.name}");
                yield break;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        lifetimeExpired = true;
        Debug.Log($"[BadSewingPoint] Auto lifetime expired for {gameObject.name} — destroying.");
        Destroy(gameObject);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isClicked) return;
        if (!gameObject.activeInHierarchy) return;

        isClicked = true;
        Debug.Log($"[BadSewingPoint] BAD PIN CLICKED: {gameObject.name}");

        if (badPinImage)
        {
            badPinImage.color = hitColor;
        }

        if (lifetimeCoroutine != null)
        {
            StopCoroutine(lifetimeCoroutine);
            lifetimeCoroutine = null;
        }

        if (flagController != null)
        {
            bool animationStarted = flagController.RequestBadSew(this);
            if (animationStarted)
            {
                Debug.Log("[BadSewingPoint] Animation started via FlagController; will wait for ConfirmBadSew.");
                return;
            }
        }

        Debug.Log("[BadSewingPoint] No animation available - losing life now.");
        LoseLifeNow();
    }

    private void LoseLifeNow()
    {
        gameManager = gameManager ?? SewGameManager.Instance;

        if (gameManager != null)
        {
            Debug.Log("[BadSewingPoint] Calling LoseLife()");
            gameManager.LoseLife();
        }
        else
        {
            Debug.LogError("[BadSewingPoint] NO GAMEMANAGER FOUND!");
        }

        Destroy(gameObject, 0.1f);
    }

    public void ConfirmBadSew()
    {
        if (!isClicked)
        {
            Debug.LogWarning("[BadSewingPoint] ConfirmBadSew called but isClicked is false, setting to true.");
            isClicked = true;
        }

        Debug.Log("[BadSewingPoint] ConfirmBadSew called after animation.");
        LoseLifeNow();
    }

    private void OnDestroy()
    {
        if (flagController != null)
            flagController.OnBadPinDestroyed(this.gameObject);
    }
}
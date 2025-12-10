using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class FlagInfo
{
    public Sprite flagSprite;
    public string countryName;
    [TextArea] public string description;
    public string adoptionDate;
}

public class FlagController : MonoBehaviour
{
    [Header("Flag Settings")]
    public FlagInfo[] flagInfos;
    public Image flagDisplay;
    public TextMeshProUGUI countryNameText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI adoptionDateText;
    public TextMeshProUGUI CountdownText;

    [Header("Sewing Settings")]
    public GameObject sewingPointPrefab;
    public GameObject badSewingPointPrefab;
    public Transform sewingPointsParent;
    public int pointsPerFlag = 5;
    public float spawnPadding = 20f;

    [Header("Bad Pin Spawning - Fruit Ninja Style")]
    public float badPinSpawnInterval = 1.5f;
    public float badPinLifetime = 2f;
    public int maxBadPinsAtOnce = 2;

    [Header("Needle Settings")]
    public RectTransform needleImage;
    public float needleMoveSpeed = 5f;

    [Header("Needle Animation")]
    public float idleSwingAngle = 8f;
    public float idleSwingSpeed = 2f;
    public float moveSwingAngle = 14f;
    public float moveSwingSpeed = 6f;
    public float needleTapDepth = 24f;
    public float needleTapDuration = 0.16f;
    public float needleTiltAngle = 18f;
    public float needleBounceHeight = 8f;
    public float needleBounceDuration = 0.12f;

    [Header("Crack Overlay")]
    public Sprite crackTexture;
    public float crackFadeSpeed = 0.5f;

    [Header("Description Display")]
    public float descriptionDisplayTime = 10f;

    [Header("Debug / Flow")]
    public int currentFlagIndex = 0;

    private List<GameObject> currentSewingPoints = new List<GameObject>();
    private List<GameObject> currentBadPoints = new List<GameObject>();
    private SewGameManager gameManager;
    private bool isMoving = false;
    private Coroutine needleCoroutine = null;
    private Coroutine badPinSpawner = null;
    private int goodPointsSewed = 0;
    private Image crackOverlayImage;
    private bool isSpawningBadPins = false;

    private void Start()
    {
        gameManager = SewGameManager.Instance;
        CreateCrackOverlay();

        if (gameManager == null)
        {
            Debug.LogError("GameManager not found!");
        }
    }

    private void Update()
    {
        if (needleImage != null && !isMoving)
        {
            float swing = Mathf.Sin(Time.time * idleSwingSpeed) * idleSwingAngle;
            needleImage.localRotation = Quaternion.Euler(0f, 0f, swing);
        }
    }

    private void CreateCrackOverlay()
    {
        if (flagDisplay == null) return;

        GameObject crackObj = new GameObject("CrackOverlay");
        crackObj.transform.SetParent(flagDisplay.transform, false);

        RectTransform rectT = crackObj.AddComponent<RectTransform>();
        rectT.anchorMin = Vector2.zero;
        rectT.anchorMax = Vector2.one;
        rectT.offsetMin = Vector2.zero;
        rectT.offsetMax = Vector2.zero;

        crackOverlayImage = crackObj.AddComponent<Image>();

        if (crackTexture != null)
        {
            crackOverlayImage.sprite = crackTexture;
        }
        else
        {
            crackOverlayImage.color = new Color(0, 0, 0, 0.3f);
        }

        crackOverlayImage.raycastTarget = false;
        crackObj.SetActive(false);
    }

    public void StartNewFlag()
    {
        // Ensure the SewGameManager exists and game is active
        gameManager = SewGameManager.Instance;
        if (gameManager == null)
        {
            Debug.LogError("FlagController: SewGameManager.Instance is null. Aborting StartNewFlag.");
            return;
        }

        if (!gameManagerIsActive())
        {
            // If for some reason the game isn't active, start it.
            Debug.Log("[FlagController] Game not active — auto-starting game.");
            gameManager.StartGame();
        }

        ClearCurrentFlag();
        StopBadPinSpawning();

        if (flagInfos == null || flagInfos.Length == 0)
        {
            Debug.LogWarning("FlagController: no flags assigned.");
            return;
        }

        if (currentFlagIndex < 0 || currentFlagIndex >= flagInfos.Length)
        {
            Debug.LogWarning("FlagController: currentFlagIndex out of range (" + currentFlagIndex + ").");
            return;
        }

        if (!DisplayNextFlag())
        {
            Debug.LogWarning("StartNewFlag: failed to display next flag.");
            return;
        }

        CreateSewingPoints();
        StartBadPinSpawning();
        // DO NOT increment currentFlagIndex here; the GameManager advances the index on completion
    }

    private bool gameManagerIsActive()
    {
        return gameManager != null && SewGameManager.Instance != null;
    }

    private bool DisplayNextFlag()
    {
        if (flagInfos == null || flagInfos.Length == 0) return false;
        if (currentFlagIndex < 0 || currentFlagIndex >= flagInfos.Length) return false;

        var f = flagInfos[currentFlagIndex];
        if (f.flagSprite == null)
        {
            Debug.LogError("FlagController: missing sprite at index " + currentFlagIndex);
            return false;
        }

        if (countryNameText) countryNameText.text = f.countryName;
        if (descriptionText) descriptionText.text = "";
        if (adoptionDateText) adoptionDateText.text = "";

        if (flagDisplay)
        {
            flagDisplay.sprite = f.flagSprite;
            flagDisplay.color = Color.white;
        }

        if (crackOverlayImage != null)
        {
            crackOverlayImage.gameObject.SetActive(true);
            crackOverlayImage.color = new Color(crackOverlayImage.color.r, crackOverlayImage.color.g, crackOverlayImage.color.b, crackTexture != null ? 1f : 0.5f);
        }

        goodPointsSewed = 0;
        return true;
    }

    private void CreateSewingPoints()
    {
        if (flagDisplay == null || sewingPointPrefab == null || sewingPointsParent == null) return;

        RectTransform flagRect = flagDisplay.rectTransform;
        currentSewingPoints.Clear();

        for (int i = 0; i < pointsPerFlag; i++)
        {
            Vector2 localPos = GetRandomEdgePosition(flagRect);
            GameObject go = Instantiate(sewingPointPrefab, sewingPointsParent);
            go.transform.localScale = Vector3.one;
            (go.transform as RectTransform).anchoredPosition = localPos;

            SewingPoint sp = go.GetComponent<SewingPoint>();
            if (sp != null)
            {
                sp.Initialize(this, gameManager, i);
                currentSewingPoints.Add(go);
                go.SetActive(i == 0);
            }
        }
    }

    private Vector2 GetRandomEdgePosition(RectTransform rect)
    {
        Vector2 size = rect.rect.size;
        int edge = Random.Range(0, 4);
        float x = 0f, y = 0f;

        switch (edge)
        {
            case 0:
                x = Random.Range(-size.x / 2 + spawnPadding, size.x / 2 - spawnPadding);
                y = size.y / 2 - spawnPadding;
                break;
            case 1:
                x = Random.Range(-size.x / 2 + spawnPadding, size.x / 2 - spawnPadding);
                y = -size.y / 2 + spawnPadding;
                break;
            case 2:
                x = -size.x / 2 + spawnPadding;
                y = Random.Range(-size.y / 2 + spawnPadding, size.y / 2 - spawnPadding);
                break;
            case 3:
                x = size.x / 2 - spawnPadding;
                y = Random.Range(-size.y / 2 + spawnPadding, size.y / 2 - spawnPadding);
                break;
        }
        return new Vector2(x, y);
    }

    // Bad pin spawner control
    public void StartBadPinSpawning()
    {
        if (badPinSpawner != null) StopCoroutine(badPinSpawner);
        isSpawningBadPins = true;
        badPinSpawner = StartCoroutine(BadPinSpawnerRoutine());
    }

    public void StopBadPinSpawning()
    {
        isSpawningBadPins = false;
        if (badPinSpawner != null)
        {
            StopCoroutine(badPinSpawner);
            badPinSpawner = null;
        }
    }

    private IEnumerator BadPinSpawnerRoutine()
    {
        while (isSpawningBadPins)
        {
            yield return new WaitForSeconds(badPinSpawnInterval);

            // prune nulls
            currentBadPoints.RemoveAll(item => item == null);

            int activeBadPins = 0;
            foreach (var bp in currentBadPoints)
            {
                if (bp != null) activeBadPins++;
            }

            if (activeBadPins < maxBadPinsAtOnce)
            {
                SpawnBadPin();
            }
        }
    }

    private void SpawnBadPin()
    {
        if (badSewingPointPrefab == null || sewingPointsParent == null) return;
        if (flagDisplay == null) return;

        RectTransform flagRect = flagDisplay.rectTransform;
        Vector2 localPos = GetRandomEdgePosition(flagRect);

        GameObject go = Instantiate(badSewingPointPrefab, sewingPointsParent);
        go.transform.localScale = Vector3.one;
        (go.transform as RectTransform).anchoredPosition = localPos;

        BadSewingPoint bsp = go.GetComponent<BadSewingPoint>();
        if (bsp != null)
        {
            bsp.Initialize(this, gameManager);
            bsp.autoLifetime = badPinLifetime;
            currentBadPoints.Add(go);
        }

        go.SetActive(true);
        // DO NOT call Destroy(go, badPinLifetime) here
    }

    public void ClearCurrentFlag()
    {
        foreach (var point in currentSewingPoints)
        {
            if (point != null) Destroy(point);
        }
        currentSewingPoints.Clear();

        foreach (var point in currentBadPoints)
        {
            if (point != null) Destroy(point);
        }
        currentBadPoints.Clear();
    }

    public bool RequestSew(SewingPoint point)
    {
        if (point == null) return false;
        if (isMoving) return false;

        RectTransform r = point.transform as RectTransform;
        Vector2 target = r != null ? r.anchoredPosition : Vector2.zero;

        if (needleCoroutine != null) StopCoroutine(needleCoroutine);
        needleCoroutine = StartCoroutine(MoveNeedleAndTapRoutine(target, point));
        return true;
    }

    public bool RequestBadSew(BadSewingPoint point)
    {
        if (point == null) return false;
        if (isMoving) return false;

        RectTransform r = point.transform as RectTransform;
        Vector2 target = r != null ? r.anchoredPosition : Vector2.zero;

        if (needleCoroutine != null) StopCoroutine(needleCoroutine);
        needleCoroutine = StartCoroutine(MoveNeedleAndTapBadRoutine(target, point));
        return true;
    }

    public void PointSewed(int pointIndex)
    {
        goodPointsSewed++;

        if (crackOverlayImage != null)
        {
            float targetAlpha = 1f - ((float)goodPointsSewed / pointsPerFlag);
            if (crackTexture == null) targetAlpha *= 0.5f;
            StartCoroutine(FadeCracks(targetAlpha));
        }

        if (pointIndex + 1 < currentSewingPoints.Count)
        {
            var next = currentSewingPoints[pointIndex + 1];
            if (next) next.SetActive(true);
        }
        else
        {
            StopBadPinSpawning();
            ShowFlagDescription();
            StartCoroutine(WaitAndCompleteFlag());
        }
    }

    private IEnumerator FadeCracks(float targetAlpha)
    {
        if (crackOverlayImage == null) yield break;

        Color startColor = crackOverlayImage.color;
        float elapsed = 0f;

        while (elapsed < crackFadeSpeed)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(startColor.a, targetAlpha, elapsed / crackFadeSpeed);
            crackOverlayImage.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        crackOverlayImage.color = new Color(startColor.r, startColor.g, startColor.b, targetAlpha);

        if (targetAlpha <= 0.05f)
        {
            crackOverlayImage.gameObject.SetActive(false);
        }
    }

    private void ShowFlagDescription()
    {
        if (currentFlagIndex >= 0 && currentFlagIndex < flagInfos.Length)
        {
            var f = flagInfos[currentFlagIndex];

            if (descriptionText)
            {
                descriptionText.text = f.description;
                Debug.Log("Showing description: " + f.description);
            }

            if (adoptionDateText)
            {
                adoptionDateText.text = "Adopted: " + f.adoptionDate;
            }
        }
    }

    private IEnumerator MoveNeedleAndTapRoutine(Vector2 targetAnchoredPos, SewingPoint point)
    {
        if (needleImage == null)
        {
            point.ConfirmSew();
            yield break;
        }

        isMoving = true;

        Vector2 start = needleImage.anchoredPosition;
        Vector2 approach = new Vector2(targetAnchoredPos.x, targetAnchoredPos.y + 20f);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * needleMoveSpeed;
            float s = Mathf.SmoothStep(0f, 1f, t);
            needleImage.anchoredPosition = Vector2.Lerp(start, approach, s);
            float swing = Mathf.Sin(Time.time * moveSwingSpeed) * moveSwingAngle;
            needleImage.localRotation = Quaternion.Euler(0f, 0f, swing);
            yield return null;
        }

        Vector2 downPos = new Vector2(approach.x, approach.y - needleTapDepth);
        float half = Mathf.Max(0.01f, needleTapDuration * 0.5f);
        float elapsed = 0f;
        while (elapsed < half)
        {
            elapsed += Time.deltaTime;
            float u = Mathf.Clamp01(elapsed / half);
            float eased = Mathf.Sin(u * Mathf.PI * 0.5f);
            needleImage.anchoredPosition = Vector2.Lerp(approach, downPos, eased);
            float tilt = -needleTiltAngle * eased;
            float swing = Mathf.Sin(Time.time * moveSwingSpeed * 1.1f) * (moveSwingAngle * 0.6f);
            needleImage.localRotation = Quaternion.Euler(0f, 0f, tilt + swing);
            yield return null;
        }

        yield return new WaitForSeconds(0.03f);

        elapsed = 0f;
        while (elapsed < half)
        {
            elapsed += Time.deltaTime;
            float u = Mathf.Clamp01(elapsed / half);
            float eased = 1f - Mathf.Cos(u * Mathf.PI * 0.5f);
            needleImage.anchoredPosition = Vector2.Lerp(downPos, approach, eased);
            float tilt = -needleTiltAngle * (1f - eased);
            float swing = Mathf.Sin(Time.time * moveSwingSpeed * 0.9f) * (moveSwingAngle * 0.5f);
            needleImage.localRotation = Quaternion.Euler(0f, 0f, tilt + swing);
            yield return null;
        }

        Vector2 bounceTarget = approach + Vector2.up * needleBounceHeight;
        elapsed = 0f;
        while (elapsed < needleBounceDuration)
        {
            elapsed += Time.deltaTime;
            float u = Mathf.Clamp01(elapsed / needleBounceDuration);
            float eased = Mathf.Sin(u * Mathf.PI);
            needleImage.anchoredPosition = Vector2.Lerp(approach, bounceTarget, eased);
            float swing = Mathf.Sin(Time.time * moveSwingSpeed * 0.6f) * (moveSwingAngle * 0.25f);
            needleImage.localRotation = Quaternion.Euler(0f, 0f, swing);
            yield return null;
        }

        t = 0f;
        Quaternion initialRot = needleImage.localRotation;
        float returnSpeed = Mathf.Max(1f, needleMoveSpeed * 0.7f);
        while (t < 1f)
        {
            t += Time.deltaTime * returnSpeed;
            float s = Mathf.SmoothStep(0f, 1f, t);
            needleImage.anchoredPosition = Vector2.Lerp(approach, start, s);
            needleImage.localRotation = Quaternion.Slerp(initialRot, Quaternion.identity, s);
            yield return null;
        }

        point.ConfirmSew();
        isMoving = false;
        needleCoroutine = null;
    }

    private IEnumerator MoveNeedleAndTapBadRoutine(Vector2 targetAnchoredPos, BadSewingPoint point)
    {
        if (needleImage == null)
        {
            point.ConfirmBadSew();
            yield break;
        }

        isMoving = true;
        Vector2 start = needleImage.anchoredPosition;
        Vector2 approach = new Vector2(targetAnchoredPos.x, targetAnchoredPos.y + 20f);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * needleMoveSpeed;
            float s = Mathf.SmoothStep(0f, 1f, t);
            needleImage.anchoredPosition = Vector2.Lerp(start, approach, s);
            yield return null;
        }

        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * (needleMoveSpeed * 1.5f);
            float s = Mathf.SmoothStep(0f, 1f, t);
            needleImage.anchoredPosition = Vector2.Lerp(approach, start, s);
            yield return null;
        }

        point.ConfirmBadSew();
        isMoving = false;
        needleCoroutine = null;
    }

    private IEnumerator WaitAndCompleteFlag()
    {
        Debug.Log("📖 === DESCRIPTION DISPLAY STARTED ===");

        // Ensure we have a valid reference to the game manager
        gameManager = SewGameManager.Instance;
        if (gameManager == null)
        {
            Debug.LogError("❌ GameManager is null, cannot pause timer!");
        }

        // Pause the timer (safe even if called multiple times)
        if (gameManager != null)
        {
            Debug.Log("⏸️ Calling PauseTimer()");
            gameManager.PauseTimer();
        }
        else
        {
            Debug.LogError("❌ GameManager is null, cannot pause timer!");
        }

        float remaining = descriptionDisplayTime;
        while (remaining > 0f)
        {
            if (CountdownText != null)
            {
                CountdownText.text = "Reading time: " + Mathf.CeilToInt(remaining) + "s";
            }

            yield return new WaitForSecondsRealtime(1f);
            remaining -= 1f;
        }

if (CountdownText != null)
{
    CountdownText.text = "";
}


        Debug.Log("✅ Description reading time complete!");

        if (gameManager != null)
        {
            Debug.Log("▶️ Calling ResumeTimer()");
            gameManager.ResumeTimer();
        }
        else
        {
            Debug.LogError("❌ GameManager is null, cannot resume timer!");
        }

        Debug.Log("➡️ Moving to next flag");
        if (gameManager != null)
        {
            gameManager.FlagCompleted();
        }
        else
        {
            Debug.LogError("❌ GameManager null at conclusion of description; cannot complete flag.");
        }

        Debug.Log("📖 === DESCRIPTION DISPLAY ENDED ===");
    }

    // Called when a bad pin is destroyed (cleanup)
    public void OnBadPinDestroyed(GameObject go)
    {
        if (currentBadPoints.Contains(go)) currentBadPoints.Remove(go);
    }

    public void OnSewingPointDestroyed(GameObject go)
    {
        if (currentSewingPoints.Contains(go)) currentSewingPoints.Remove(go);
    }



}
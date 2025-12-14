using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class HuntMaster : MonoBehaviour
{
    public static HuntMaster Instance { get; private set; }

    [Header("Card Setup")]
    public GameObject cardPrefab;
    public Transform cardGrid; 
    public Sprite cardBackSprite;
    public Sprite[] batch1CardFaces; 
    public Sprite[] batch2CardFaces; 
    
    [Header("Layout Settings")]
    public Vector2 cardSize = new Vector2(350, 450); 
    public Vector2 spacing = new Vector2(50, 50);
    
    [Header("Game Settings")]
    public float flipBackDelay = 1f;
    public int currentBatch = 1; 
    
    [Header("UI")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI batchText;
    public GameObject winPanel;
    public GameObject losePanel;
    public Button nextBatchButton;
    public TextMeshProUGUI finalScoreText; 

    [Header("Popup System")]
    public script_popup popupManager; 

    private List<Card> allCards = new List<Card>();
    private Card firstFlippedCard;
    private Card secondFlippedCard;
    private int matchesFound = 0;
    private int score = 0;
    private float timeRemaining = 180f;
    private bool isChecking = false;
    private bool gameActive = false;
    
    [System.Serializable]
    public class ScorePayload { public int user_id; public string game_id; public int score; }
    [System.Serializable]
    public class UnlockPayload { public int user_id; public string marker_id; }

    void Awake() { Instance = this; }

    void Start()
    {
        if (winPanel) winPanel.SetActive(false);
        if (losePanel) losePanel.SetActive(false);
        if (nextBatchButton) nextBatchButton.gameObject.SetActive(false);
        
        if (popupManager == null) 
        {
            popupManager = FindFirstObjectByType<script_popup>();
            if (popupManager == null) Debug.LogWarning("⚠️ script_popup not found in scene!");
        }
        
        StartBatch(1);
    }
    
    void Update()
    {
        if (gameActive)
        {
            timeRemaining -= Time.deltaTime;
            UpdateTimerUI();
            if (timeRemaining <= 0) GameOver(false);
        }
    }
    
    public void StartBatch(int batchNumber)
    {
        currentBatch = batchNumber;
        matchesFound = 0;
        gameActive = true;
        ClearCards();
        CreateCards();
        UpdateBatchUI();
        UpdateScoreUI();
    }
    
    void CreateCards()
    {
        Sprite[] selectedBatch = (currentBatch == 1) ? batch1CardFaces : batch2CardFaces;
        if (selectedBatch == null || selectedBatch.Length == 0) return;

        List<int> cardIDs = new List<int>();
        for (int i = 0; i < 5; i++) { cardIDs.Add(i); cardIDs.Add(i); }

        for (int i = 0; i < cardIDs.Count; i++)
        {
            int temp = cardIDs[i];
            int randomIndex = Random.Range(i, cardIDs.Count);
            cardIDs[i] = cardIDs[randomIndex];
            cardIDs[randomIndex] = temp;
        }

        int[] rowStructure = { 3, 3, 3, 1 }; 
        int currentCardIndex = 0;
        float totalHeight = (rowStructure.Length * cardSize.y) + ((rowStructure.Length - 1) * spacing.y);
        float startY = totalHeight / 2f - (cardSize.y / 2f);

        for (int rowIndex = 0; rowIndex < rowStructure.Length; rowIndex++)
        {
            int countInRow = rowStructure[rowIndex];
            float rowWidth = (countInRow * cardSize.x) + ((countInRow - 1) * spacing.x);
            float startX = -(rowWidth / 2f) + (cardSize.x / 2f);
            float yPos = startY - (rowIndex * (cardSize.y + spacing.y));

            for (int colIndex = 0; colIndex < countInRow; colIndex++)
            {
                if (currentCardIndex >= cardIDs.Count) break;
                int id = cardIDs[currentCardIndex];
                
                GameObject cardObj = Instantiate(cardPrefab, cardGrid);
                RectTransform rt = cardObj.GetComponent<RectTransform>();
                rt.anchoredPosition = new Vector2(startX + (colIndex * (cardSize.x + spacing.x)), yPos); 
                
                Card card = cardObj.GetComponent<Card>();
                if (card != null && id < selectedBatch.Length)
                {
                    card.SetupCard(id, selectedBatch[id], cardBackSprite);
                    allCards.Add(card);
                }
                currentCardIndex++;
            }
        }
    }
    
    void ClearCards()
    {
        foreach (Transform child in cardGrid) Destroy(child.gameObject);
        allCards.Clear();
    }
    
    public bool CanFlipCard() { return !isChecking && gameActive; }
    
    public void CardFlipped(Card card)
    {
        if (firstFlippedCard == null)
        {
            firstFlippedCard = card;
        }
        else if (secondFlippedCard == null && card != firstFlippedCard)
        {
            secondFlippedCard = card;
            StartCoroutine(CheckMatch());
        }
    }
    
    IEnumerator CheckMatch()
    {
        isChecking = true;
        yield return new WaitForSeconds(flipBackDelay);
        
        if (firstFlippedCard.GetCardID() == secondFlippedCard.GetCardID())
        {
            firstFlippedCard.SetMatched();
            secondFlippedCard.SetMatched();
            matchesFound++;
            score += 100;
            UpdateScoreUI();
            if (matchesFound >= 5) BatchCompleted();
        }
        else
        {
            score = Mathf.Max(0, score - 10);
            UpdateScoreUI();
            firstFlippedCard.FlipBack();
            secondFlippedCard.FlipBack();
        }
        firstFlippedCard = null;
        secondFlippedCard = null;
        isChecking = false;
    }
    
    void BatchCompleted()
    {
        gameActive = false;
        score += 500;
        UpdateScoreUI();
        if (currentBatch == 1 && nextBatchButton) nextBatchButton.gameObject.SetActive(true);
        else GameOver(true);
    }
    
    public void LoadNextBatch()
    {
        if (nextBatchButton) nextBatchButton.gameObject.SetActive(false);
        StartBatch(2);
    }
    
    void GameOver(bool won)
    {
        gameActive = false;
        if (won)
        {
            if (winPanel) winPanel.SetActive(true);
            if (finalScoreText) finalScoreText.text = "Score: " + score;
            
            int userId = PlayerPrefs.GetInt("user_id", 0);
            if(userId != 0) 
            {
                StartCoroutine(SubmitScore(userId));
                CheckAndUnlockRewards(userId); // ✅ Checks score logic & constructs message
            }
        }
        else
        {
            if (losePanel) losePanel.SetActive(true);
        }
    }

    void CheckAndUnlockRewards(int userId)
    {
        List<string> newlyUnlocked = new List<string>();

        // 1. EMPANADAS (Condition: Win)
        if (PlayerPrefs.GetInt("HasUnlocked_MKT_Empanadas", 0) == 0)
        {
            StartCoroutine(UnlockReward(userId, "MKT_Empanadas"));
            newlyUnlocked.Add("Empanadas");
            PlayerPrefs.SetInt("HasUnlocked_MKT_Empanadas", 1);
        }

        // 2. LONGGANISA (Condition: Win + Score >= 1500)
        if (score >= 1500)
        {
            if (PlayerPrefs.GetInt("HasUnlocked_MKT_Longganisa", 0) == 0)
            {
                StartCoroutine(UnlockReward(userId, "MKT_Longganisa"));
                newlyUnlocked.Add("Longganisa");
                PlayerPrefs.SetInt("HasUnlocked_MKT_Longganisa", 1);
            }
        }

        PlayerPrefs.Save();

        // 3. SHOW POPUP WITH DETAILS
        if (popupManager != null)
        {
            if (newlyUnlocked.Count > 0)
            {
                string items = string.Join(" & ", newlyUnlocked);
                // ✅ Passes the custom message here
                popupManager.ShowPopup(PopupType.pop_success, null, "Awesome!", "You unlocked: " + items);
            }
            else
            {
                string msg = (score < 1500) ? "Score 1500+ to unlock Longganisa!" : "Great match! No new rewards.";
                popupManager.ShowPopup(PopupType.pop_success, null, "OK", msg);
            }
        }
    }

    IEnumerator SubmitScore(int userId)
    {
        ScorePayload payload = new ScorePayload { user_id = userId, game_id = "matching_cards", score = score };
        if (APIManager.Instance != null)
        {
            yield return StartCoroutine(APIManager.Instance.PostRequest("/api/mobile-score", payload,
                (res) => Debug.Log("✅ Card Score Submitted!"),
                (err) => Debug.LogError("❌ Card Score Error: " + err)
            ));
        }
    }

    IEnumerator UnlockReward(int userId, string itemId)
    {
        UnlockPayload payload = new UnlockPayload { user_id = userId, marker_id = itemId };
        if (APIManager.Instance != null)
        {
            yield return StartCoroutine(APIManager.Instance.PostRequest("/api/mobile-unlock", payload,
                (res) => Debug.Log($"✅ Reward Unlocked: {itemId}"),
                (err) => Debug.LogError($"❌ Unlock Failed for {itemId}: {err}")
            ));
        }
    }
    
    public void RestartGame()
    {
        timeRemaining = 180f;
        score = 0;
        if (winPanel) winPanel.SetActive(false);
        if (losePanel) losePanel.SetActive(false);
        if (nextBatchButton) nextBatchButton.gameObject.SetActive(false);
        StartBatch(1);
    }
    
    void UpdateScoreUI() { if (scoreText) scoreText.text = "Score: " + score; }
    void UpdateTimerUI()
    {
        if (timerText)
        {
            int minutes = Mathf.FloorToInt(timeRemaining / 60);
            int seconds = Mathf.FloorToInt(timeRemaining % 60);
            timerText.text = string.Format("{0:0}:{1:00}", minutes, seconds);
        }
    }
    void UpdateBatchUI() { if (batchText) batchText.text = "Batch: " + currentBatch; }
}
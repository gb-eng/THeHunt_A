using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class SewGameManager : MonoBehaviour
{
    public static SewGameManager Instance { get; private set; }

    [Header("Game Settings")]
    public int score = 0;
    public int scorePerFlag = 50;
    public float gameTime = 120f;
    public int maxFlags = 10;
    public int startingLives = 3;

    [Header("UI References")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI livesText;
    public Button startButton;
    public GameObject gameOverPanel;
    public TextMeshProUGUI gameOverText;
    public Button restartButton;

    private float timeRemaining;
    private bool isGameActive = false;
    private bool isTimerPaused = false;
    private FlagController flagController;
    private int currentLives;

    // JSON Payload Class for API
    [System.Serializable]
    public class ScorePayload 
    { 
        public int user_id; 
        public string game_id; 
        public int score; 
    }

    private void Awake()
    {
        Instance = this;
        currentLives = startingLives;
    }

    void Start()
    {
        // UI Initialization
        if (gameOverPanel) gameOverPanel.SetActive(false);
        if (scoreText) scoreText.text = "Score: 0";
        if (timerText) timerText.text = "Time: " + Mathf.CeilToInt(gameTime);
        
        // Button Listeners
        if (startButton != null) startButton.onClick.AddListener(StartGame);
        if (restartButton != null) restartButton.onClick.AddListener(RestartGame);
        if (restartButton) restartButton.gameObject.SetActive(false);
        
        // Find Flag Controller (Fixed Ambiguity Error)
        flagController = UnityEngine.Object.FindFirstObjectByType<FlagController>();
        
        if (livesText) livesText.text = "Lives: " + currentLives;
    }

    void Update()
    {
        if (!isGameActive) return;

        if (!isTimerPaused)
        {
            timeRemaining -= Time.deltaTime;
            UpdateTimerUI();

            if (timeRemaining <= 0f)
            {
                EndGame("Time's Up!");
            }
        }
    }

    // --- Game Flow Control ---

    public void StartGame()
    {
        score = 0;
        timeRemaining = gameTime;
        currentLives = startingLives;
        isGameActive = true;
        isTimerPaused = false;

        UpdateScoreUI();
        UpdateTimerUI();
        UpdateLivesUI();

        if (startButton) startButton.gameObject.SetActive(false);
        if (gameOverPanel) gameOverPanel.SetActive(false);
        if (restartButton) restartButton.gameObject.SetActive(false);

        if (flagController != null)
        {
            flagController.enabled = true;
            flagController.currentFlagIndex = 0;
            flagController.StartNewFlag();
        }
    }

    public void EndGame(string reason)
    {
        if (!isGameActive) return;
        isGameActive = false;

        // Disable Game Logic
        if (flagController != null)
        {
            flagController.StopBadPinSpawning();
            flagController.ClearCurrentFlag();
            flagController.enabled = false;
        }

        // Show UI
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        if (gameOverText != null) gameOverText.text = $"Game Over!\n{reason}\nFinal Score: {score}";
        if (restartButton != null) restartButton.gameObject.SetActive(true);

        // ✅ SUBMIT SCORE TO LEADERBOARD
        int userId = PlayerPrefs.GetInt("user_id", 0);
        
        if (userId != 0)
        {
            Debug.Log($"🏆 Submitting Score: {score} for User: {userId}");
            StartCoroutine(SubmitScore(userId));
        }
        else
        {
            Debug.LogError("❌ Cannot submit score: User is not logged in (ID=0).");
        }
    }

    public void RestartGame()
    {
        if (restartButton) restartButton.gameObject.SetActive(false);
        StartGame();
    }

    // --- API Communication ---

    IEnumerator SubmitScore(int userId)
    {
        // Prepare Data
        ScorePayload payload = new ScorePayload 
        { 
            user_id = userId, 
            game_id = "sewing_game", // Match this ID in your database if needed
            score = score 
        };

        // Send Request
        if (APIManager.Instance != null)
        {
            // ✅ NOTE: We use /api/ prefix here to match Laravel routes
            yield return StartCoroutine(APIManager.Instance.PostRequest("/api/mobile-score", payload,
                (response) => {
                    Debug.Log("✅ Leaderboard Updated Successfully!");
                },
                (error) => {
                    Debug.LogError("❌ Leaderboard Submission Failed: " + error);
                }
            ));
        }
        else
        {
            Debug.LogError("❌ APIManager is missing from the scene!");
        }
    }

    // --- Helper Methods ---

    public void PauseTimer() 
    { 
        isTimerPaused = true; 
    }
    
    public void ResumeTimer() 
    { 
        isTimerPaused = false; 
    }

    public void AddScore(int amount)
    {
        if (!isGameActive) return;
        score += amount;
        UpdateScoreUI();
    }

    public void LoseLife()
    {
        if (!isGameActive) return;
        currentLives = Mathf.Max(0, currentLives - 1);
        UpdateLivesUI();

        if (currentLives <= 0)
        {
            EndGame("Out of Lives!");
        }
    }

    public void FlagCompleted()
    {
        if (!isGameActive) return;

        AddScore(scorePerFlag);

        if (flagController != null)
        {
            flagController.currentFlagIndex++;

            if (flagController.currentFlagIndex >= maxFlags)
            {
                EndGame("All Flags Complete!");
            }
            else
            {
                flagController.enabled = true;
                flagController.StartNewFlag();
            }
        }
    }

    // --- UI Updaters ---

    private void UpdateScoreUI()
    {
        if (scoreText != null) scoreText.text = "Score: " + score;
    }

    private void UpdateTimerUI()
    {
        if (timerText != null) timerText.text = "Time: " + Mathf.CeilToInt(timeRemaining);
    }

    private void UpdateLivesUI()
    {
        if (livesText != null) livesText.text = "Lives: " + currentLives;
    }
}
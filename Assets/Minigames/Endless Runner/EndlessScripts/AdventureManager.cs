using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections; // ✅ CRITICAL: Required for IEnumerator

public class AdventureManager : MonoBehaviour
{
    public static AdventureManager Instance;
    
    [Header("Game Settings")]
    public int maxLives = 3;
    public float gameSpeed = 6f;
    public float speedIncreaseRate = 0.5f; // Increased for better difficulty ramp
    
    [Header("Scoring")]
    public float scoreMultiplier = 10f;
    
    private int currentLives;
    private float currentScore;
    private bool isGameOver = false;
    
    public PlayerController player;
    public UIManager uiManager;

    // ✅ 1. Define the Data Structure for the API
    [System.Serializable]
    public class ScorePayload 
    { 
        public int user_id; 
        public string game_id; 
        public int score; 
    }
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        currentLives = maxLives;
        currentScore = 0;
        
        if (uiManager != null)
        {
            UpdateUI();
        }
        else
        {
            Debug.LogError("UIManager not assigned!");
        }
    }
    
    void Update()
    {
        if (!isGameOver)
        {
            // Increase score over time
            currentScore += Time.deltaTime * scoreMultiplier;
            
            // Gradually increase game speed
            gameSpeed += speedIncreaseRate * Time.deltaTime;
            
            if (uiManager != null)
            {
                UpdateUI();
            }
        }
    }
    
    public void LoseLife()
    {
        if (isGameOver) return;
        
        currentLives--;
        Debug.Log("Lost a life! Lives remaining: " + currentLives);
        
        if (uiManager != null)
        {
            UpdateUI();
        }
        
        if (currentLives <= 0)
        {
            GameOver();
        }
    }
    
    void GameOver()
    {
        isGameOver = true;
        int finalScoreInt = Mathf.FloorToInt(currentScore);

        Debug.Log("GAME OVER! Final Score: " + finalScoreInt);
        
        if (player != null)
        {
            player.Die();
        }
        
        if (uiManager != null)
        {
            uiManager.ShowGameOver(finalScoreInt);
        }

        // ✅ 2. Connect to Database
        // We get the User ID saved during Login
        int userId = PlayerPrefs.GetInt("user_id", 0);
        
        if (userId != 0)
        {
            // Start the connection routine
            StartCoroutine(SubmitScore(userId, finalScoreInt));
        }
        else
        {
            Debug.LogError("❌ User not logged in. Score cannot be saved.");
        }
    }

    // ✅ 3. The Missing IEnumerator
    IEnumerator SubmitScore(int userId, int score)
    {
        ScorePayload payload = new ScorePayload 
        { 
            user_id = userId, 
            game_id = "endless_run", // Matches the tab on your website
            score = score 
        };

        // Check if APIManager exists (it lives in the Login scene usually)
        if (APIManager.Instance != null)
        {
            Debug.Log($"📤 Sending Score: {score}...");
            
            yield return StartCoroutine(APIManager.Instance.PostRequest("/api/mobile-score", payload,
                (res) => Debug.Log("✅ Run Score Saved to Database!"),
                (err) => Debug.LogError("❌ Score Save Failed: " + err)
            ));
        }
        else
        {
            Debug.LogError("❌ APIManager not found! Did you start from the Login Scene?");
        }
    }
    
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    public void ReturnToMenu()
    {
        // Make sure this matches your actual Menu scene name
        SceneManager.LoadScene("D_mainScreen"); 
    }
    
    void UpdateUI()
    {
        uiManager.UpdateLives(currentLives);
        uiManager.UpdateScore(Mathf.FloorToInt(currentScore));
    }
    
    public float GetGameSpeed()
    {
        return gameSpeed;
    }
    
    public bool IsGameOver()
    {
        return isGameOver;
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestController : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject mainMenuPanel;
    public GameObject gamePanel;
    public GameObject resultPanel;

    [Header("Main Menu Buttons")]
    public Button startButton;

    [Header("Game UI")]
    public TextMeshProUGUI questionNumberText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText; // ✅ New Timer UI
    public TextMeshProUGUI questionText;
    public Button optionA_Button;
    public Button optionB_Button;
    public Button optionC_Button;
    public TextMeshProUGUI optionA_Text;
    public TextMeshProUGUI optionB_Text;
    public TextMeshProUGUI optionC_Text;
    public Slider progressBar;
    public TextMeshProUGUI explanationText; // ✅ New: Show why answer is correct

    [Header("Result UI")]
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI resultMessageText;
    public Button playAgainButton;
    public Button mainMenuButton;

    [Header("Game Settings")]
    public int questionsPerGame = 10;
    public int basePoints = 100;
    public float timePerQuestion = 15f;

    private List<Question> currentGameQuestions;
    private int currentQuestionIndex = 0;
    private int score = 0;
    private bool hasAnswered = false;
    private float timer;

    // API Payload
    [System.Serializable]
    public class ScorePayload { public int user_id; public string game_id; public int score; }

    void Start()
    {
        SetupButtons();
        ShowMainMenu();
        if(explanationText) explanationText.text = "";
    }

    void Update()
    {
        if (gamePanel.activeSelf && !hasAnswered)
        {
            timer -= Time.deltaTime;
            
            // Update Timer UI
            if (timerText) timerText.text = Mathf.CeilToInt(timer).ToString();
            
            // Time's Up Logic
            if (timer <= 0)
            {
                CheckAnswer("TIMEOUT");
            }
        }
    }

    void SetupButtons()
    {
        if (startButton) startButton.onClick.AddListener(StartGame);
        if (playAgainButton) playAgainButton.onClick.AddListener(StartGame);
        if (mainMenuButton) mainMenuButton.onClick.AddListener(ShowMainMenu);
    }

    void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        gamePanel.SetActive(false);
        resultPanel.SetActive(false);
    }

    void StartGame()
    {
        mainMenuPanel.SetActive(false);
        gamePanel.SetActive(true);
        resultPanel.SetActive(false);

        currentQuestionIndex = 0;
        score = 0;
        
        if (QuestionDatabase.Instance == null)
        {
            Debug.LogError("QuestionDatabase missing!");
            return;
        }
        
        currentGameQuestions = QuestionDatabase.Instance.GetRandomQuestions(questionsPerGame);

        // Setup Listeners once
        optionA_Button.onClick.RemoveAllListeners();
        optionB_Button.onClick.RemoveAllListeners();
        optionC_Button.onClick.RemoveAllListeners();

        optionA_Button.onClick.AddListener(() => CheckAnswer("A"));
        optionB_Button.onClick.AddListener(() => CheckAnswer("B"));
        optionC_Button.onClick.AddListener(() => CheckAnswer("C"));

        DisplayQuestion();
    }

    void DisplayQuestion()
    {
        if (currentQuestionIndex >= currentGameQuestions.Count)
        {
            ShowResults();
            return;
        }

        hasAnswered = false;
        timer = timePerQuestion; // Reset Timer
        if(explanationText) explanationText.text = ""; // Clear explanation

        EnableButtons(true);
        ResetButtonColors();

        Question currentQuestion = currentGameQuestions[currentQuestionIndex];

        questionNumberText.text = $"Question {currentQuestionIndex + 1}/{questionsPerGame}";
        scoreText.text = $"Score: {score}";
        questionText.text = currentQuestion.questionText;
        optionA_Text.text = "A. " + currentQuestion.optionA;
        optionB_Text.text = "B. " + currentQuestion.optionB;
        optionC_Text.text = "C. " + currentQuestion.optionC;

        progressBar.value = (float)currentQuestionIndex / questionsPerGame;
    }

    void CheckAnswer(string selectedAnswer)
    {
        if (hasAnswered) return;
        hasAnswered = true;

        Question currentQuestion = currentGameQuestions[currentQuestionIndex];
        bool isCorrect = (selectedAnswer == currentQuestion.correctAnswer);

        if (isCorrect)
        {
            // Score Calculation: Base Points + (Time Remaining * 10)
            int timeBonus = Mathf.FloorToInt(timer) * 10;
            score += (basePoints + timeBonus);
            
            HighlightButton(selectedAnswer, Color.green);
            PlaySound(true);
        }
        else
        {
            // Wrong or Timeout
            if(selectedAnswer != "TIMEOUT") HighlightButton(selectedAnswer, Color.red);
            
            // Always show correct answer
            HighlightButton(currentQuestion.correctAnswer, Color.green);
            PlaySound(false);
        }

        // Show Explanation
        if(explanationText) explanationText.text = currentQuestion.explanation;

        EnableButtons(false);
        StartCoroutine(NextQuestionDelay());
    }

    IEnumerator NextQuestionDelay()
    {
        yield return new WaitForSeconds(2.5f); // Longer delay to read explanation
        currentQuestionIndex++;
        DisplayQuestion();
    }

    void HighlightButton(string answer, Color color)
    {
        Button button = null;
        switch (answer)
        {
            case "A": button = optionA_Button; break;
            case "B": button = optionB_Button; break;
            case "C": button = optionC_Button; break;
        }

        if (button != null)
        {
            ColorBlock cb = button.colors;
            cb.normalColor = color;
            cb.disabledColor = color; // Keep color when disabled
            button.colors = cb;
        }
    }

    void ResetButtonColors()
    {
        Color normalColor = Color.white;
        Button[] buttons = { optionA_Button, optionB_Button, optionC_Button };

        foreach (Button button in buttons)
        {
            ColorBlock cb = button.colors;
            cb.normalColor = normalColor;
            cb.highlightedColor = normalColor;
            cb.disabledColor = new Color(0.8f, 0.8f, 0.8f); // Default disabled gray
            button.colors = cb;
        }
    }

    void EnableButtons(bool enable)
    {
        optionA_Button.interactable = enable;
        optionB_Button.interactable = enable;
        optionC_Button.interactable = enable;
    }

    void ShowResults()
    {
        gamePanel.SetActive(false);
        resultPanel.SetActive(true);

        finalScoreText.text = "Final Score: " + score;

        // Determine Rank
        if (score >= 2000) resultMessageText.text = "Trivia Master!";
        else if (score >= 1000) resultMessageText.text = "Great Knowledge!";
        else resultMessageText.text = "Keep Learning!";

        // ✅ SUBMIT TO LEADERBOARD
        int userId = PlayerPrefs.GetInt("user_id", 0);
        if(userId != 0)
        {
            StartCoroutine(SubmitScore(userId));
        }
        else
        {
            Debug.LogError("Cannot save score: User not logged in");
        }
    }

    IEnumerator SubmitScore(int userId)
    {
        ScorePayload payload = new ScorePayload 
        { 
            user_id = userId, 
            game_id = "trivia_quest", // Matches website Tab
            score = score 
        };

        if (APIManager.Instance != null)
        {
            yield return StartCoroutine(APIManager.Instance.PostRequest("/api/mobile-score", payload,
                (res) => Debug.Log("Trivia Score Submitted!"),
                (err) => Debug.LogError("Trivia Error: " + err)
            ));
        }
    }

    // Placeholder for Audio
    void PlaySound(bool correct)
    {
        // Add AudioSource logic here if you have sound files
    }
}
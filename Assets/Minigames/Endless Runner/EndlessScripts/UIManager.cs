using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("HUD")]
    public TextMeshProUGUI scoreText;
    public GameObject[] lifeIcons;
    
    [Header("Game Over Screen")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI finalScoreText;
    public Button restartButton;
    public Button menuButton;
    
    void Start()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
        
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(() => AdventureManager.Instance.RestartGame());
        }
        
        if (menuButton != null)
        {
            menuButton.onClick.AddListener(() => AdventureManager.Instance.ReturnToMenu());
        }
    }
    
    public void UpdateScore(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score.ToString();
        }
    }
    
    public void UpdateLives(int lives)
    {
        if (lifeIcons == null || lifeIcons.Length == 0) return;
        
        for (int i = 0; i < lifeIcons.Length; i++)
        {
            if (lifeIcons[i] != null)
            {
                lifeIcons[i].SetActive(i < lives);
            }
        }
    }
    
    public void ShowGameOver(int finalScore)
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        
        if (finalScoreText != null)
        {
            finalScoreText.text = "Final Score: " + finalScore.ToString();
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager Instance { get; private set; }

    [Header("Puzzle Settings")]
    public GameObject puzzlePiecePrefab;
    public Transform puzzleGrid; 
    public Sprite[] puzzleImages;
    
    [Header("UI Elements")]
    public Button shuffleButton;
    public Button nextPictureButton;
    public Button prevPictureButton;
    public TextMeshProUGUI pictureInfo;
    
    [Header("Game HUD")]
    public TextMeshProUGUI timerText;  // ✅ Drag your Timer Text here
    public TextMeshProUGUI movesText;  // ✅ Drag your Moves Text here
    public GameObject winPanel;        // ✅ Drag your Win Panel here
    public TextMeshProUGUI finalScoreText; 

    [Header("Puzzle State")]
    public int currentPictureIndex = 0;
    public bool isGameActive = false;
    
    private PuzzlePiece[,] puzzlePieces = new PuzzlePiece[3, 3];
    private int[,] currentState = new int[3, 3]; 
    private Vector2Int emptyPosition = new Vector2Int(2, 2);
    private Sprite[] targetSprites = new Sprite[9]; 
    
    // Scoring & Stats
    private float timer;
    private int moves;
    private const int BASE_SCORE = 10000; // Max potential score

    // API Payload
    [System.Serializable]
    public class ScorePayload { public int user_id; public string game_id; public int score; }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if(winPanel) winPanel.SetActive(false);
        
        // ✅ AUTO-RESIZE PIECES TO BE LARGER
        AdjustGridSize();

        InitializePuzzle();
        SetupUI();
    }

    void AdjustGridSize()
    {
        // Get the width of the container
        RectTransform gridRect = puzzleGrid.GetComponent<RectTransform>();
        GridLayoutGroup gridLayout = puzzleGrid.GetComponent<GridLayoutGroup>();

        if (gridRect != null && gridLayout != null)
        {
            // Calculate available width (minus padding)
            float width = gridRect.rect.width;
            
            // We want 3 columns. Cell size = (Width - Spacing) / 3
            float spacing = gridLayout.spacing.x * 2;
            float cellSize = (width - spacing) / 3f;

            // Apply the new size
            gridLayout.cellSize = new Vector2(cellSize, cellSize);
        }
    }

    void Update()
    {
        // ✅ TIMER LOGIC
        if (isGameActive)
        {
            timer += Time.deltaTime;
            if (timerText) 
            {
                int minutes = Mathf.FloorToInt(timer / 60F);
                int seconds = Mathf.FloorToInt(timer - minutes * 60);
                timerText.text = string.Format("{0:0}:{1:00}", minutes, seconds);
            }
        }
    }
    
    void InitializePuzzle()
    {
        CreatePuzzlePieces();
        LoadPicture(currentPictureIndex);
        // Don't start timer until shuffle happens
    }
    
    void CreatePuzzlePieces()
    {
        foreach (Transform child in puzzleGrid) Destroy(child.gameObject);
        
        // Initialize logic array
        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++) currentState[x, y] = (y * 3 + x);
        }
        
        // Create Visual Pieces
        for (int i = 0; i < 9; i++)
        {
            GameObject pieceObj = Instantiate(puzzlePiecePrefab, puzzleGrid);
            PuzzlePiece piece = pieceObj.GetComponent<PuzzlePiece>();
            
            int x = i % 3;
            int y = i / 3;
            
            piece.Initialize(new Vector2Int(x, y), this, i);
            puzzlePieces[x, y] = piece;
        }
        
        // Set bottom-right as empty
        puzzlePieces[2, 2].SetEmpty(true);
        currentState[2, 2] = 8;
        emptyPosition = new Vector2Int(2, 2);
    }
    
    void LoadPicture(int pictureIndex)
    {
        if (pictureIndex < 0 || pictureIndex >= puzzleImages.Length) return;
        
        Sprite fullImage = puzzleImages[pictureIndex];
        
        for (int i = 0; i < 9; i++)
        {
            int x = i % 3;
            int y = i / 3;
            
            if (i == 8) targetSprites[i] = null; // The empty slot
            else targetSprites[i] = CreateSpriteFromTexture(fullImage.texture, x, y);
        }
        
        UpdatePuzzleDisplay();
        UpdatePictureInfo();
    }
    
    void UpdatePuzzleDisplay()
    {
        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                int pieceID = currentState[x, y];
                PuzzlePiece piece = puzzlePieces[x, y];
                
                // If piece ID is 8, it's the empty slot
                if (pieceID == 8) 
                {
                    piece.SetEmpty(true);
                    piece.SetSprite(null);
                }
                else
                {
                    piece.SetEmpty(false);
                    piece.SetSprite(targetSprites[pieceID]);
                }
            }
        }
    }
    
    Sprite CreateSpriteFromTexture(Texture2D texture, int pieceX, int pieceY)
    {
        int pieceWidth = texture.width / 3;
        int pieceHeight = texture.height / 3;
        int startX = pieceX * pieceWidth;
        int startY = (2 - pieceY) * pieceHeight; 
        
        Rect rect = new Rect(startX, startY, pieceWidth, pieceHeight);
        return Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f));
    }
    
    void SetupUI()
    {
        if (shuffleButton) shuffleButton.onClick.AddListener(() => StartCoroutine(ShuffleRoutine()));
        if (nextPictureButton) nextPictureButton.onClick.AddListener(NextPicture);
        if (prevPictureButton) prevPictureButton.onClick.AddListener(PrevPicture);
        
        UpdatePictureInfo();
    }
    
    public void OnPieceClicked(Vector2Int piecePosition)
    {
        if (!isGameActive) return;
        
        if (IsAdjacentToEmpty(piecePosition))
        {
            SwapWithEmpty(piecePosition);
            moves++;
            if(movesText) movesText.text = "Moves: " + moves;

            UpdatePuzzleDisplay();
            CheckWinCondition();
        }
    }
    
    bool IsAdjacentToEmpty(Vector2Int position)
    {
        Vector2Int distance = position - emptyPosition;
        return (Mathf.Abs(distance.x) == 1 && distance.y == 0) || 
               (Mathf.Abs(distance.y) == 1 && distance.x == 0);
    }
    
    void SwapWithEmpty(Vector2Int piecePosition)
    {
        int temp = currentState[piecePosition.x, piecePosition.y];
        currentState[piecePosition.x, piecePosition.y] = currentState[emptyPosition.x, emptyPosition.y];
        currentState[emptyPosition.x, emptyPosition.y] = temp;
        emptyPosition = piecePosition;
    }
    
    void CheckWinCondition()
    {
        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                int expectedID = (y * 3 + x);
                if (currentState[x, y] != expectedID) return; 
            }
        }
        
        GameWon();
    }
    
    void GameWon()
    {
        isGameActive = false;
        
        // ✅ SCORING ALGORITHM (Time Based)
        // Start with 10,000. Subtract 10 points per second. Subtract 5 points per move.
        // Example: 60 seconds + 50 moves = 10000 - 600 - 250 = 9150 pts
        int finalScore = Mathf.Max(0, BASE_SCORE - (Mathf.FloorToInt(timer) * 10) - (moves * 5));
        
        if (pictureInfo) pictureInfo.text = "COMPLETED!";
        if (finalScoreText) finalScoreText.text = "Score: " + finalScore;
        if (winPanel) winPanel.SetActive(true);

        // ✅ FILL THE MISSING PIECE
        // We find the bottom-right piece (which is technically the 'empty' one) and give it the last sprite part
        // We need to regenerate the 8th sprite part (bottom-right of original image)
        Sprite fullImage = puzzleImages[currentPictureIndex];
        Sprite lastPieceSprite = CreateSpriteFromTexture(fullImage.texture, 2, 2); // 2,2 is bottom right
        
        puzzlePieces[2, 2].SetEmpty(false);
        puzzlePieces[2, 2].SetSprite(lastPieceSprite);

        // ✅ SEND TO DB
        int userId = PlayerPrefs.GetInt("user_id", 0);
        if(userId != 0)
        {
            StartCoroutine(SubmitScore(userId, finalScore));
        }
    }

    IEnumerator SubmitScore(int userId, int score)
    {
        ScorePayload payload = new ScorePayload 
        { 
            user_id = userId, 
            game_id = "sliding_puzzle", 
            score = score 
        };

        if (APIManager.Instance != null)
        {
            yield return StartCoroutine(APIManager.Instance.PostRequest("/api/mobile-score", payload,
                (res) => Debug.Log("Puzzle Score Submitted!"),
                (err) => Debug.LogError("Puzzle Score Error: " + err)
            ));
        }
    }
    
    IEnumerator ShuffleRoutine()
    {
        isGameActive = false;
        timer = 0;
        moves = 0;
        if(movesText) movesText.text = "Moves: 0";
        if(timerText) timerText.text = "0:00";
        if(winPanel) winPanel.SetActive(false);

        // Reset pieces first
        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++) currentState[x, y] = (y * 3 + x);
        }
        emptyPosition = new Vector2Int(2, 2);
        
        // Fast Shuffle logic
        for (int i = 0; i < 200; i++) 
        {
            List<Vector2Int> validMoves = GetValidMoves();
            if (validMoves.Count > 0)
            {
                Vector2Int randomMove = validMoves[Random.Range(0, validMoves.Count)];
                SwapWithEmpty(randomMove);
            }
        }
        
        UpdatePuzzleDisplay();
        UpdatePictureInfo();
        
        yield return new WaitForSeconds(0.5f);
        isGameActive = true; // Start timer
    }
    
    List<Vector2Int> GetValidMoves()
    {
        List<Vector2Int> validMoves = new List<Vector2Int>();
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        
        foreach (Vector2Int dir in directions)
        {
            Vector2Int newPos = emptyPosition + dir;
            if (newPos.x >= 0 && newPos.x < 3 && newPos.y >= 0 && newPos.y < 3)
            {
                validMoves.Add(newPos);
            }
        }
        return validMoves;
    }
    
    void NextPicture() { currentPictureIndex = (currentPictureIndex + 1) % puzzleImages.Length; LoadPicture(currentPictureIndex); StartCoroutine(ShuffleRoutine()); }
    void PrevPicture() { currentPictureIndex = (currentPictureIndex - 1 + puzzleImages.Length) % puzzleImages.Length; LoadPicture(currentPictureIndex); StartCoroutine(ShuffleRoutine()); }
    void UpdatePictureInfo() { if (pictureInfo != null) pictureInfo.text = "Picture " + (currentPictureIndex + 1) + "/" + puzzleImages.Length; }
}
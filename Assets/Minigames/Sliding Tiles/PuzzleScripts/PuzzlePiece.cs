using UnityEngine;
using UnityEngine.UI;

public class PuzzlePiece : MonoBehaviour
{
    private Vector2Int gridPosition;
    private PuzzleManager puzzleManager;
    private Button button;
    private Image image;
    private bool isEmpty = false;
    private int pieceID; 
    
    void Awake()
    {
        button = GetComponent<Button>();
        image = GetComponent<Image>();
    }
    
    public void Initialize(Vector2Int position, PuzzleManager manager, int id)
    {
        gridPosition = position;
        puzzleManager = manager;
        pieceID = id;
        
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClick);
        }
        
        if (image != null)
        {
            image.color = Color.white;
            image.preserveAspect = false; 
            image.raycastTarget = true;
        }
        
        if (pieceID == 8) SetEmpty(true);
    }
    
    void OnClick()
    {
        // Check if game is active before allowing clicks
        if (puzzleManager != null && !isEmpty && puzzleManager.isGameActive)
        {
            puzzleManager.OnPieceClicked(gridPosition);
        }
    }
    
    public void SetSprite(Sprite sprite)
    {
        if (image != null)
        {
            image.sprite = sprite;
            image.enabled = (sprite != null); // Hide if null
            image.color = Color.white;
        }
    }
    
    public void SetEmpty(bool empty)
    {
        isEmpty = empty;
        
        if (image != null)
        {
            // Option A: Fully Invisible (Current)
            // image.enabled = !empty; 

            // Option B: Faint "Ghost" Piece (Better for UI?)
            if (empty) {
                image.enabled = true;
                image.color = new Color(1f, 1f, 1f, 0.2f); // Very faint
            } else {
                image.enabled = true;
                image.color = Color.white;
            }
        }
        
        // Keep button interactable? No, you can't click the empty slot itself.
        if (button != null) button.interactable = !empty;
    }
}
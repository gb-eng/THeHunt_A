using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class script_gameMenu : MonoBehaviour
{
    public UIDocument game_screen;

    void Start()
    {
        if (game_screen == null) game_screen = GetComponent<UIDocument>();
        var root = game_screen.rootVisualElement;
        
        Debug.Log("Game menu UI loaded. Searching for buttons...");

        // Find Buttons
        var game1 = root.Q<Button>("game1"); // Sew The Flag (MAR)
        var game2 = root.Q<Button>("game2"); // Flavors (MKT)
        var game3 = root.Q<Button>("game3"); // Trivia (BAS)
        var game4 = root.Q<Button>("game4"); // Adventure (APA)
        var game5 = root.Q<Button>("game5"); // Restore (CAS)
        var backBtn = root.Q<Button>("backbutton_D");

        // ✅ SETUP BUTTONS: Pass the "GAME_ID" that ARScanController unlocks
        
        SetupGameButton(game1, "GAME_FLAG", "SewScene", "Sew The Flag", 
            "Tap the GOLD needles to sew the revolutionary flags! Avoid the RED needles or you will lose a life! Complete all 10 historic Philippine flags.");

        SetupGameButton(game2, "GAME_FLAVORS", "MatchingCardScene", "Memory of Flavors", 
            "Flip the cards to find matching pairs of delicious Taal delicacies! Clear both sets of 10 cards to complete the game!");

        SetupGameButton(game3, "GAME_TRIVIA", "TriviaQuestScene", "Taal Trivia Quest", 
            "Answer 10 questions about Taal, Batangas and prove your knowledge of Taal's rich culture and history!");

        SetupGameButton(game4, "GAME_ADVENTURE", "EndlessRunScene", "Apacible's Adventure", 
            "Guide Leon as far as you can in this endless runner minigame. The adventure ends when you hit 3 obstacles. Good luck!");

        SetupGameButton(game5, "GAME_RESTORE", "SlidingPuzzScene", "Restore the Heritage", 
            "Slide the tiles to complete the picture! Tap a tile next to the empty space to move it. Arrange all pieces correctly to reveal beautiful Taal Heritage Sites.");

        if (backBtn != null)
        {
            backBtn.clicked += () => {
                Debug.Log("Loading D_mainScreen...");
                SceneManager.LoadScene("D_mainScreen");
            };
        }
    }

    void SetupGameButton(Button btn, string gameLockId, string sceneName, string title, string instructions)
    {
        if (btn == null) return;

        // ✅ CHECK LOCK: "HasUnlocked_GAME_FLAG", etc.
        bool isUnlocked = PlayerPrefs.GetInt("HasUnlocked_" + gameLockId, 0) == 1;

        if (isUnlocked)
        {
            btn.style.opacity = 1f; 
            btn.SetEnabled(true);
            btn.clicked += () => SelectGame(sceneName, title, instructions);
        }
        else
        {
            btn.style.opacity = 0.5f; 
            btn.clicked += () => {
                Debug.Log($"🔒 {title} is locked. Scan the trigger artifact first.");
                if (PopupManager.Instance != null)
                    PopupManager.Instance.ShowReward("T_Locked", "Game Locked!", () => { });
            };
        }
    }

    void SelectGame(string sceneName, string title, string instructions)
    {
        Debug.Log($"Selected game: {title} | Scene: {sceneName}");

        if (script_gameManager.Instance == null)
        {
            var gd = new GameObject("GameData");
            gd.AddComponent<script_gameManager>();
        }

        script_gameManager.Instance.selectedGame = sceneName;
        script_gameManager.Instance.selectedTitle = title;
        script_gameManager.Instance.selectedInstructions = instructions;

        SceneManager.LoadScene("G_gameInstruct");
    }
}
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class script_gameMenu : MonoBehaviour
{
    public UIDocument game_screen;
    void Start()
    {
        var root = game_screen.rootVisualElement;
        Debug.Log("Game menu UI loaded. Searching for buttons...");

        // Try to find buttons (add debug lines)
        var game1 = root.Q<Button>("game1");
        var game2 = root.Q<Button>("game2");
        var game3 = root.Q<Button>("game3");
        var game4 = root.Q<Button>("game4");
        var game5 = root.Q<Button>("game5");
        var backBtn = root.Q<Button>("backbutton_D");

        Debug.Log(game1 != null ? "Found game1 button ✅" : "❌ game1 button not found!");
        Debug.Log(game2 != null ? "Found game2 button ✅" : "❌ game2 button not found!");
        Debug.Log(game2 != null ? "Found game3 button ✅" : "❌ game3 button not found!");
        Debug.Log(game2 != null ? "Found game4 button ✅" : "❌ game4 button not found!");
        Debug.Log(game2 != null ? "Found game5 button ✅" : "❌ game5 button not found!");
        Debug.Log(backBtn != null ? "Found backbutton_D ✅" : "❌ backbutton_D not found!");

        // Hook up game buttons
        if (game1 != null)
            game1.clicked += () => SelectGame(
                "SewScene",
                "Sew The Flag",
                "Tap the GOLD needles to sew the revolutionary flags! Avoid the RED needles or you will lose a life! Complete all 10 historic Philippine flags."
            );

        if (game2 != null)
            game2.clicked += () => SelectGame(
                "MatchingCardScene",
                "Memory of Flavors",
                "Flip the cards to find matching pairs of delicious Taal delicacies! Clear both sets of 10 cards to complete the game!"
            );

        if (game3 != null)
            game3.clicked += () => SelectGame(
                "TriviaQuestScene",
                "Taal Trivia Quest",
                "Answer 10 questions about Taal, Batangas and prove your knowledge of Taal's rich culture and history!"
            );

        if (game4 != null)
            game4.clicked += () => SelectGame(
                "EndlessRunScene",
                "Apacible's Adventure",
                "Guide Leon as far as you can in this endless runner minigame. The adventure ends when you hit 3 obstacles. Good luck!"
            );

        if (game5 != null)
            game5.clicked += () => SelectGame(
                "SlidingPuzzScene",
                "Restore the Heritage",
                "Slide the tiles to complete the picture! Tap a tile next to the empty space to move it. Arrange all pieces correctly to reveal beautiful Taal Heritage Sites."
            );

        // Back button
        if (backBtn != null)
            backBtn.clicked += () =>
            {
                Debug.Log("Loading D_mainScreen...");
                SceneManager.LoadScene("D_mainScreen");
            };
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

        // Works directly without loader instance
        Debug.Log("Loading G_gameInstruct...");
        SceneManager.LoadScene("G_gameInstruct");
    }
}

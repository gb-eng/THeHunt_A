using UnityEngine;

public class script_gameManager : MonoBehaviour
{
    public static script_gameManager Instance;

    public string selectedTitle;
    public string selectedInstructions;
    public string selectedGame;
    public string id;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}

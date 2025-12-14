using UnityEngine;
using UnityEngine.SceneManagement;

public class loadTransition : MonoBehaviour
{
    public float delay = 5f; // seconds before next scene

    void Start()
    {
        // Invoke transition after delay
        Invoke(nameof(LoadNextScene), delay);
    }

    void LoadNextScene()
    {
        SceneManager.LoadScene("C_loginscreen");
    }
}

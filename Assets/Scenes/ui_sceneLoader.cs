using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class ui_sceneLoader : MonoBehaviour
{
    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        var button_screen = root.Q<Button>("screenButton");
        if (button_screen != null)
        {
            button_screen.clicked += () =>
            {
                Debug.Log("Loading A_loadScreen...");
                SceneManager.LoadScene("A_loadScreen");
            };
        }

        var butt_back_DF = root.Q<Button>("F_gameScreen");
        if (butt_back_DF != null)
        {
            butt_back_DF.clicked += () =>
            {
                Debug.Log("Loading F_gameScreen...");
                SceneManager.LoadScene("F_gameScreen");
            };
        }

        var butt_back_DE = root.Q<Button>("backbutton_D");
        if (butt_back_DE != null)
        {
            butt_back_DE.clicked += () =>
            {
                Debug.Log("Loading D_mainScreen...");
                SceneManager.LoadScene("D_mainScreen");
            };
        }

        var butt_back_GF = root.Q<Button>("backbutton_F");
        if (butt_back_GF != null)
        {
            butt_back_GF.clicked += () =>
            {
                Debug.Log("Loading F_gameScreen...");
                SceneManager.LoadScene("F_gameScreen");
            };
        }

        var butt_cam = root.Q<Button>("arButton");
        if (butt_cam != null)
        {
            butt_cam.clicked += () =>
        {
            Debug.Log("Loading E_cameraScreen with AR scenes...");
            // Load your main camera screen first
            SceneManager.LoadScene("E_cameraScreen");
        };

        }

        var butt_mingame = root.Q<Button>("gamesButton");
        if (butt_mingame != null)
        {
            butt_mingame.clicked += () =>
            {
                Debug.Log("Loading F_gameScreen...");
                SceneManager.LoadScene("F_gameScreen");
            };
        }

        var butt_progress = root.Q<Button>("progressButton");
        if (butt_progress != null)
        {
            butt_progress.clicked += () =>
            {
                Debug.Log("Loading H_progressScreen...");
                SceneManager.LoadScene("H_progressScreen");
            };
        }
        var butt_museum = root.Q<Button>("museumButton");
        if (butt_museum != null)
        {
            butt_museum.clicked += () =>
            {
                Debug.Log("Loading I_museumScreen...");
                SceneManager.LoadScene("I_museumScreen");
            };
        }
    }

    public void LoadScene(string sceneName)
{
    Debug.Log("🧭 Loading scene: " + sceneName);
    SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
}

    }
    


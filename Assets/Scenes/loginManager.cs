using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System.Collections;

public class script_login : MonoBehaviour
{
    public ui_sceneLoader sceneLoader;
    
    private TextField emailField;
    private TextField passwordField;
    private Button loginButton;
    private Button registerButton;

    // Data Classes for JSON
    [System.Serializable]
    public class LoginPayload { public string email; public string password; }

    [System.Serializable]
    public class LoginResponse { public bool success; public string message; public UserData user; }

    [System.Serializable]
    public class UserData { public int id; public string email; public string username; }

    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        
        // Find Elements
        emailField = root.Q<TextField>("emailField");
        passwordField = root.Q<TextField>("passwordField");
        loginButton = root.Q<Button>("loginButton");
        registerButton = root.Q<Button>("registerButton"); 

        // Setup Login Logic
        if(loginButton != null)
            loginButton.clicked += OnLoginClicked;
            
        // Setup Register Logic
        if(registerButton != null)
            registerButton.clicked += OnRegisterClicked;
    }

    private void OnRegisterClicked()
    {
        Debug.Log("Opening Registration Page...");
        Application.OpenURL("https://thehunt.xyz"); 
    }

    private void OnLoginClicked()
    {
        string email = emailField.value;
        string password = passwordField.value;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            Debug.Log("Please enter both email and password.");
            return;
        }

        StartCoroutine(LoginProcess(email, password));
    }

    IEnumerator LoginProcess(string email, string password)
    {
        LoginPayload payload = new LoginPayload { email = email, password = password };

        if (APIManager.Instance == null)
        {
            Debug.LogError("❌ APIManager missing! Make sure to create the GameObject in the scene.");
            yield break;
        }

        yield return StartCoroutine(APIManager.Instance.PostRequest("/mobile-login", payload, 
            (response) => {
                // Success
                LoginResponse data = JsonUtility.FromJson<LoginResponse>(response);
                if (data.success && data.user != null)
                {
                    // ✅ SAVE USER DATA
                    PlayerPrefs.SetInt("user_id", data.user.id);
                    PlayerPrefs.SetString("email", data.user.email);
                    
                    // ✅ SAVE NAME (Fixes the "Null" issue)
                    PlayerPrefs.SetString("player_name", data.user.username);
                    
                    PlayerPrefs.Save(); 
                    
                    Debug.Log($"✅ Login Saved. Welcome, {data.user.username}!");

                    if (sceneLoader != null) sceneLoader.LoadScene("D_mainScreen");
                    else SceneManager.LoadScene("D_mainScreen");
                }
                else
                {
                    Debug.LogError("Login failed: " + data.message);
                }
            }, 
            (error) => {
                // Error
                Debug.LogError("Login Request Failed: " + error);
            }
        ));
    }

    public void LogoutUser()
    {
        PlayerPrefs.DeleteAll();
        SceneManager.LoadScene("C_loginScreen");
    }
}
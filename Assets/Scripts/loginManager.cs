using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System.Collections;

public class script_login : MonoBehaviour
{
    public ui_sceneLoader sceneLoader;
    public script_popup popupManager; // Ensure this is linked in Inspector

    private TextField emailField;
    private TextField passwordField;
    private Button loginButton;
    private Button registerButton;

    [System.Serializable]
    public class LoginPayload { public string email; public string password; }

    [System.Serializable]
    public class LoginResponse { public bool success; public string message; public UserData user; }

    [System.Serializable]
    public class UserData { public int id; public string email; public string username; }

    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        
        emailField = root.Q<TextField>("emailField");
        passwordField = root.Q<TextField>("passwordField");
        loginButton = root.Q<Button>("loginButton");
        registerButton = root.Q<Button>("registerButton"); 

        if(loginButton != null) loginButton.clicked += OnLoginClicked;
        if(registerButton != null) registerButton.clicked += OnRegisterClicked;

        // Auto-find popup if missing
        if (popupManager == null) popupManager = FindFirstObjectByType<script_popup>();
    }

    private void OnRegisterClicked()
    {
        Application.OpenURL("https://thehunt.xyz"); 
    }

    private void OnLoginClicked()
    {
        string email = emailField.value;
        string password = passwordField.value;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ShowError("Please enter both email and password.");
            return;
        }

        StartCoroutine(LoginProcess(email, password));
    }

    IEnumerator LoginProcess(string email, string password)
    {
        LoginPayload payload = new LoginPayload { email = email, password = password };

        if (APIManager.Instance == null)
        {
            ShowError("System Error: APIManager is missing.");
            yield break;
        }

        yield return StartCoroutine(APIManager.Instance.PostRequest("/mobile-login", payload, 
            (response) => {
                // --- SUCCESS CALLBACK (200 OK) ---
                LoginResponse data = JsonUtility.FromJson<LoginResponse>(response);
                
                if (data.success && data.user != null)
                {
                    PlayerPrefs.SetInt("user_id", data.user.id);
                    PlayerPrefs.SetString("email", data.user.email);
                    PlayerPrefs.SetString("player_name", data.user.username);
                    PlayerPrefs.Save(); 
                    
                    string key = "HasSeenIntro_" + data.user.id;
                    if (PlayerPrefs.GetInt(key, 0) == 0) LoadScene("I_introScreen");
                    else LoadScene("D_mainScreen");
                }
                else
                {
                    ShowError(data.message ?? "Login failed.");
                }
            }, 
            (error) => {
                // --- ERROR CALLBACK (401, 404, 500, or No Internet) ---
                // Unity treats 401 (Wrong Password) as an error, confusing it with network issues.
                
                // 1. Try to extract clean message from JSON inside the error string
                if (error.Contains("{"))
                {
                    try 
                    {
                        string json = error.Substring(error.IndexOf("{"));
                        LoginResponse errData = JsonUtility.FromJson<LoginResponse>(json);
                        if (errData != null && !string.IsNullOrEmpty(errData.message))
                        {
                            ShowError(errData.message); // e.g. "Invalid credentials"
                            return;
                        }
                    } 
                    catch { /* Parsing failed, fall through to generic logic */ }
                }

                // 2. Specific Logic for 401 (Unauthorized)
                if (error.Contains("401") || error.Contains("Unauthorized"))
                {
                    ShowError("Invalid email or password.");
                }
                // 3. Generic Network Error
                else 
                {
                    ShowError("Unable to connect to server. Please check your internet.");
                }
            }
        ));
    }

    void ShowError(string message)
    {
        Debug.LogWarning("Login Logic: " + message);
        if (popupManager != null)
        {
            // Use existing popup system
            popupManager.ShowPopup(PopupType.pop_error, null, "Try Again", message);
        }
    }

    void LoadScene(string name)
    {
        if (sceneLoader != null) sceneLoader.LoadScene(name);
        else SceneManager.LoadScene(name);
    }

    public void LogoutUser()
    {
        PlayerPrefs.DeleteAll();
        SceneManager.LoadScene("C_loginScreen");
    }
}
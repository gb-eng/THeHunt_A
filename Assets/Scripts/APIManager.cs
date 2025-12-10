using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using System;

public class APIManager : MonoBehaviour
{
    private static APIManager _instance;

    // ✅ AUTO-CREATE if missing
    public static APIManager Instance
    {
        get
        {
            if (_instance == null)
            {
                // FIX: Explicitly use UnityEngine.Object to avoid ambiguity
                _instance = UnityEngine.Object.FindFirstObjectByType<APIManager>();

                // If still null, create it automatically
                if (_instance == null)
                {
                    GameObject go = new GameObject("APIManager");
                    _instance = go.AddComponent<APIManager>();
                    Debug.Log("⚡ APIManager was missing, so we auto-created it.");
                }
            }
            return _instance;
        }
    }

    // ✅ Configuration
    private const string BASE_URL = "https://thehunt.xyz"; 

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    public IEnumerator PostRequest(string endpoint, object payload, Action<string> onSuccess, Action<string> onError)
    {
        string url = BASE_URL + endpoint;
        string json = JsonUtility.ToJson(payload);

        Debug.Log($"🚀 sending to {url}: {json}");

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Accept", "application/json");
            request.SetRequestHeader("User-Agent", "TheHuntMobile/1.0");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                string errorMsg = $"❌ Network Error: {request.error}\nResponse: {request.downloadHandler.text}";
                Debug.LogError(errorMsg);
                if (onError != null) onError(errorMsg);
            }
            else
            {
                string response = request.downloadHandler.text;
                Debug.Log($"✅ Success: {response}");
                if (onSuccess != null) onSuccess(response);
            }
        }
    }
}
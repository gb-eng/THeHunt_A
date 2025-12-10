using UnityEngine;
using UnityEngine.Networking;
using Vuforia;
using System.Collections;

public class MarkerUnlocker : MonoBehaviour
{
    private ObserverBehaviour observer;

    void Start()
    {
        observer = GetComponent<ObserverBehaviour>();
        if (observer)
        {
            observer.OnTargetStatusChanged += OnTargetStatusChanged;
        }
    }

    private void OnTargetStatusChanged(ObserverBehaviour behaviour, TargetStatus status)
    {
        if ((status.Status == Status.TRACKED || status.Status == Status.EXTENDED_TRACKED) && behaviour)
        {
            string userId = PlayerPrefs.GetString("user_id", ""); // ✅ Pull from login or guest setup
            string markerId = behaviour.TargetName;

            if (string.IsNullOrEmpty(userId))
            {
                Debug.LogWarning("❌ No user_id found. Make sure login or guest registration is complete.");
                return;
            }

            if (string.IsNullOrEmpty(markerId))
            {
                Debug.LogWarning("❌ Marker ID is empty. Check Vuforia TargetName setup.");
                return;
            }

            Debug.Log($"📡 Attempting unlock: user_id={userId}, marker_id={markerId}");
            StartCoroutine(UnlockMarker(userId, markerId));
        }
    }

    IEnumerator UnlockMarker(string userId, string markerId)
    {
        string url = "https://thehunt.xyz/api/mobile-unlock"; // ✅ Use live server

        var payload = new
        {
            user_id = userId,
            marker_id = markerId
        };

        string json = JsonUtility.ToJson(payload);
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();

        // ✅ Add browser-like headers to bypass Cloudflare/Hostinger filters
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Accept", "application/json");
        request.SetRequestHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
        request.SetRequestHeader("Referer", "https://thehunt.xyz");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("✅ Marker unlocked: " + request.downloadHandler.text);
        }
        else
        {
            Debug.LogError("❌ Unlock failed: " + request.error + "\nResponse: " + request.downloadHandler.text);
        }
    }
}
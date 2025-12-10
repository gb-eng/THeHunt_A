using UnityEngine;
using Vuforia;
using UnityEngine.UIElements;
using System.Collections;

public class script_cloud : MonoBehaviour
{
    private CloudRecoBehaviour cloudReco;

    // API Data Payload
    [System.Serializable]
    public class UnlockPayload { public int user_id; public string marker_id; }

    void Awake()
    {
        cloudReco = GetComponent<CloudRecoBehaviour>();
        if (cloudReco == null) { Debug.LogError("❌ CloudRecoBehaviour missing!"); return; }

        cloudReco.RegisterOnNewSearchResultEventHandler(OnNewSearchResult);
    }

    private void OnNewSearchResult(CloudRecoBehaviour.CloudRecoSearchResult result)
    {
        // 1. PAUSE SCANNING (So we don't get 100 popups)
        cloudReco.enabled = false; 

        string rawId = result.TargetName;
        string cleanId = GetCleanAssetId(rawId); 

        Debug.Log($"🎯 Scanned: {rawId} -> Mapped to: {cleanId}");

        // 2. SHOW POPUP & PASS RESTART LOGIC
        if (RewardPopup.Instance != null)
        {
            // We pass a function: "When you close, set cloudReco.enabled = true"
            RewardPopup.Instance.ShowReward(cleanId, "Discovery!", () => {
                Debug.Log("🔄 Restarting Cloud Scanning...");
                cloudReco.enabled = true; 
            });
        }
        else
        {
            Debug.LogError("❌ RewardPopup Instance not found! Is 'RewardPopupManager' in the scene?");
        }

        // 3. Handle Database Logic
        int userId = PlayerPrefs.GetInt("user_id", 0);
        
        if (cleanId == "UNLOCK_FLAG_GAME")
        {
            // Special minigame logic here if needed
        }
        else if (userId != 0)
        {
            StartCoroutine(UnlockMarker(cleanId, userId));
        }
    }

    IEnumerator UnlockMarker(string markerId, int userId)
    {
        UnlockPayload payload = new UnlockPayload { user_id = userId, marker_id = markerId };

        if (APIManager.Instance != null)
        {
            yield return StartCoroutine(APIManager.Instance.PostRequest("/api/mobile-unlock", payload,
                (res) => Debug.Log($"✅ Marker {markerId} Saved to DB!"),
                (err) => Debug.LogError($"❌ Unlock Error: {err}")
            ));
        }
    }
    
    private string GetCleanAssetId(string vuforiaTargetName)
    {
        switch (vuforiaTargetName)
        {
            case "mrkr_apacible2-min":  return "Apaciblehouse";
            case "mrkr_basilica-min":   return "basilica";
            case "mrkr_real":           return "MariaRosa";
            case "markr_casareal-min":  return "CasaReal";
            case "mrkr_agoncillo-min":  return "MarcelaHouse";
            case "mrkr_agoncillo_flag": return "UNLOCK_FLAG_GAME";
            default: return vuforiaTargetName;
        }
    }

    void OnDestroy()
    {
        if (cloudReco != null) cloudReco.UnregisterOnNewSearchResultEventHandler(OnNewSearchResult);
    }
}
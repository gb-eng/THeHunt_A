using UnityEngine;
using Vuforia;
using System.Collections;
using System.Collections.Generic;

public class ARScanController : MonoBehaviour
{
    private CloudRecoBehaviour cloudReco;

    [System.Serializable]
    public class UnlockPayload { public int user_id; public string marker_id; }

    // ✅ TRIGGER MAPPING: CleanID -> MinigameID
    private Dictionary<string, string> minigameTriggers = new Dictionary<string, string>
    {
        { "MAR_Sewing", "GAME_FLAG" },       // Agoncillo Flag -> Flag Game
        { "BAS_Basilica", "GAME_TRIVIA" },   // Basilica -> Trivia Game
        { "APA_House", "GAME_ADVENTURE" },   // Apacible House -> Adventure Game
        { "CAS_CasaReal", "GAME_RESTORE" },  // Casa Real -> Restore Game
        { "MKT_Scene", "GAME_FLAVORS" }      // Market Scene -> Flavors Game
    };

    void Awake()
    {
        cloudReco = GetComponent<CloudRecoBehaviour>();
        if (cloudReco == null) { Debug.LogError("CloudRecoBehaviour missing!"); return; }

        cloudReco.RegisterOnNewSearchResultEventHandler(OnNewSearchResult);
    }

    private void OnNewSearchResult(CloudRecoBehaviour.CloudRecoSearchResult result)
    {
        string rawId = result.TargetName;
        string cleanId = GetCleanAssetId(rawId); 

        // 1. CHECK STORY LOCK
        if (StoryManager.Instance != null)
        {
            string lockMessage;
            if (!StoryManager.Instance.IsScanAllowed(cleanId, out lockMessage))
            {
                Debug.Log($"Scan Blocked: {lockMessage}");
                cloudReco.enabled = false; 
                if (PopupManager.Instance != null)
                    PopupManager.Instance.ShowReward("T_Locked", "Story Locked", () => cloudReco.enabled = true);
                return; 
            }
        }

        // 2. PROCEED WITH SCAN
        cloudReco.enabled = false; 
        Debug.Log($"Scanned: {rawId} -> Mapped to: {cleanId}");

        int userId = PlayerPrefs.GetInt("user_id", 0);
        string dbId = ConvertToDatabaseId(cleanId);

        // 3. MINIGAME UNLOCK LOGIC
        string minigameId = "";
        
        if (minigameTriggers.ContainsKey(cleanId))
        {
            string gameKey = minigameTriggers[cleanId];
            
            // Check if already unlocked locally
            if (PlayerPrefs.GetInt("HasUnlocked_" + gameKey, 0) == 0)
            {
                minigameId = gameKey;
                
                // Unlock Game & Area
                PlayerPrefs.SetInt("HasUnlocked_" + gameKey, 1);
                string areaPrefix = cleanId.Split('_')[0]; 
                PlayerPrefs.SetInt("HasUnlocked_" + areaPrefix, 1);
                
                PlayerPrefs.Save();
                Debug.Log($"🔓 Unlocked Game: {gameKey}");
            }
        }

        // 4. SHOW POPUPS
        if (PopupManager.Instance != null)
        {
            // Popup 1: Artifact/Lore
            PopupManager.Instance.ShowReward(cleanId, "Discovery!", () => {
                
                // Callback: Minigame Popup
                if (!string.IsNullOrEmpty(minigameId))
                {
                    if (userId != 0) StartCoroutine(UnlockMarker(minigameId, userId));

                    PopupManager.Instance.ShowReward("T_" + minigameId, "Minigame Unlocked!", () => {
                        cloudReco.enabled = true; 
                    });
                }
                else
                {
                    cloudReco.enabled = true; 
                }
            });
        }

        // 5. SAVE TO DB
        if (userId != 0)
        {
            StartCoroutine(UnlockMarker(dbId, userId));
        }
    }

    IEnumerator UnlockMarker(string markerId, int userId)
    {
        UnlockPayload payload = new UnlockPayload { user_id = userId, marker_id = markerId };

        if (APIManager.Instance != null)
        {
            yield return StartCoroutine(APIManager.Instance.PostRequest("/api/mobile-unlock", payload,
                (res) => Debug.Log($"✅ Unlocked {markerId} in DB!"),
                (err) => Debug.LogError($"Unlock Error: {err}")
            ));
        }
    }
    
    // ✅ FINAL MAPPING
    private string GetCleanAssetId(string vuforiaTargetName)
    {
        switch (vuforiaTargetName)
        {
            // BASILICA
            case "mrkr_basilica-min":       return "BAS_Basilica";
            case "mrkr_basilica_statue":    return "BAS_Jesus";
            case "basilicastoup":           return "BAS_Stoup";

            // AGONCILLO
            case "mrkr_agoncillo_flag":     return "MAR_Sewing";
            case "mrkr_agoncillo-min":      return "MAR_House";
            case "mrkr_agoncillo_drawer":   return "MAR_Drawer";
            case "mrkr_agoncillo_vase":     return "MAR_Vase";

            // APACIBLE
            case "mrkr_apacible2-min":      return "APA_House";
            case "mrkr_apacible_sumbrero":  return "APA_Sumbrero";
            case "mrkr_apacible_leon":      return "APA_Leon";

            // MARKET
            // Note: Empanada/Longganisa removed (They are Game Rewards now)
            case "mrkr_taalmarketplace2":   return "MKT_Scene"; // Triggers Game

            // CASA REAL
            case "mrkr_casareal-min":       return "CAS_CasaReal";
            case "mrkr_real":               return "CAS_MariaRosa";
            case "mrkr_casereal2-min":      return "CAS_Marker";

            default: return vuforiaTargetName;
        }
    }

    private string ConvertToDatabaseId(string cleanId)
    {
        if (cleanId.Contains("_"))
            return cleanId.Split('_')[1].ToLower(); 
        return cleanId.ToLower();
    }

    void OnDestroy()
    {
        if (cloudReco != null) cloudReco.UnregisterOnNewSearchResultEventHandler(OnNewSearchResult);
    }
}
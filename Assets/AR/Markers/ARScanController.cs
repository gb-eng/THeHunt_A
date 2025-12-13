using UnityEngine;
using Vuforia;
using System.Collections;
using System.Collections.Generic;

public class ARScanController : MonoBehaviour
{
    private CloudRecoBehaviour cloudReco;

    [System.Serializable]
    public class UnlockPayload { public int user_id; public string marker_id; }

    // ✅ TRIGGER MAPPING: Scanning these specific items unlocks the Minigame
    private Dictionary<string, string> minigameTriggers = new Dictionary<string, string>
    {
        { "MAR_Sewing", "GAME_FLAG" },       // Flag Marker -> Sew The Flag
        { "BAS_Basilica", "GAME_TRIVIA" },   // Basilica Marker -> Trivia
        { "APA_House", "GAME_ADVENTURE" },   // Apacible House -> Adventure
        { "CAS_CasaReal", "GAME_RESTORE" },  // Casa Real -> Restore
        { "MKT_Empanadas", "GAME_FLAVORS" }  // Empanada -> Flavors
    };

    void Awake()
    {
        cloudReco = GetComponent<CloudRecoBehaviour>();
        if (cloudReco == null) { Debug.LogError("❌ CloudRecoBehaviour missing!"); return; }

        cloudReco.RegisterOnNewSearchResultEventHandler(OnNewSearchResult);
    }

    private void OnNewSearchResult(CloudRecoBehaviour.CloudRecoSearchResult result)
    {
        string rawId = result.TargetName;
        string cleanId = GetCleanAssetId(rawId); 

        // 1. STORY LOCK CHECK
        if (StoryManager.Instance != null)
        {
            string lockMessage;
            if (!StoryManager.Instance.IsScanAllowed(cleanId, out lockMessage))
            {
                Debug.Log($"⛔ Scan Blocked: {lockMessage}");
                cloudReco.enabled = false; 
                if (PopupManager.Instance != null)
                    PopupManager.Instance.ShowReward("T_Locked", "Story Locked", () => cloudReco.enabled = true);
                return; 
            }
        }

        // 2. PROCEED WITH SCAN
        cloudReco.enabled = false; 
        Debug.Log($"🎯 Scanned: {rawId} -> Mapped to: {cleanId}");

        int userId = PlayerPrefs.GetInt("user_id", 0);
        string dbId = ConvertToDatabaseId(cleanId);

        // 3. MINIGAME UNLOCK LOGIC
        string minigameId = "";
        
        // If this item is a Trigger (e.g. Flag Marker)
        if (minigameTriggers.ContainsKey(cleanId))
        {
            string gameKey = minigameTriggers[cleanId]; // e.g. "GAME_FLAG"
            
            // Check if already unlocked locally to avoid double popup
            if (PlayerPrefs.GetInt("HasUnlocked_" + gameKey, 0) == 0)
            {
                minigameId = gameKey;
                
                // ✅ UNLOCK THE GAME
                PlayerPrefs.SetInt("HasUnlocked_" + gameKey, 1);
                
                // ✅ UNLOCK THE AREA (For the Menu grouping)
                // Extract "MAR" from "MAR_Sewing"
                string areaPrefix = cleanId.Split('_')[0]; 
                PlayerPrefs.SetInt("HasUnlocked_" + areaPrefix, 1);
                
                PlayerPrefs.Save();
                Debug.Log($"🔓 Unlocked Game: {gameKey} & Area: {areaPrefix}");
            }
        }

        // 4. SHOW POPUPS (Chained)
        if (PopupManager.Instance != null)
        {
            // Popup 1: The Artifact
            PopupManager.Instance.ShowReward(cleanId, "Discovery!", () => {
                
                // Callback: After closing Artifact Popup...
                if (!string.IsNullOrEmpty(minigameId))
                {
                    // Popup 2: The Minigame
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

        // 5. SAVE ARTIFACT TO DB
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
                (err) => Debug.LogError($"❌ Unlock Error: {err}")
            ));
        }
    }
    
    // ✅ COMPLETE MAPPING FOR YOUR NEW LIST
    private string GetCleanAssetId(string vuforiaTargetName)
    {
        switch (vuforiaTargetName)
        {
            // --- BASILICA (BAS) ---
            case "mrkr_basilica-min":       return "BAS_Basilica";  // [TRIGGER]
            case "mrkr_basilica_statue":    return "BAS_Jesus";
            case "basilicastoup":           return "BAS_Stoup";

            // --- AGONCILLO (MAR) ---
            case "mrkr_agoncillo_flag":     return "MAR_Sewing";    // [TRIGGER] - Flag Game
            case "mrkr_agoncillo-min":      return "MAR_House";
            case "mrkr_agoncillo_drawer":   return "MAR_Drawer";
            case "mrkr_agoncillo_vase":     return "MAR_Vase";

            // --- APACIBLE (APA) ---
            case "mrkr_apacible2-min":      return "APA_House";     // [TRIGGER]
            case "mrkr_apacible_sumbrero":  return "APA_Sumbrero";
            case "mrkr_apacible_leon":      return "APA_Leon";

            // --- MARKET (MKT) ---
            case "mrkr_taalmarketplace2":           return "MKT_Scene";     // Lore Only

            // --- CASA REAL (CAS) ---
            case "mrkr_casareal-min":       return "CAS_CasaReal";  // [TRIGGER]
            case "mrkr_real":               return "CAS_MariaRosa";
            case "mrkr_casereal2-min":      return "CAS_Marker";    // Lore Only

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
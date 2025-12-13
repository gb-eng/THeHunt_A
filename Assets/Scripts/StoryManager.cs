using UnityEngine;
using System.Collections.Generic;
using System.Linq; 

public class StoryManager : MonoBehaviour
{
    public static StoryManager Instance { get; private set; }

    [System.Serializable]
    public struct LoreFragment
    {
        public string associatedItemID; 
        [TextArea(3, 5)] 
        public string text;             
    }

    [System.Serializable]
    public class Chapter
    {
        public int chapterId;           
        public string title;            
        public string locationName;     
        public List<LoreFragment> fragments; 
        public bool isUnlocked;
    }

    public List<Chapter> chapters = new List<Chapter>();
    public int currentChapterIndex = 0; 

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else Destroy(gameObject);

        InitializeStory();
    }

    void InitializeStory()
    {
        // CHAPTER 1: BASILICA
        chapters.Add(new Chapter {
            chapterId = 1, title = "The Sentinel's Seat", locationName = "Taal Basilica", isUnlocked = true,
            fragments = new List<LoreFragment> {
                new LoreFragment { associatedItemID = "BAS_Basilica", text = "The Sentinel's endurance is measured not in years, but in vertical reach. The sheer scale of faith is built upon layers of ash and resilience." },
                new LoreFragment { associatedItemID = "BAS_Jesus", text = "The statue stands as a guardian of the faith, watching over the town through centuries of change." },
                new LoreFragment { associatedItemID = "BAS_Stoup", text = "The holy water stoup marks the entrance, a symbol of cleansing and the beginning of the journey into the Fourth Circle." }
            }
        });

        // CHAPTER 2: AGONCILLO
        chapters.Add(new Chapter {
            chapterId = 2, title = "The Revolution's Heart", locationName = "Agoncillo Museum", isUnlocked = false,
            fragments = new List<LoreFragment> {
                new LoreFragment { associatedItemID = "MAR_Sewing", text = "The sewing machine hummed with the creation of flags and symbols, stitching together the identity of a new republic." },
                new LoreFragment { associatedItemID = "MAR_House", text = "This house harbored nationalist activity. History here is written not in battle cries, but in secret departures and coded whispers." },
                new LoreFragment { associatedItemID = "MAR_Drawer", text = "Hidden compartments in the furniture stored the documents that would shape the future of the revolution." },
                new LoreFragment { associatedItemID = "MAR_Vase", text = "Even the decor held secrets. This vase witnessed the gathering of diplomats who would fight for the nation's recognition abroad." }
            }
        });

        // CHAPTER 3: APACIBLE
        chapters.Add(new Chapter {
            chapterId = 3, title = "The Weaver's Silence", locationName = "Apacible Museum", isUnlocked = false,
            fragments = new List<LoreFragment> {
                new LoreFragment { associatedItemID = "APA_House", text = "From the softness of silk to the hardness of steel, the Apacible legacy balances diplomacy with the strength of the blade." },
                new LoreFragment { associatedItemID = "APA_Sumbrero", text = "The hat worn by gentlemen of the era, shielding thoughts of rebellion from the scorching sun of occupation." },
                new LoreFragment { associatedItemID = "APA_Leon", text = "Leon Apacible's influence stretched far, weaving a network of support that sustained the resistance." }
            }
        });

        // CHAPTER 4: MARKET
        chapters.Add(new Chapter {
            chapterId = 4, title = "The Elder's Legacy", locationName = "Taal Market", isUnlocked = false,
            fragments = new List<LoreFragment> {
                new LoreFragment { associatedItemID = "MKT_Empanadas", text = "The sustenance of the people. The craft of food is as vital as the craft of stone, feeding the spirit of the revolution." },
                new LoreFragment { associatedItemID = "MKT_Longganisa", text = "A taste that defines the region. In the busy market, the true strength of Taal's economy and community spirit thrives." },
                new LoreFragment { associatedItemID = "MKT_Scene", text = "The bustling energy of the market has been the heartbeat of Taal's economy since the town's relocation." }
            }
        });

        // CHAPTER 5: CASA REAL
        chapters.Add(new Chapter {
            chapterId = 5, title = "The Lost Lake View", locationName = "Casa Real", isUnlocked = false,
            fragments = new List<LoreFragment> {
                new LoreFragment { associatedItemID = "CAS_CasaReal", text = "The ash from 1754 holds a unique geological fingerprint. This building stands as a testament to the town's relocation and rebirth." },
                new LoreFragment { associatedItemID = "CAS_MariaRosa", text = "Maria Rosa, a figure of the town's memory, represents the continuity of life even after the devastating eruption." },
                new LoreFragment { associatedItemID = "CAS_Marker", text = "This marker commemorates the rebuilding of the town center, a pledge that Taal would rise again from the ashes." }
            }
        });
    }

    public void RefreshChapterProgress(List<string> userUnlockedIds)
    {
        currentChapterIndex = 0; 
        for (int i = 0; i < chapters.Count; i++) {
            Chapter chap = chapters[i];
            bool isChapterComplete = true;
            foreach(var frag in chap.fragments) {
                string reqCore = frag.associatedItemID.Split('_')[1].ToLower();
                if (!userUnlockedIds.Exists(id => id.ToLower().Contains(reqCore))) {
                    isChapterComplete = false; break;
                }
            }
            if (isChapterComplete) {
                if (i + 1 < chapters.Count) {
                    chapters[i + 1].isUnlocked = true;
                    currentChapterIndex = i + 1;
                }
            } else {
                for (int j = i + 1; j < chapters.Count; j++) chapters[j].isUnlocked = false;
                break; 
            }
        }
    }

    public bool IsScanAllowed(string markerId, out string lockedMessage)
    {
        int targetChapterIndex = -1;
        for(int i=0; i<chapters.Count; i++) {
            if (chapters[i].fragments.Exists(f => markerId.Contains(f.associatedItemID) || f.associatedItemID.Contains(markerId))) {
                targetChapterIndex = i; break;
            }
        }
        if (targetChapterIndex == -1) { lockedMessage=""; return true; }
        if (targetChapterIndex > currentChapterIndex) {
            lockedMessage = $"🔒 Locked! Complete '{chapters[currentChapterIndex].title}' first.";
            return false;
        }
        lockedMessage = "";
        return true;
    }
}
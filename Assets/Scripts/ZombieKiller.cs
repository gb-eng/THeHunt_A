using UnityEngine;

public class ZombieKiller : MonoBehaviour
{
    // Type the exact names of objects you want to destroy when this scene starts
    public string[] zombiesToKill; 

    void Awake()
    {
        foreach (string name in zombiesToKill)
        {
            GameObject zombie = GameObject.Find(name);
            
            // If found, and it is persistent (DontDestroyOnLoad), kill it
            if (zombie != null && zombie.scene.name == "DontDestroyOnLoad")
            {
                Debug.Log($"🔪 ZombieKiller: Destroying stuck UI '{name}'");
                Destroy(zombie);
            }
        }
    }
}
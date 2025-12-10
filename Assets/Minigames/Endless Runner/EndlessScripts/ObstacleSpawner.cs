using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [Header("Spawning")]
    public GameObject[] obstaclePrefabs;
    public Transform spawnPoint;
    public float minSpawnInterval = 1.5f;
    public float maxSpawnInterval = 3f;
    
    [Header("Spawn Variation")]
    public float minXPosition = -4f;
    public float maxXPosition = 4f;
    
    [Header("Sorting Layer Settings - IMPORTANT")]
    [Tooltip("Sorting layer name for obstacles (e.g., 'Obstacles', 'Foreground')")]
    public string obstacleSortingLayer = "Foreground";
    [Tooltip("Order in layer - higher numbers appear in front")]
    public int obstacleOrderInLayer = 10;
    
    private float nextSpawnTime;
    
    void Start()
    {
        SetNextSpawnTime();
        
        // Validate sorting layer exists
        if (!SortingLayer.IsValid(SortingLayer.NameToID(obstacleSortingLayer)))
        {
            Debug.LogWarning($"Sorting layer '{obstacleSortingLayer}' not found! Using default layer.");
        }
    }
    
    void Update()
    {
        if (AdventureManager.Instance == null || AdventureManager.Instance.IsGameOver()) return;
        
        if (Time.time >= nextSpawnTime)
        {
            SpawnObstacle();
            SetNextSpawnTime();
        }
    }
    
    void SpawnObstacle()
    {
        if (obstaclePrefabs.Length == 0)
        {
            Debug.LogWarning("No obstacle prefabs assigned!");
            return;
        }
        
        // Choose random obstacle
        GameObject obstaclePrefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];
        
        // Random X position
        Vector3 spawnPos = spawnPoint.position;
        spawnPos.x = Random.Range(minXPosition, maxXPosition);
        
        // Spawn obstacle
        GameObject obstacle = Instantiate(obstaclePrefab, spawnPos, Quaternion.identity);
        obstacle.transform.SetParent(transform);
        
        // FIX SORTING ORDER - Make sure obstacles appear in front of background
        SetObstacleSortingOrder(obstacle);
    }
    
    void SetObstacleSortingOrder(GameObject obstacle)
    {
        // Get all SpriteRenderer components (including children)
        SpriteRenderer[] renderers = obstacle.GetComponentsInChildren<SpriteRenderer>(true);
        
        if (renderers.Length == 0)
        {
            Debug.LogWarning($"Obstacle '{obstacle.name}' has no SpriteRenderer!");
            return;
        }
        
        foreach (SpriteRenderer renderer in renderers)
        {
            renderer.sortingLayerName = obstacleSortingLayer;
            renderer.sortingOrder = obstacleOrderInLayer;
        }
        
        Debug.Log($"Set {renderers.Length} renderer(s) to layer '{obstacleSortingLayer}' order {obstacleOrderInLayer}");
    }
    
    void SetNextSpawnTime()
    {
        float interval = Random.Range(minSpawnInterval, maxSpawnInterval);
        
        if (AdventureManager.Instance != null)
        {
            float speedFactor = AdventureManager.Instance.GetGameSpeed() / 5f;
            interval = Mathf.Max(interval / speedFactor, 0.8f);
        }
        
        nextSpawnTime = Time.time + interval;
    }
}
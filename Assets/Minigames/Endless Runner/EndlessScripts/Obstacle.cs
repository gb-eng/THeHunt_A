using UnityEngine;

public class Obstacle : MonoBehaviour
{
    private float moveSpeed;
    
    void Start()
    {
        if (AdventureManager.Instance != null)
        {
            moveSpeed = AdventureManager.Instance.GetGameSpeed();
        }
        else
        {
            moveSpeed = 6f; // Fallback speed
            Debug.LogWarning("AdventureManager not found! Using default speed.");
        }
    }
    
    void Update()
    {
        // Move obstacle to the LEFT (coming from right side)
        if (AdventureManager.Instance != null)
        {
            moveSpeed = AdventureManager.Instance.GetGameSpeed();
        }
        
        transform.Translate(Vector3.left * moveSpeed * Time.deltaTime);
        
        // Destroy when off-screen (passed the player on left side)
        if (transform.position.x < -15f)
        {
            Destroy(gameObject);
        }
    }
}
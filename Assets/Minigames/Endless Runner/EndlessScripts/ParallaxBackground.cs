using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    public float parallaxFactor = 0.5f; // 0 = static, 1 = moves with game speed
    private Material mat;
    private float offset;

    void Start()
    {
        Renderer rend = GetComponent<Renderer>();
        if (rend) mat = rend.material;
    }

    void Update()
    {
        if (AdventureManager.Instance != null && !AdventureManager.Instance.IsGameOver())
        {
            // Scroll texture based on Game Speed
            float speed = AdventureManager.Instance.GetGameSpeed() * parallaxFactor;
            offset += (speed * Time.deltaTime) / 10f; // Divide by 10 to normalize texture coordinates
            
            if (mat != null) mat.mainTextureOffset = new Vector2(offset, 0);
        }
    }
}
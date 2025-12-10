using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float jumpForce = 15f;
    public float gravity = 3f;
    
    [Header("Ground Detection - IMPORTANT")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.3f;
    public LayerMask groundLayer;
    [Tooltip("Additional downward raycast distance")]
    public float groundRaycastDistance = 1.0f;
    
    [Header("Jump Settings")]
    public float jumpCooldown = 0.2f; // ✅ Time to wait before checking ground again
    private float lastJumpTime;       // ✅ Tracks when we last jumped

    [Header("Advanced Ground Detection")]
    [Tooltip("Enable to see detailed ground detection info in Console")]
    public bool debugMode = false;
    [Tooltip("Use relaxed ground detection (more forgiving)")]
    public bool relaxedDetection = true;
    
    [Header("Animation")]
    public Animator animator;
    public SpriteRenderer spriteRenderer;
    
    private Rigidbody2D rb;
    private bool isGrounded;
    private bool isAlive = true;
    
    private PlayerInput playerInput;
    private InputAction jumpAction;
    
    // Debug info
    private bool lastCircleCheck, lastRayCheck, lastBoxCheck;
    
    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        if (playerInput != null)
        {
            jumpAction = playerInput.actions["Jump"];
        }
    }
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = gravity;
        
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
            
        ValidateSetup();
    }
    
    void ValidateSetup()
    {
        if (groundCheck == null) Debug.LogError("❌ CRITICAL: GroundCheck not assigned!");
        if (groundLayer == 0) Debug.LogError("❌ CRITICAL: Ground Layer not set!");
        if (rb == null) Debug.LogError("❌ CRITICAL: Rigidbody2D not found!");
    }
    
    void Update()
    {
        if (!isAlive) return;
        
        CheckGroundStatus();
        
        // Check for Jump Input (Supports Keyboard, Mouse, Touch, Gamepad)
        bool jumpPressed = false;
        
        if (jumpAction != null && jumpAction.WasPressedThisFrame()) jumpPressed = true;
        else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) jumpPressed = true;
        else if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame) jumpPressed = true;
        else if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame) jumpPressed = true;
        
        if (jumpPressed)
        {
            if (isGrounded)
            {
                Jump();
            }
        }
        
        UpdateAnimation();
    }
    
    void CheckGroundStatus()
    {
        // ✅ 1. COOLDOWN CHECK: If we just jumped, pretend we are flying
        if (Time.time < lastJumpTime + jumpCooldown) 
        {
            isGrounded = false;
            return;
        }

        if (groundCheck == null)
        {
            isGrounded = false;
            return;
        }
        
        // Method 1: Circle overlap
        lastCircleCheck = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        
        // Method 2: Raycast
        RaycastHit2D rayHit = Physics2D.Raycast(groundCheck.position, Vector2.down, groundRaycastDistance, groundLayer);
        lastRayCheck = rayHit.collider != null;
        
        // Method 3: Box cast
        Vector2 boxSize = new Vector2(groundCheckRadius * 2, 0.1f);
        RaycastHit2D boxHit = Physics2D.BoxCast(groundCheck.position, boxSize, 0f, Vector2.down, groundRaycastDistance, groundLayer);
        lastBoxCheck = boxHit.collider != null;
        
        // Method 4: Sphere cast
        RaycastHit2D sphereHit = Physics2D.CircleCast(groundCheck.position, groundCheckRadius, Vector2.down, groundRaycastDistance, groundLayer);
        bool sphereCheck = sphereHit.collider != null;
        
        if (relaxedDetection)
        {
            isGrounded = lastCircleCheck || lastRayCheck || lastBoxCheck || sphereCheck;
        }
        else
        {
            int detectionCount = 0;
            if (lastCircleCheck) detectionCount++;
            if (lastRayCheck) detectionCount++;
            if (lastBoxCheck) detectionCount++;
            if (sphereCheck) detectionCount++;
            isGrounded = detectionCount >= 2;
        }
    }
    
    void Jump()
    {
        // ✅ 2. PREVENT SPAM: Double check cooldown
        if (Time.time < lastJumpTime + jumpCooldown) return;
        
        lastJumpTime = Time.time;
        
        // ✅ 3. APPLY FORCE
        // Using linearVelocity for Unity 6+ (use velocity for older versions)
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        
        // ✅ 4. FORCE UNGROUNDED: Instantly tell script we are in the air
        isGrounded = false;
        
        if (animator != null) animator.SetTrigger("Jump");
    }
    
    void UpdateAnimation()
    {
        if (animator != null)
        {
            animator.SetBool("IsGrounded", isGrounded);
            animator.SetFloat("VerticalSpeed", rb.linearVelocity.y);
        }
    }
    
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Obstacle"))
        {
            TakeDamage();
        }
    }
    
    void TakeDamage()
    {
        if (AdventureManager.Instance != null)
        {
            AdventureManager.Instance.LoseLife();
        }
        if (spriteRenderer != null) StartCoroutine(FlashEffect());
    }
    
    System.Collections.IEnumerator FlashEffect()
    {
        for (int i = 0; i < 3; i++)
        {
            spriteRenderer.color = new Color(1, 1, 1, 0.5f);
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(0.1f);
        }
    }
    
    public void Die()
    {
        isAlive = false;
        if (animator != null) animator.SetTrigger("Death");
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0;
    }
    
    void OnDrawGizmos()
    {
        if (groundCheck == null) return;
        Gizmos.color = lastCircleCheck ? Color.green : Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
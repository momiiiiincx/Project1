using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerController: MonoBehaviour
{
    public enum PlayerState { Idle, Run, Jump, Fall, WallSlide }
    private PlayerState currentState;

    [Header("Movement")]
    public float moveSpeed = 6f;
    public float jumpForce = 14f;

    [Header("Wall Settings")]
    public float wallCheckDistance = 0.6f;
    public float wallSlideSpeed = 2f;
    public LayerMask wallLayer;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("Health & UI")]
    public int health = 3;
    public Image[] heartImages;
    public GameObject gameOverUI;
    public float invincibilityTime = 1.0f;
    private bool isInvincible = false;

    [Header("Effects")]
    public ParticleSystem walkDust; // ลาก Particle System มาใส่ที่นี่

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Animator anim;

    private float moveInput;
    private bool isGrounded;
    private bool isTouchingWall;
    private bool isWallSliding;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();

        if (gameOverUI != null) gameOverUI.SetActive(false);
    }

    void Update()
    {
        GetInput();
        CheckGround();
        CheckWall();
        HandleJump();
        HandleWallSlide();

        // จัดการ Effect และ Animation
        HandleParticles();
        DetermineState();
    }

    void FixedUpdate() => Move();

    void GetInput()
    {
        moveInput = Input.GetAxisRaw("Horizontal");

        // ถ้าเดินไปทางขวา (ค่าบวก) ให้หมุนตัวไปที่ 0 องศา
        if (moveInput > 0.1f)
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        // ถ้าเดินไปทางซ้าย (ค่าลบ) ให้หมุนตัวไปที่ 180 องศา
        else if (moveInput < -0.1f)
        {
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }
    }

    void Move()
    {
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
    }

    void HandleJump()
    {
        if (Input.GetButtonDown("Jump"))
        {
            if (isGrounded)
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            else if (isWallSliding)
                rb.linearVelocity = new Vector2((sr.flipX ? 1 : -1) * moveSpeed, jumpForce);
        }
    }

    void CheckGround() => isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

    void CheckWall()
    {
        Vector2 direction = sr.flipX ? Vector2.left : Vector2.right;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, wallCheckDistance, wallLayer);
        isTouchingWall = hit.collider != null;
    }

    void HandleWallSlide()
    {
        if (isTouchingWall && !isGrounded && rb.linearVelocity.y < 0)
        {
            isWallSliding = true;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -wallSlideSpeed);
        }
        else isWallSliding = false;
    }

    // --- ระบบ Particle ---
    void HandleParticles()
    {
        if (walkDust == null) return;

        // ถ้ามีการเดิน (moveInput != 0) และอยู่บนพื้น
        if (Mathf.Abs(moveInput) > 0.1f && isGrounded)
        {
            if (!walkDust.isPlaying) walkDust.Play();
        }
        else
        {
            if (walkDust.isPlaying) walkDust.Stop();
        }
    }

    // --- ระบบความเสียหายและการตาย ---
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Trap") && !isInvincible)
        {
            TakeDamage();
        }
    }

    void TakeDamage()
    {
        health--;
        UpdateHealthUI();

        if (health <= 0) Die();
        else StartCoroutine(InvincibilityRoutine());
    }

    void UpdateHealthUI()
    {
        for (int i = 0; i < heartImages.Length; i++)
        {
            if (i < health) heartImages[i].enabled = true;
            else heartImages[i].enabled = false;
        }
    }

    void Die()
    {
        if (gameOverUI != null) gameOverUI.SetActive(true);
        Destroy(gameObject);
    }

    IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;
        for (float i = 0; i < invincibilityTime; i += 0.1f)
        {
            sr.enabled = !sr.enabled;
            yield return new WaitForSeconds(0.1f);
        }
        sr.enabled = true;
        isInvincible = false;
    }

    // --- State Machine Animation ---
    void DetermineState()
    {
        PlayerState newState;
        if (isWallSliding) newState = PlayerState.WallSlide;
        else if (isGrounded)
        {
            if (Mathf.Abs(moveInput) > 0.1f) newState = PlayerState.Run;
            else newState = PlayerState.Idle;
        }
        else
        {
            if (rb.linearVelocity.y > 0.1f) newState = PlayerState.Jump;
            else newState = PlayerState.Fall;
        }
        ChangeAnimationState(newState);
    }

    void ChangeAnimationState(PlayerState newState)
    {
        if (currentState == newState) return;
        anim.Play(newState.ToString());
        currentState = newState;
    }
}
using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class PlayerController : MonoBehaviour
{
    public event EventHandler OnDied;
    public event EventHandler OnFellIntoVoid;
    public event EventHandler OnRespawned;
    public event EventHandler OnJumped;
    public event EventHandler OnWallJumped;
    public event EventHandler OnDoubleJumped;
    public event EventHandler OnLanded;

    public enum PlayerState { Appearing, Idle, Running, Jumping, DoubleJumping, Falling, WallSliding, Dead, Disappearing }

    public static PlayerController Instance { get; private set; }

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float jumpForce = 12f;

    [Header("Wall Movement")]
    [SerializeField] private float wallJumpForceX = 10f;
    [SerializeField] private float wallJumpForceY = 14f;
    [SerializeField] private float wallSlideSpeed = -3f;

    [Header("Feel Mechanics")]
    [SerializeField] private float coyoteTime = 0.12f;
    [SerializeField] private float jumpBufferTime = 0.10f;
    [SerializeField] private float fastFallMultiplier = 2.5f;
    [SerializeField][Range(0f, 1f)] private float doubleJumpForceMultiplier = 0.85f;
    [SerializeField][Range(0f, 1f)] private float airControlLerp = 1f;
    [SerializeField] private float respawnDelay = 0.5f;
    [SerializeField] private float hazardHitUpwardForce = 10f;
    [SerializeField] private float hazardHitMinAngularVelocity = 200f;
    [SerializeField] private float hazardHitMaxAngularVelocity = 600f;

    [Header("Detection")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float groundCheckDistance = 0.05f;
    [SerializeField] private float wallCheckDistance = 0.05f;


    // ── Components ────────────────────────────────────────────────────────────

    private Rigidbody2D rb;
    public Rigidbody2D Rb => rb;
    private BoxCollider2D col;

    // ── Input ─────────────────────────────────────────────────────────────────

    private Vector2 moveInput;

    // ── State ─────────────────────────────────────────────────────────────────

    public PlayerState CurrentPlayerState { get; set; }

    // ── Timers ────────────────────────────────────────────────────────────────

    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private bool canDoubleJump;

    // ── Moving Platform ───────────────────────────────────────────────────────
    private MovingPlatform currentPlatform;

    // ── Wall ──────────────────────────────────────────────────────────────────

    private bool isTouchingWall;
    private bool isPressingIntoWall;
    private int wallDirection;        // 1 = right wall, -1 = left wall

    private float wallJumpLockTimer;
    private const float WallJumpLockDuration = 0.35f;


    // ── Rigidbody ───────────────────────────────────────────────────────────────

    private float originalGravityScale;
    private RigidbodyConstraints2D originalConstraints;

    // ── Start/End Checkpoint ─────────────────────────────────────────────────
    public bool HasStarted { get; set; } = false;
    public bool HasReachedEnd { get; set; } = false;

    // ── Public Read-Only  ─────────────────────────────────────────────────────

    public float XSpeed => Mathf.Abs(rb.linearVelocity.x);
    public float YVelocity => rb.linearVelocity.y;
    public bool IsGrounded { get; private set; }
    public bool IsWallSliding => CurrentPlayerState == PlayerState.WallSliding;
    public bool IsFacingRight { get; private set; } = true;
    public bool IsDead { get; private set; }
    public int DeathCount { get; private set; }

    // =========================================================================
    // Unity Lifecycle
    // =========================================================================

    private void Awake()
    {
        Instance = this;

        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
        originalGravityScale = rb.gravityScale;
        originalConstraints = rb.constraints;
        CurrentPlayerState = PlayerState.Appearing;
    }

    private void Start()
    {
        GameInput.Instance.EnablePlayerInput();
        GameInput.Instance.OnJumpStarted += GameInput_OnJumpStarted;

        transform.position = CheckpointManager.Instance.GetRespawnPosition();
    }

    private void Update()
    {
        if (IsDead) return;

        if (CurrentPlayerState == PlayerState.Appearing || CurrentPlayerState == PlayerState.Disappearing)
        {
            // During appearing/disappearing, ignore input and keep player locked in place.
            if (rb.gravityScale != 0f)
                rb.gravityScale = 0f;
            if (rb.linearVelocity != Vector2.zero)
                rb.linearVelocity = Vector2.zero;
            return;
        }
        else
        {
            // Ensure gravity is normal after appearing/disappearing finishes
            if (rb.gravityScale == 0f)
                rb.gravityScale = originalGravityScale;
        }

        moveInput = GameInput.Instance.MoveInput;

        bool wasGroundedLastFrame = IsGrounded;

        CheckGrounded();

        if (!wasGroundedLastFrame && IsGrounded)
        {
            OnLanded?.Invoke(this, EventArgs.Empty);
        }

        CheckWall();
        UpdateCoyoteTime();
        UpdateJumpBuffer();
        HandleJump();
        HandleWallSlide();
        HandleFlip();

        if (wallJumpLockTimer > 0f)
            wallJumpLockTimer -= Time.deltaTime;

        UpdateState();
    }

    private void FixedUpdate()
    {
        if (CurrentPlayerState == PlayerState.Appearing || CurrentPlayerState == PlayerState.Disappearing || IsDead || !HasStarted || HasReachedEnd) return;
        HandleMovement();
        HandleFastFall();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out Spikes _))
        {
            HandleHazardCollision(other);
        }

        if (other.TryGetComponent(out EndCheckpoint _))
        {
            HasReachedEnd = true;
            rb.linearVelocity = Vector2.zero;
            StartCoroutine(DisappearRoutine());
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.TryGetComponent(out StartCheckpoint _) && !HasStarted)
        {
            HasStarted = true;
        }
    }

    private IEnumerator DisappearRoutine()
    {
        float disappearingDelay = 0.5f;
        yield return new WaitForSeconds(disappearingDelay);
        CurrentPlayerState = PlayerState.Disappearing;
    }

    private void OnDestroy()
    {
        if (GameInput.Instance == null) return;
        GameInput.Instance.OnJumpStarted -= GameInput_OnJumpStarted;
    }

    private void HandleHazardCollision(Collider2D hazardCollider)
    {
        if (IsDead) return;

        Die();

        // Determine closest contact point on the hazard collider relative to player center
        Vector2 contactPoint = hazardCollider.ClosestPoint(transform.position);
        bool hazardBelow = contactPoint.y < transform.position.y - 0.05f;

        if (hazardBelow)
        {
            // Allow rotation so the player can spin
            rb.constraints = RigidbodyConstraints2D.None;

            // Remove vertical velocity, reduce horizontal velocity, and apply an upward impulse
            rb.linearVelocity = new Vector2(rb.linearVelocity.x * 0.5f, 0f);
            rb.AddForce(Vector2.up * hazardHitUpwardForce, ForceMode2D.Impulse);
        }
        // Random angular velocity for a nice spin
        float ang = UnityEngine.Random.Range(hazardHitMinAngularVelocity, hazardHitMaxAngularVelocity);
        if (UnityEngine.Random.value < 0.5f) ang = -ang;
        rb.angularVelocity = ang;
    }

    // =========================================================================
    // State Machine — describes what's happening for the animator.
    // =========================================================================

    private void UpdateState()
    {
        // Wall slide takes priority over all airborne states
        if (isTouchingWall && isPressingIntoWall && !IsGrounded && rb.linearVelocity.y <= 1f)
        {
            CurrentPlayerState = PlayerState.WallSliding;
        }
        else if (!IsGrounded && rb.linearVelocity.y > 0f && CurrentPlayerState == PlayerState.DoubleJumping)
        {
            // Stay in DoubleJumping while still rising from the double jump
        }
        else if (!IsGrounded && rb.linearVelocity.y > 0f)
        {
            CurrentPlayerState = PlayerState.Jumping;
        }
        else if (!IsGrounded && rb.linearVelocity.y < 0f)
        {
            CurrentPlayerState = PlayerState.Falling;
        }
        else if (IsGrounded && moveInput.x != 0 && Mathf.Abs(rb.linearVelocity.x) > 0.01f)
        {
            CurrentPlayerState = PlayerState.Running;
        }
        else
        {
            CurrentPlayerState = PlayerState.Idle;
        }
    }

    // =========================================================================
    // Input Callbacks
    // =========================================================================

    private void GameInput_OnJumpStarted(object sender, EventArgs e)
    {
        if (IsDead) return;
        // Just register the buffer — HandleJump() in Update will consume it.
        jumpBufferCounter = jumpBufferTime;
    }

    // =========================================================================
    // Detection
    // =========================================================================

    private void CheckGrounded()
    {
        Bounds b = col.bounds;

        RaycastHit2D hit = Physics2D.BoxCast(
            b.center,
            new Vector2(b.size.x * 0.9f, b.size.y),
            0f,
            Vector2.down,
            groundCheckDistance,
            groundLayer
        );

        IsGrounded = hit.collider != null;

        if (IsGrounded)
        {
            currentPlatform = hit.collider.GetComponent<MovingPlatform>();
        }
        else
        {
            currentPlatform = null;
        }
    }

    private void CheckWall()
    {
        Bounds b = col.bounds;
        Vector2 castSize = new Vector2(b.size.x, b.size.y * 0.9f);

        RaycastHit2D rightHit = Physics2D.BoxCast(b.center, castSize, 0f, Vector2.right, wallCheckDistance, wallLayer);
        RaycastHit2D leftHit = Physics2D.BoxCast(b.center, castSize, 0f, Vector2.left, wallCheckDistance, wallLayer);

        if (rightHit.collider != null) { isTouchingWall = true; wallDirection = 1; }
        else if (leftHit.collider != null) { isTouchingWall = true; wallDirection = -1; }
        else { isTouchingWall = false; wallDirection = 0; }

        isPressingIntoWall = isTouchingWall
            && ((wallDirection == 1 && moveInput.x > 0.01f)
            || (wallDirection == -1 && moveInput.x < -0.01f));
    }

    // =========================================================================
    // Movement
    // =========================================================================

    private void HandleMovement()
    {
        if (wallJumpLockTimer > 0f) return;

        float targetX = moveInput.x * moveSpeed;

        float platformCarryX = 0f;
        if (IsGrounded && currentPlatform != null)
        {
            platformCarryX = currentPlatform.DeltaMovement.x / Time.fixedDeltaTime;
        }

        if (IsGrounded)
        {
            rb.linearVelocity = new Vector2(
                targetX + platformCarryX,
                rb.linearVelocity.y
            );
        }
        else
        {
            float smoothedX = Mathf.Lerp(rb.linearVelocity.x, targetX, airControlLerp);
            rb.linearVelocity = new Vector2(smoothedX, rb.linearVelocity.y);
        }
    }

    private void HandleFlip()
    {
        if (wallJumpLockTimer > 0f) return;

        if (moveInput.x > 0.01f && !IsFacingRight) Flip();
        else if (moveInput.x < -0.01f && IsFacingRight) Flip();
    }

    private void Flip()
    {
        IsFacingRight = !IsFacingRight;
        Vector3 s = transform.localScale;
        s.x *= -1f;
        transform.localScale = s;
    }

    // =========================================================================
    // Jump
    // =========================================================================

    private void UpdateCoyoteTime()
    {
        if (IsGrounded)
        {
            coyoteTimeCounter = coyoteTime;
            canDoubleJump = true;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }
    }

    private void UpdateJumpBuffer()
    {
        if (jumpBufferCounter > 0f)
            jumpBufferCounter -= Time.deltaTime;
    }

    private void HandleJump()
    {
        if (jumpBufferCounter <= 0f) return;

        // Wall jump
        if (isTouchingWall && !IsGrounded)
        {
            rb.linearVelocity = new Vector2(-wallDirection * wallJumpForceX, wallJumpForceY);
            jumpBufferCounter = 0f;
            coyoteTimeCounter = 0f;
            wallJumpLockTimer = WallJumpLockDuration;

            // Face the direction we're jumping toward (away from the wall)
            bool shouldFaceRight = wallDirection == -1;
            if (shouldFaceRight != IsFacingRight) Flip();

            OnWallJumped?.Invoke(this, EventArgs.Empty);

            return;
        }

        // Regular jump (includes coyote time)
        if (coyoteTimeCounter > 0f && IsGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpBufferCounter = 0f;
            coyoteTimeCounter = 0f;
            OnJumped?.Invoke(this, EventArgs.Empty);
            return;
        }

        // Double jump
        if (canDoubleJump)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce * doubleJumpForceMultiplier);
            jumpBufferCounter = 0f;
            canDoubleJump = false;
            CurrentPlayerState = PlayerState.DoubleJumping;
            OnDoubleJumped?.Invoke(this, EventArgs.Empty);
        }
    }

    // =========================================================================
    // Wall Slide
    // =========================================================================

    private void HandleWallSlide()
    {
        if (!isTouchingWall || !isPressingIntoWall || IsGrounded || rb.linearVelocity.y > 1f || wallJumpLockTimer > 0f) return;

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, wallSlideSpeed);
    }

    // =========================================================================
    // Fast Fall
    // =========================================================================

    private void HandleFastFall()
    {
        if (moveInput.y >= -0.5f || IsGrounded || rb.linearVelocity.y >= 0f) return;

        float extraGravity = Mathf.Abs(Physics2D.gravity.y)
                             * rb.gravityScale
                             * (fastFallMultiplier - 1f);

        rb.linearVelocity += Vector2.down * extraGravity * Time.fixedDeltaTime;
    }

    // =========================================================================
    // Death and Respawn
    // =========================================================================

    public void Die(bool isVoidFall = false)
    {
        if (IsDead) return;
        DeathCount++;
        IsDead = true;
        col.enabled = false;

        if (!isVoidFall)
        {
            OnDied?.Invoke(this, EventArgs.Empty);
            CurrentPlayerState = PlayerState.Dead;
        }
        else
        {
            OnFellIntoVoid?.Invoke(this, EventArgs.Empty);
        }

        StartCoroutine(RespawnAfterDelay());
    }

    public void Respawn(Vector3 position)
    {
        IsDead = false;

        transform.position = position;
        col.enabled = true;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        transform.rotation = Quaternion.identity;
        rb.constraints = originalConstraints;

        isTouchingWall = false;
        isPressingIntoWall = false;
        canDoubleJump = false;
        wallDirection = 0;
        coyoteTimeCounter = 0f;
        jumpBufferCounter = 0f;
        wallJumpLockTimer = 0f;

        CurrentPlayerState = PlayerState.Appearing;

        OnRespawned?.Invoke(this, EventArgs.Empty);
    }

    private IEnumerator RespawnAfterDelay()
    {
        yield return new WaitForSeconds(respawnDelay);

        Vector3 respawnPosition = CheckpointManager.Instance.GetRespawnPosition();
        Respawn(respawnPosition);
    }
}
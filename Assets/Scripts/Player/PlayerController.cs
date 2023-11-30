using UnityEngine;

public class PlayerController : MonoBehaviour {
    public float mass = 4.0f;
    private float size = 4.0f;
    private float moveForce = 1.0f;
    private float jumpForce = 10.0f;
    private float maxJumpTime = 0.5f;
    private float maxMomentum = 10.0f;
    private float playerRadius = 0.9f;
    private float decelerationRate = 5.0f;

    public float sizeScale = 5.0f;
    private float sizeScaleMin = 1.0f;
    private float sizeScaleMax = 10.0f;

    public float sizeChangeSpeed = 10.0f;
    public float groundCheckAngle = 60.0f;
    public float groundCheckRayCount = 13f;
    public float continuousJumpFactor = 2f;
    public float momentumDecreaseFactor = 0.8f;

    [Header("Min - Max Variables")]
    public float minSize = 0.3f;
    public float maxSize = 0.7f;
    public float minMass = 0.5f;
    public float maxMass = 4.0f;
    public float minJumpForce = 2.0f;
    public float maxJumpForce = 25.0f;
    public float minMoveForce = 10.0f;
    public float maxMoveForce = 30.0f;
    public float minMaxJumpTime = 0.3f;
    public float maxMaxJumpTime = 0.5f;
    public float minMaxMomentum = 10.0f;
    public float maxMaxMomentum = 50.0f;
    public float minplayerRadius = 0.35f;
    public float maxplayerRadius = 1.40f;
    public float minDecelerationRate = 2f;
    public float maxDecelerationRate = 8.0f;

    public LayerMask groundLayer;

    private Player player;
    private Rigidbody2D rb;
    private bool isJumping = false;
    private bool isGrounded = false;

    private float jumpTime = 0.0f;
    private float moveInput = 0.0f;
    private bool continuousJumpInput = false;

    public SoundManager soundManager;

    private void Start() {
        rb = GetComponent<Rigidbody2D>();
        player = GetComponent<Player>();

        if (soundManager == null)
            soundManager = FindObjectOfType<SoundManager>();

        float boundSizeX = GetComponent<SpriteRenderer>().sprite.bounds.size.x;
        minplayerRadius = minSize * boundSizeX / 2 + 0.1f;
        maxplayerRadius = maxSize * boundSizeX / 2 + 0.1f;

        player.UpdateHealth(sizeScale, sizeScaleMin, sizeScaleMax);
        player.UpdateDamagePower(sizeScale, sizeScaleMin, sizeScaleMax);

        size = minSize + (sizeScale - sizeScaleMin) * (maxSize - minSize) / (sizeScaleMax - sizeScaleMin);
        mass = minMass + (sizeScale - sizeScaleMin) * (maxMass - minMass) / (sizeScaleMax - sizeScaleMin);
        moveForce = minMoveForce + (sizeScale - sizeScaleMin) * (maxMoveForce - minMoveForce) / (sizeScaleMax - sizeScaleMin);
        jumpForce = minJumpForce + (sizeScale - sizeScaleMin) * (maxJumpForce - minJumpForce) / (sizeScaleMax - sizeScaleMin);
        maxJumpTime = maxMaxJumpTime + (sizeScale - sizeScaleMin) * (minMaxJumpTime - maxMaxJumpTime) / (sizeScaleMax - sizeScaleMin);
        maxMomentum = minMaxMomentum + (sizeScale - sizeScaleMin) * (maxMaxMomentum - minMaxMomentum) / (sizeScaleMax - sizeScaleMin);
        playerRadius = minplayerRadius + (sizeScale - sizeScaleMin) * (maxplayerRadius - minplayerRadius) / (sizeScaleMax - sizeScaleMin);
        decelerationRate = minDecelerationRate + (sizeScale - sizeScaleMin) * (maxDecelerationRate - minDecelerationRate) / (sizeScaleMax - sizeScaleMin);

        transform.localScale = new Vector3(size, size, 1);
        rb.mass = mass;
    }

    private void Update() {
        moveInput = Input.GetAxis("Horizontal");
        continuousJumpInput = Input.GetButton("Jump");

        ChangeSize();
        InitialJump();
    }

    private void FixedUpdate() {
        Move();
        ContinuousJump();
    }

    private void Move() {
        Vector2 force = moveForce * moveInput * Vector2.right;
        float currentMomentum = rb.mass * rb.velocity.x;

        if (Mathf.Abs(currentMomentum + force.x) > maxMomentum) {
            force.x = (Mathf.Sign(moveInput) * maxMomentum - currentMomentum) * momentumDecreaseFactor;
        }
        if (!isGrounded) {
            force.x *= 0.5f;
            float decelerationForceX = -rb.velocity.x * decelerationRate * 0.2f;
            rb.AddForce(Vector2.right * decelerationForceX);
        }

        rb.AddForce(force);

        if (moveInput == 0 || moveInput * rb.velocity.x < 0) {
            float decelerationForceX = -rb.velocity.x * decelerationRate;
            rb.AddForce(Vector2.right * decelerationForceX);
        }
    }

    private void InitialJump() {
        bool jumpInput = Input.GetButtonDown("Jump");

        if (jumpInput && isGrounded && !isJumping) {
            soundManager.PlayJumpSound();
            isJumping = true;
            jumpTime = 0.0f;

            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            jumpTime += Time.deltaTime;
            isGrounded = false;
        }
    }

    private void ContinuousJump() {
        if (continuousJumpInput && isJumping && jumpTime < maxJumpTime) {
            rb.AddForce(continuousJumpFactor * jumpForce * Vector2.up);
            jumpTime += Time.deltaTime;
        }
    }

    private bool CanGrow() {
        return !((Physics2D.Raycast(transform.position, Vector2.up, playerRadius, groundLayer) &&
            Physics2D.Raycast(transform.position, Vector2.down, playerRadius, groundLayer)) ||
            (Physics2D.Raycast(transform.position, Vector2.left, playerRadius, groundLayer) &&
            Physics2D.Raycast(transform.position, Vector2.right, playerRadius, groundLayer)));
    }

    private void ChangeSize() {
        float sizeInput = Input.GetAxis("Vertical") * sizeChangeSpeed;

        if (Mathf.Abs(sizeInput) >= 0.3) {
            if (sizeInput < 0 || CanGrow()) {
                sizeScale = Mathf.Clamp(sizeScale + sizeInput * Time.deltaTime, sizeScaleMin, sizeScaleMax);

                player.UpdateHealth(sizeScale, sizeScaleMin, sizeScaleMax);
                player.UpdateDamagePower(sizeScale, sizeScaleMin, sizeScaleMax);

                size = minSize + (sizeScale - sizeScaleMin) * (maxSize - minSize) / (sizeScaleMax - sizeScaleMin);
                mass = minMass + (sizeScale - sizeScaleMin) * (maxMass - minMass) / (sizeScaleMax - sizeScaleMin);
                moveForce = minMoveForce + (sizeScale - sizeScaleMin) * (maxMoveForce - minMoveForce) / (sizeScaleMax - sizeScaleMin);
                jumpForce = minJumpForce + (sizeScale - sizeScaleMin) * (maxJumpForce - minJumpForce) / (sizeScaleMax - sizeScaleMin);
                maxJumpTime = maxMaxJumpTime + (sizeScale - sizeScaleMin) * (minMaxJumpTime - maxMaxJumpTime) / (sizeScaleMax - sizeScaleMin);
                maxMomentum = minMaxMomentum + (sizeScale - sizeScaleMin) * (maxMaxMomentum - minMaxMomentum) / (sizeScaleMax - sizeScaleMin);
                playerRadius = minplayerRadius + (sizeScale - sizeScaleMin) * (maxplayerRadius - minplayerRadius) / (sizeScaleMax - sizeScaleMin);
                decelerationRate = minDecelerationRate + (sizeScale - sizeScaleMin) * (maxDecelerationRate - minDecelerationRate) / (sizeScaleMax - sizeScaleMin);

                transform.localScale = new Vector3(size, size, 1);
                rb.mass = mass;
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        float angleIncrement = groundCheckAngle / (groundCheckRayCount - 1);
        int i = 0;

        for (i = 0; i < groundCheckRayCount; i++) {
            float angle = -groundCheckAngle / 2.0f + i * angleIncrement;
            Vector2 direction = Quaternion.Euler(0, 0, angle) * Vector2.down;

            if (Physics2D.Raycast(transform.position, direction, playerRadius, groundLayer)) {
                isGrounded = true;
                isJumping = false;
                jumpTime = 0.0f;
                break;
            }
        }

        if (i == groundCheckRayCount) {
            isJumping = true;
            isGrounded = false;
        }
    }

    private void OnCollisionExit2D(Collision2D collision) {
        float angleIncrement = groundCheckAngle / (groundCheckRayCount - 1);
        int i = 0;

        for (i = 0; i < groundCheckRayCount; i++) {
            float angle = -groundCheckAngle / 2.0f + i * angleIncrement;
            Vector2 direction = Quaternion.Euler(0, 0, angle) * Vector2.down;

            if (Physics2D.Raycast(transform.position, direction, playerRadius, groundLayer)) {
                isGrounded = true;
                isJumping = false;
                jumpTime = 0.0f;
                break;
            }
        }

        if (i == groundCheckRayCount) {
            isJumping = true;
            isGrounded = false;
        }
    }
}

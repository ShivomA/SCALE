using UnityEngine;

public class PlayerController : MonoBehaviour {
    private float mass = 4.0f;
    private float moveForce = 1.0f;
    private float jumpForce = 10.0f;
    private float maxJumpTime = 0.5f;
    private float maxMomentum = 10.0f;
    private float playerRadius = 0.9f;
    private float decelerationRate = 5.0f;

    public float sizeChangeSpeed = 1.0f;
    public float groundCheckAngle = 60.0f;
    public float groundCheckRayCount = 13f;
    public float decreasedJumpFactor = 0.5f;
    public float momentumDecreaseFactor = 0.8f;

    public float minSize = 0.2f;
    public float maxSize = 0.8f;
    public float minMass = 0.5f;
    public float maxMass = 4.0f;
    public float minMoveForce = 3.0f;
    public float maxMoveForce = 9.0f;
    public float minJumpForce = 2.0f;
    public float maxJumpForce = 25.0f;
    public float minMaxJumpTime = 0.3f;
    public float maxMaxJumpTime = 0.5f;
    public float minMaxMomentum = 5.0f;
    public float maxMaxMomentum = 30.0f;
    public float minplayerRadius = 0.35f;
    public float maxplayerRadius = 1.40f;
    public float minDecelerationRate = 2f;
    public float maxDecelerationRate = 8.0f;

    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private bool isJumping = false;
    private bool isGrounded = false;

    private float jumpTime = 0.0f;
    private float moveInput = 0.0f;

    private void Start() {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update() {
        moveInput = Input.GetAxis("Horizontal");

        Jump();
        ChangeSize();
    }

    private void FixedUpdate() {
        Move();
    }

    private void Move() {
        Vector2 force = moveForce * moveInput * Vector2.right;
        float currentMomentum = rb.mass * rb.velocity.x;

        if (Mathf.Abs(currentMomentum + force.x) > maxMomentum) {
            force.x = (Mathf.Sign(moveInput) * maxMomentum - currentMomentum) * momentumDecreaseFactor;
        }
        rb.AddForce(force);

        if (moveInput == 0 || moveInput * rb.velocity.x < 0) {
            float decelerationForceX = -rb.velocity.x * decelerationRate;
            rb.AddForce(new Vector2(decelerationForceX, 0));
        }
    }

    private void Jump() {
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

        if (Input.GetButtonDown("Jump") && isGrounded && !isJumping) {
            isJumping = true;
            jumpTime = 0.0f;

            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            jumpTime += Time.deltaTime;
            isGrounded = false;
        } else if (Input.GetButton("Jump") && isJumping && jumpTime < maxJumpTime) {
            rb.AddForce(decreasedJumpFactor * jumpForce * Vector2.up);
            jumpTime += Time.deltaTime;
        }
    }

    private void ChangeSize() {
        float sizeInput = Input.GetAxis("Vertical") * sizeChangeSpeed;

        if (sizeInput != 0) {
            if (!Physics2D.Raycast(transform.position, Vector2.up, playerRadius, groundLayer)) {
                float newSize = Mathf.Clamp(transform.localScale.x + sizeInput * Time.deltaTime, minSize, maxSize);

                mass = minMass + (newSize - minSize) * (maxMass - minMass) / (maxSize - minSize);
                moveForce = minMoveForce + (newSize - minSize) * (maxMoveForce - minMoveForce) / (maxSize - minSize);
                jumpForce = minJumpForce + (newSize - minSize) * (maxJumpForce - minJumpForce) / (maxSize - minSize);
                maxJumpTime = maxMaxJumpTime + (newSize - minSize) * (minMaxJumpTime - maxMaxJumpTime) / (maxSize - minSize);
                maxMomentum = minMaxMomentum + (newSize - minSize) * (maxMaxMomentum - minMaxMomentum) / (maxSize - minSize);
                playerRadius = minplayerRadius + (newSize - minSize) * (maxplayerRadius - minplayerRadius) / (maxSize - minSize);
                decelerationRate = minDecelerationRate + (newSize - minSize) * (maxDecelerationRate - minDecelerationRate) / (maxSize - minSize);

                transform.localScale = new Vector3(newSize, newSize, 1);
                rb.mass = mass;
            }
        }
    }
}

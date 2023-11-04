using UnityEngine;

public class PlayerController : MonoBehaviour {
    private float mass = 4.0f;
    private float moveForce = 1.0f;
    private float jumpForce = 10.0f;
    private float maxMomentum = 10.0f;
    private float decelerationRate = 5.0f;

    public float maxJumpTime = 0.5f;
    public float sizeChangeSpeed = 5.0f;
    public float decreasedJumpFactor = 0.5f;
    public float momentumDecreaseFactor = 0.5f;

    public float minSize = 0.2f;
    public float maxSize = 0.8f;
    public float minMass = 0.5f;
    public float maxMass = 4.0f;
    public float minMoveForce = 3.0f;
    public float maxMoveForce = 9.0f;
    public float minJumpForce = 2.0f;
    public float maxJumpForce = 25.0f;
    public float minMaxMomentum = 5.0f;
    public float maxMaxMomentum = 30.0f;
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
        if (Input.GetButton("Jump") && isGrounded) {
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
        float newSize = Mathf.Clamp(transform.localScale.x + sizeInput * Time.deltaTime, minSize, maxSize);

        mass = minMass + (newSize - minSize) * (maxMass - minMass) / (maxSize - minSize);
        moveForce = minMoveForce + (newSize - minSize) * (maxMoveForce - minMoveForce) / (maxSize - minSize);
        jumpForce = minJumpForce + (newSize - minSize) * (maxJumpForce - minJumpForce) / (maxSize - minSize);
        maxMomentum = minMaxMomentum + (newSize - minSize) * (maxMaxMomentum - minMaxMomentum) / (maxSize - minSize);
        decelerationRate = minDecelerationRate + (newSize - minSize) * (maxDecelerationRate - minDecelerationRate) / (maxSize - minSize);

        transform.localScale = new Vector3(newSize, newSize, 1);
        rb.mass = mass;
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        if ((groundLayer & 1 << collision.gameObject.layer) != 0) {
            isGrounded = true;
            isJumping = false;
            jumpTime = 0.0f;
        }
    }

    private void OnCollisionExit2D(Collision2D collision) {
        if ((groundLayer & 1 << collision.gameObject.layer) != 0) {
            isGrounded = false;
        }
    }
}


using UnityEngine;

public class JumpingEnemy : MonoBehaviour {
    public int damage = 15;
    public int strength = 3;
    public int maxHealth = 30;
    public float radius = 1.2f;
    public float jumpForce = 10.0f;
    public float jumpCooldown = 1.2f;
    public float detectionRange = 8.0f;
    public float normalMaxSpeed = 1.5f;
    public float normalMoveForce = 10.0f;
    public float followingMaxSpeed = 4.0f;
    public float followingMoveForce = 0.5f;
    public float verticalDetectionRange = 10.0f;

    public float leftBoundary;
    public float rightBoundary;
    public float heightOffset = 1.5f;
    public Color damageColor = Color.red;
    public float damageVisualEffectTime = 2.0f;

    public float groundCheckAngle = 30.0f;
    public float groundCheckRayCount = 7f;

    private float steadyTime;
    private int currentHealth;
    private Color originalColor;
    private bool isGrounded = true;
    private float damageVisualEffectImpactTime;
    private SpriteRenderer enemySpriteRenderer;

    private Rigidbody2D rb;

    private bool movingRight = true;

    public Transform playerTransform;
    public LayerMask playerLayer;
    public LayerMask groundLayer;

    private void Start() {
        if (playerTransform == null)
            playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();

        enemySpriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = enemySpriteRenderer.color;

        if (leftBoundary == rightBoundary) {
            leftBoundary = transform.position.x - 15;
            rightBoundary = transform.position.x + 15;
        }
    }

    private void Update() {
        DamageVisual();
    }

    private void FixedUpdate() {
        MovementLogic();
    }

    private void MovementLogic() {
        bool outOfBoundary = false;

        if (transform.position.x < leftBoundary) {
            rb.AddForce(Vector2.right * normalMoveForce);
            Flip();
            outOfBoundary = true;
        } else if (transform.position.x > rightBoundary) {
            rb.AddForce(Vector2.left * normalMoveForce);
            Flip();
            outOfBoundary = true;
        }

        if (steadyTime <= jumpCooldown && isGrounded) {
            steadyTime += Time.deltaTime;
        }

        if (!outOfBoundary) {
            bool sawPlayer;

            if (Mathf.Abs(playerTransform.position.x - transform.position.x) < detectionRange &&
                Mathf.Abs(playerTransform.position.y - transform.position.y) < verticalDetectionRange) {
                sawPlayer = true;
            } else { sawPlayer = false; }

            if (sawPlayer) {
                if (steadyTime >= jumpCooldown && isGrounded) {
                    if (playerTransform.position.y >= transform.position.y - heightOffset) {
                        float xForce = followingMoveForce * (playerTransform.position.x - transform.position.x);
                        rb.AddForce(new Vector2(xForce, jumpForce), ForceMode2D.Impulse);
                    }

                    steadyTime = 0;
                    isGrounded = false;
                    rb.velocity = new Vector2(Mathf.Clamp(rb.velocity.x, -followingMaxSpeed, followingMaxSpeed), rb.velocity.y);
                }
            } else {
                if (movingRight) {
                    rb.AddForce(Vector2.right * normalMoveForce);
                    if (transform.position.x >= rightBoundary) {
                        Flip();
                    }
                } else {
                    rb.AddForce(Vector2.left * normalMoveForce);
                    if (transform.position.x <= leftBoundary) {
                        Flip();
                    }
                }
                rb.velocity = new Vector2(Mathf.Clamp(rb.velocity.x, -normalMaxSpeed, normalMaxSpeed), rb.velocity.y);
            }
        }
    }

    private void Flip() {
        movingRight = !movingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    private void DamageVisual() {
        if (damageVisualEffectImpactTime > 0) {
            damageVisualEffectImpactTime -= Time.deltaTime;

            if (damageVisualEffectImpactTime > 0) {
                float lerpValue = Mathf.PingPong(Time.time * 5.0f, 1.0f);
                enemySpriteRenderer.color = Color.Lerp(originalColor, damageColor, lerpValue);
            } else {
                enemySpriteRenderer.color = originalColor;
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        float angleIncrement = groundCheckAngle / (groundCheckRayCount - 1);
        int i = 0;

        for (i = 0; i < groundCheckRayCount; i++) {
            float angle = -groundCheckAngle / 2.0f + i * angleIncrement;
            Vector2 direction = Quaternion.Euler(0, 0, angle) * Vector2.down;

            if (Physics2D.Raycast(transform.position, direction, radius, groundLayer)) {
                isGrounded = true;
                break;
            }
        }

        if (i == groundCheckRayCount) {
            isGrounded = false;
        } else if (steadyTime >= jumpCooldown) {
            steadyTime = 0;
        }

        if (collision.gameObject.CompareTag("Player")) {
            if (collision.gameObject.TryGetComponent(out Player player)) {
                Transform playerTransform = collision.gameObject.transform;
                Rigidbody2D playerRb = collision.gameObject.GetComponent<Rigidbody2D>();
                PlayerController playerController = collision.gameObject.GetComponent<PlayerController>();

                Vector2 forceDirection = (playerTransform.position - transform.position).normalized * playerController.mass;
                playerRb.AddForce(new Vector2(forceDirection.x * player.damageTakenForceX, forceDirection.y * player.damageTakenForceY), ForceMode2D.Impulse);

                if (ShouldTakeDamage(playerController)) {
                    TakeDamage((int)playerController.damagePower);
                    damageVisualEffectImpactTime = damageVisualEffectTime;
                } else {
                    player.TakeDamage(damage);
                }
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision) {
        float angleIncrement = groundCheckAngle / (groundCheckRayCount - 1);
        int i = 0;

        for (i = 0; i < groundCheckRayCount; i++) {
            float angle = -groundCheckAngle / 2.0f + i * angleIncrement;
            Vector2 direction = Quaternion.Euler(0, 0, angle) * Vector2.down;

            if (Physics2D.Raycast(transform.position, direction, radius, groundLayer)) {
                isGrounded = true;
                break;
            }
        }

        if (i == groundCheckRayCount) {
            isGrounded = false;
        } else if (steadyTime >= jumpCooldown) {
            steadyTime = 0;
        }
    }

    private bool ShouldTakeDamage(PlayerController playerController) {
        if (Physics2D.Raycast(transform.position, Vector2.up, radius, playerLayer)) {
            if (playerController.damagePower >= strength) {
                return true;
            } else { return false; }
        } else
            return false;
    }

    public void TakeDamage(int damage) {
        currentHealth -= damage;

        if (currentHealth <= 0) {
            Die();
        }
    }

    private void Die() {
        Destroy(gameObject);
    }
}

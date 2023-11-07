
using UnityEngine;

public class FollowingEnemy : MonoBehaviour {
    public int damage = 10;
    public int strength = 4;
    public int maxHealth = 30;
    public float radius = 0.6f;
    public float detectionRange = 8f;
    public float normalMaxSpeed = 2.0f;
    public float normalMoveForce = 1.0f;
    public float followingMaxSpeed = 4.0f;
    public float followingMoveForce = 2.0f;
    public float verticalDetectionRange = 1.5f;

    public float leftBoundaryX;
    public float rightBoundaryX;
    public Color damageColor = Color.red;
    public float damageVisualEffectTime = 2.0f;

    private int currentHealth;
    private Color originalColor;
    private float damageVisualEffectImpactTime;
    private SpriteRenderer enemySpriteRenderer;

    private Rigidbody2D rb;

    private bool sawPlayer = false;
    private bool movingRight = true;

    public Transform playerTransform;
    public LayerMask playerLayer;

    private void Start() {
        if (playerTransform == null)
            playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();

        enemySpriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = enemySpriteRenderer.color;

        if (leftBoundaryX == 0 || rightBoundaryX == 0) {
            leftBoundaryX = transform.position.x - 15;
            rightBoundaryX = transform.position.x + 15;
        }
    }

    private void Update() {
        MovementLogic();
        DamageVisual();
    }

    private void MovementLogic() {
        if (Mathf.Abs(playerTransform.position.x - transform.position.x) < detectionRange &&
            Mathf.Abs(playerTransform.position.y - transform.position.y) < verticalDetectionRange) {
            sawPlayer = true;
        } else { sawPlayer = false; }

        if (sawPlayer) {
            if (transform.position.x >= leftBoundaryX && transform.position.x <= rightBoundaryX) {
                rb.AddForce(new Vector2(followingMoveForce * Mathf.Sign(playerTransform.position.x - transform.position.x), 0));
            }

            rb.velocity = new Vector2(Mathf.Clamp(rb.velocity.x, -followingMaxSpeed, followingMaxSpeed), rb.velocity.y);
        } else {
            if (movingRight) {
                rb.AddForce(new Vector2(normalMoveForce, 0));
                if (transform.position.x >= rightBoundaryX) {
                    Flip();
                }
            } else {
                rb.AddForce(new Vector2(-normalMoveForce, 0));
                if (transform.position.x <= leftBoundaryX) {
                    Flip();
                }
            }
            rb.velocity = new Vector2(Mathf.Clamp(rb.velocity.x, -normalMaxSpeed, normalMaxSpeed), rb.velocity.y);
        }

        if (transform.position.x < leftBoundaryX) {
            rb.AddForce(new Vector2(normalMoveForce, 0));
        } else if (transform.position.x > rightBoundaryX) {
            rb.AddForce(new Vector2(-normalMoveForce, 0));
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
        if (collision.gameObject.CompareTag("Player")) {
            if (collision.gameObject.TryGetComponent(out Player player)) {
                PlayerController playerController = collision.gameObject.GetComponent<PlayerController>();
                if (ShouldTakeDamage(playerController)) {
                    TakeDamage((int)playerController.damagePower);
                    damageVisualEffectImpactTime = damageVisualEffectTime;
                } else {
                    player.TakeDamage(damage);
                }
            }
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

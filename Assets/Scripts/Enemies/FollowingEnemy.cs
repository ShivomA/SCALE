
using UnityEngine;

public class FollowingEnemy : MonoBehaviour {
    public int damage = 10;
    public int strength = 0;
    public int maxHealth = 30;
    public float radius = 0.6f;
    public float detectionRange = 8f;
    public float normalMaxSpeed = 1.0f;
    public float normalMoveForce = 4.0f;
    public float followingMaxSpeed = 3.0f;
    public float followingMoveForce = 6.0f;
    public float verticalDetectionRange = 2.5f;

    public float leftBoundary;
    public float rightBoundary;
    public Color damageColor = Color.red;
    public float damageVisualEffectTime = 2.0f;

    private int currentHealth;
    private Color originalColor;
    private float damageVisualEffectImpactTime;
    private SpriteRenderer enemySpriteRenderer;

    private Rigidbody2D rb;

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

        float sizeX = transform.localScale.x;
        float boundSizeY = enemySpriteRenderer.sprite.bounds.size.y;
        radius = sizeX * boundSizeY / 2 + 0.1f;

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
        bool sawPlayer;

        if (Mathf.Abs(playerTransform.position.x - transform.position.x) < detectionRange &&
            Mathf.Abs(playerTransform.position.y - transform.position.y) < verticalDetectionRange) {
            sawPlayer = true;
        } else { sawPlayer = false; }

        if (sawPlayer) {
            rb.AddForce(followingMoveForce * Mathf.Sign(playerTransform.position.x - transform.position.x) * Vector2.right);
            rb.velocity = new Vector2(Mathf.Clamp(rb.velocity.x, -followingMaxSpeed, followingMaxSpeed), rb.velocity.y);
        } else {
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

            if (!outOfBoundary) {
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
            }
            rb.velocity = new Vector2(Mathf.Clamp(rb.velocity.x, -normalMaxSpeed, normalMaxSpeed), rb.velocity.y);
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

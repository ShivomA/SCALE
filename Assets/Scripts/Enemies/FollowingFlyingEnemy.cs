using UnityEngine;

public class FollowingFlyingEnemy : MonoBehaviour {
    public int damage = 10;
    public int strength = 4;
    public int maxHealth = 30;
    public float radius = 0.6f;
    public float detectionRange = 8f;
    public float normalMaxSpeedX = 2.0f;
    public float normalMaxSpeedY = 1.0f;
    public float normalMoveForceX = 1.0f;
    public float normalMoveForceY = 30.0f;
    public float followingMaxSpeedX = 4.0f;
    public float followingMaxSpeedY = 2.0f;
    public float followingMoveForceX = 2.0f;
    public float followingMoveForceY = 60.0f;

    public float topBoundary;
    public float leftBoundary;
    public float rightBoundary;
    public float bottomBoundary;
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

        if (leftBoundary == rightBoundary) {
            leftBoundary = transform.position.x - 15;
            rightBoundary = transform.position.x + 15;
        }

        if (topBoundary == bottomBoundary) {
            topBoundary = transform.position.y + 4;
            bottomBoundary = transform.position.y - 5;
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
            rb.AddForce(Vector2.right * normalMoveForceX);
            Flip();
            outOfBoundary = true;
        } else if (transform.position.x > rightBoundary) {
            rb.AddForce(Vector2.left * normalMoveForceX);
            Flip();
            outOfBoundary = true;
        }
        if (transform.position.y > topBoundary) {
            rb.AddForce(Vector2.down * normalMoveForceY);
            outOfBoundary = true;
        } else if (transform.position.y < bottomBoundary) {
            rb.AddForce(Vector2.up * normalMoveForceY);
            outOfBoundary = true;
        }

        if (!outOfBoundary) {
            bool sawPlayer;

            if ((playerTransform.position - transform.position).magnitude < detectionRange) {
                sawPlayer = true;
            } else { sawPlayer = false; }

            if (sawPlayer) {
                Vector2 forceDirection = (playerTransform.position - transform.position).normalized;
                rb.AddForce(new Vector2(forceDirection.x * followingMoveForceX, forceDirection.y * followingMoveForceY));
                rb.velocity = new Vector2(Mathf.Clamp(rb.velocity.x, -followingMaxSpeedX, followingMaxSpeedX),
                    Mathf.Clamp(rb.velocity.y, -followingMaxSpeedY, followingMaxSpeedY));
            } else {
                float yForce = (Mathf.PingPong(Time.time, 1.0f) - 0.5f) * normalMoveForceY;

                if (movingRight) {
                    rb.AddForce(new Vector2(normalMoveForceX, yForce));
                    if (transform.position.x >= rightBoundary) {
                        Flip();
                    }
                } else {
                    rb.AddForce(new Vector2(-normalMoveForceX, yForce));
                    if (transform.position.x <= leftBoundary) {
                        Flip();
                    }
                }

                rb.velocity = new Vector2(Mathf.Clamp(rb.velocity.x, -normalMaxSpeedX, normalMaxSpeedX),
                    Mathf.Clamp(rb.velocity.y, -normalMaxSpeedY, normalMaxSpeedY));
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


using UnityEngine;

public class FollowingEnemy : MonoBehaviour {
    public int damage = 8;
    public int strength = 0;
    public int maxHealth = 15;

    public float destroyedHealthPoints = 1.5f;
    public float destroyedHealthForceMagnitude = 5;

    public float detectionRange = 8f;
    public float normalMaxSpeed = 1.0f;
    public float normalMoveForce = 4.0f;
    public float followingMaxSpeed = 3.0f;
    public float followingMoveForce = 8.0f;
    public float retreatingMoveForce = 50.0f;
    public float verticalDetectionRange = 6.0f;

    public float leftBoundary;
    public float rightBoundary;
    public float dangerBoundaryLeft;
    public float dangerBoundaryRight;
    public Color damageColor = Color.red;
    public float damageVisualEffectTime = 2.0f;

    private float enemyWidth;
    private float enemyHeight;
    private int collissionRayCount = 5;

    private int currentHealth;
    private Color originalColor;
    private int numHitReceived = 0;
    private float damageVisualEffectImpactTime;
    private SpriteRenderer enemySpriteRenderer;

    private Rigidbody2D rb;

    private bool movingRight = true;

    public Player player;
    public Transform playerTransform;
    public GameObject collectableHealth;
    public LayerMask playerLayer;

    private void Start() {
        if (player == null)
            player = FindObjectOfType<Player>();

        if (playerTransform == null)
            playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

        float sizeX = transform.localScale.x;
        float sizeY = transform.localScale.y;
        float boundSizeX = GetComponent<SpriteRenderer>().sprite.bounds.size.x;
        float boundSizeY = GetComponent<SpriteRenderer>().sprite.bounds.size.y;
        enemyWidth = sizeX * boundSizeX;
        enemyHeight = sizeY * boundSizeY / 2 + 0.1f;

        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();

        enemySpriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = enemySpriteRenderer.color;

        if (leftBoundary == rightBoundary) {
            leftBoundary = transform.position.x - 10;
            rightBoundary = transform.position.x + 10;
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
            if (Mathf.Abs(playerTransform.position.x - transform.position.x) < 5 && playerTransform.position.y - transform.position.y > 1.5) {
                Vector2 forceToApply = retreatingMoveForce * Mathf.Sign(playerTransform.position.x - transform.position.x) * Vector2.left;
                if (dangerBoundaryLeft != dangerBoundaryRight) {
                    if (transform.position.x < dangerBoundaryLeft + 2 && forceToApply.x < 0) {
                        if (playerTransform.position.x < dangerBoundaryLeft + 2) {
                            forceToApply *= -1;
                        }
                    } else if (transform.position.x > dangerBoundaryRight - 2 && forceToApply.x > 0) {
                        if (playerTransform.position.x > dangerBoundaryRight - 2) {
                            forceToApply *= -1;
                        }
                    }
                }
                rb.AddForce(forceToApply);
            } else {
                Vector2 forceToApply = followingMoveForce * Mathf.Sign(playerTransform.position.x - transform.position.x) * Vector2.right;
                if (dangerBoundaryLeft != dangerBoundaryRight) {
                    if (transform.position.x < dangerBoundaryLeft && forceToApply.x < 0) {
                        forceToApply *= -1;
                    } else if (transform.position.x > dangerBoundaryRight && forceToApply.x > 0) {
                        forceToApply *= -1;
                    }
                }
                rb.AddForce(forceToApply);
            }

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

                if (ShouldTakeDamage(player)) {
                    numHitReceived += 1;
                    TakeDamage((int)player.damagePower);
                    damageVisualEffectImpactTime = damageVisualEffectTime;
                } else {
                    player.TakeDamage(damage);
                }
            }
        }
    }

    private bool ShouldTakeDamage(Player player) {
        float positionIncrement = enemyWidth / (collissionRayCount - 1);

        for (int i = 1; i < collissionRayCount - 1; i++) {
            float originPositionX = transform.position.x - enemyWidth / 2.0f + i * positionIncrement;
            Vector3 originPosition = new(originPositionX, transform.position.y, transform.position.z);

            if (Physics2D.Raycast(originPosition, Vector2.up, enemyHeight, playerLayer)) {
                if (player.damagePower >= strength) {
                    return true;
                }
            }
        }

        return false;
    }

    public void TakeDamage(int damage) {
        currentHealth -= damage;

        if (currentHealth <= 0) {
            Die();
        }
    }

    private void Die() {
        if (collectableHealth != null) {
            for (int i = 0; i < numHitReceived; i++) {
                Vector3 spawnPosition = transform.position;
                Quaternion spawnRotation = Quaternion.identity;
                Vector3 healthScale = new(destroyedHealthPoints / 20f, destroyedHealthPoints / 20f, destroyedHealthPoints / 20f);

                GameObject instantiatedCollectableHealth = Instantiate(collectableHealth, spawnPosition, spawnRotation);
                instantiatedCollectableHealth.transform.localScale = healthScale;
                Rigidbody2D healthRb = instantiatedCollectableHealth.GetComponent<Rigidbody2D>();

                Vector2 randomDirection = Random.onUnitSphere;
                randomDirection.x *= destroyedHealthPoints;
                healthRb.AddForce(randomDirection * destroyedHealthForceMagnitude, ForceMode2D.Impulse);

                CollectableHealth collectableHealthScript = instantiatedCollectableHealth.GetComponent<CollectableHealth>();
                collectableHealthScript.healthPoints = destroyedHealthPoints;
            }
        }

        Destroy(gameObject);
    }
}

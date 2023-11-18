using UnityEngine;

public class FollowingFlyingEnemy : MonoBehaviour {
    public int damage = 10;
    public int strength = 0;
    public int maxHealth = 30;
    public int destroyedHealthPoints = 4;
    public int destroyedHealthForceMagnitude = 5;

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
            }

            rb.velocity = new Vector2(Mathf.Clamp(rb.velocity.x, -normalMaxSpeedX, normalMaxSpeedX),
                Mathf.Clamp(rb.velocity.y, -normalMaxSpeedY, normalMaxSpeedY));
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

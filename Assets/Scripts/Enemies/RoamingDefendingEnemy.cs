using UnityEngine;

public class RoamingDefendingEnemy : MonoBehaviour {
    public int damage = 6;
    public int strength = 0;
    public int maxHealth = 12;

    public float maxDefendingStrength = 50;

    public float destroyedHealthPoints = 2f;
    public float destroyedHealthForceMagnitude = 5;

    public float maxSpeed = 1.0f;
    public float moveForce = 5.0f;
    public float detectionRange = 6f;
    public float defendCooldown = 1.2f;
    public float verticalDetectionRange = 5f;

    public float leftBoundary;
    public float rightBoundary;
    public Color damageColor = Color.red;
    public float damageVisualEffectTime = 2.0f;
    public Color defendingColor = new(128, 0, 255);

    private float enemyWidth;
    private float enemyHeight;
    private int collissionRayCount = 5;

    private float steadyTime;
    private int currentHealth;
    private Color originalColor;
    private int numHitReceived = 0;
    private bool isDefending = false;
    private float defendingStrength = 25;
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

        if (defendingStrength > 0 && Mathf.Abs(playerTransform.position.x - transform.position.x) < detectionRange &&
            Mathf.Abs(playerTransform.position.y - transform.position.y) < verticalDetectionRange) {
            sawPlayer = true;
        } else { sawPlayer = false; }

        if (sawPlayer) {
            if (steadyTime <= defendCooldown) {
                steadyTime += Time.deltaTime;
            }
            rb.velocity = Vector2.zero;

            if (steadyTime > defendCooldown && defendingStrength > 0) {
                isDefending = true;
                enemySpriteRenderer.color = Color.Lerp(originalColor, defendingColor, defendingStrength / maxDefendingStrength);
            }
        } else {
            if (steadyTime > 0) { steadyTime -= Time.deltaTime; }
            isDefending = false;
            enemySpriteRenderer.color = Color.Lerp(originalColor, defendingColor, steadyTime / defendCooldown);

            if (movingRight) {
                rb.AddForce(Vector2.right * moveForce);
                if (transform.position.x >= rightBoundary) {
                    Flip();
                }
            } else {
                rb.AddForce(Vector2.left * moveForce);
                if (transform.position.x <= leftBoundary) {
                    Flip();
                }
            }

            rb.velocity = new Vector2(Mathf.Clamp(rb.velocity.x, -maxSpeed, maxSpeed), rb.velocity.y);
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
        if (isDefending && defendingStrength > 0) {
            defendingStrength -= player.damagePower;
            enemySpriteRenderer.color = Color.Lerp(originalColor, defendingColor, defendingStrength / maxDefendingStrength);

            if (defendingStrength <= 0)
                isDefending = false;

            return false;
        } else {
            float positionIncrement = enemyWidth / (collissionRayCount - 1);

            for (int i = 0; i < collissionRayCount; i++) {
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

                GameObject instantiatedCollectableHealth = Instantiate(collectableHealth, spawnPosition, spawnRotation);
                Rigidbody2D healthRb = instantiatedCollectableHealth.GetComponent<Rigidbody2D>();

                CollectableHealth collectableHealthScript = instantiatedCollectableHealth.GetComponent<CollectableHealth>();
                collectableHealthScript.UpdateHealthPoint(destroyedHealthPoints);

                Vector2 randomDirection = Random.onUnitSphere;
                randomDirection.x *= destroyedHealthPoints;
                healthRb.AddForce(randomDirection * destroyedHealthForceMagnitude, ForceMode2D.Impulse);
            }
        }

        Destroy(gameObject);
    }
}

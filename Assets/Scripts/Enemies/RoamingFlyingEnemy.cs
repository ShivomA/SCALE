using UnityEngine;

public class RoamingFlyingEnemy : MonoBehaviour {
    public int damage = 4;
    public int strength = 0;
    public int maxHealth = 16;

    public float destroyedHealthPoints = 2;
    public float destroyedHealthForceMagnitude = 5;

    public float maxSpeed = 1.0f;
    public float moveForce = 2.0f;
    public float verticalMoveForce = 30.0f;

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
    public GameObject collectableHealth;
    public LayerMask playerLayer;

    private void Start() {
        if (player == null)
            player = FindObjectOfType<Player>();

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

        if (topBoundary == bottomBoundary) {
            topBoundary = transform.position.y + 4;
            bottomBoundary = transform.position.y - 4;
        }
    }

    private void Update() {
        DamageVisual();
    }

    private void FixedUpdate() {
        MovementLogic();
    }

    private void MovementLogic() {
        float yForce = (Mathf.PingPong(Time.time, 1.0f) - 0.5f) * verticalMoveForce;

        if (movingRight) {
            rb.AddForce(new Vector2(moveForce, yForce));
            if (transform.position.x >= rightBoundary) {
                Flip();
            }
        } else {
            rb.AddForce(new Vector2(-moveForce, yForce));
            if (transform.position.x <= leftBoundary) {
                Flip();
            }
        }

        if (transform.position.y > topBoundary)
            rb.AddForce(Vector2.down * verticalMoveForce);
        else if (transform.position.y < bottomBoundary)
            rb.AddForce(Vector2.up * verticalMoveForce);

        rb.velocity = new Vector2(rb.velocity.x,rb.velocity.y).normalized * maxSpeed;
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

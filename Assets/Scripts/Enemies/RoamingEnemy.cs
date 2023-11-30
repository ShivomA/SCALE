using UnityEngine;

public class RoamingEnemy : MonoBehaviour {
    public int damage = 6;
    public int strength = 0;
    public int maxHealth = 12;

    public float destroyedHealthPoints = 1;
    public float destroyedHealthForceMagnitude = 5;

    public float maxSpeed = 1.0f;
    public float moveForce = 5.0f;
    public float retreatingMaxSpeed = 6.0f;
    public float retreatingMoveForce = 12.0f;

    public float leftBoundary;
    public float rightBoundary;
    public Color damageColor = Color.red;
    public float damageVisualEffectTime = 2.0f;

    private bool isAttacked;
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

    public SoundManager soundManager;

    private void Start() {
        if (player == null)
            player = FindObjectOfType<Player>();

        if (soundManager == null)
            soundManager = FindObjectOfType<SoundManager>();

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

        if (!isAttacked)
            rb.velocity = new Vector2(Mathf.Clamp(rb.velocity.x, -maxSpeed, maxSpeed), rb.velocity.y);
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
                    if (damageVisualEffectImpactTime <= 0) {
                        numHitReceived += 1;
                        TakeDamage((int)player.damagePower);
                        damageVisualEffectImpactTime = damageVisualEffectTime;
                    }

                    isAttacked = true;
                    Invoke(nameof(ResetIsAttacked), 1f);

                    Vector2 retreatingForce;
                    float retreatingForceMagnitude = playerTransform.position.x - transform.position.x;
                    float retreatingForceDirection = -Mathf.Sign(playerTransform.position.x - transform.position.x);

                    retreatingForce.x = retreatingForceDirection * (8 * retreatingMoveForce - retreatingMoveForce * retreatingForceMagnitude);

                    rb.AddForce(new Vector2(retreatingForce.x * moveForce, 0), ForceMode2D.Impulse);
                    rb.velocity = new Vector2(Mathf.Clamp(rb.velocity.x, -retreatingMaxSpeed, retreatingMaxSpeed), 0);
                } else {
                    player.TakeDamage(damage);
                }
            }
        }
    }

    private void ResetIsAttacked() {
        isAttacked = false;
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
        } else {
            soundManager.PlayEnemyDamageSound();
        }
    }

    private void Die() {
        soundManager.PlayEnemyDeathSound();

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

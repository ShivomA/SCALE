using UnityEngine;

public class TargetingFlyingEnemy : MonoBehaviour {
    public int damage = 15;
    public int strength = 0;
    public int maxHealth = 16;

    public float destroyedHealthPoints = 3.5f;
    public float destroyedHealthForceMagnitude = 5;

    public float hitCooldown = 1.0f;
    public float detectionRange = 15f;
    public float attackCooldown = 1.2f;
    public float attackDuration = 2.0f;
    public float normalMaxSpeedX = 2.0f;
    public float normalMaxSpeedY = 1.0f;
    public float normalMoveForceX = 10.0f;
    public float normalMoveForceY = 30.0f;
    public float decelerationForce = 10.0f;
    public float playerHeightOffset = 3.0f;
    public float followingMaxSpeed = 20.0f;
    public float takePositionForce = 150.0f;
    public float followingMoveForceX = 30.0f;
    public float followingMoveForceY = 30.0f;

    public float topBoundary;
    public float leftBoundary;
    public float rightBoundary;
    public float bottomBoundary;
    public Color damageColor = Color.red;
    public float damageVisualEffectTime = 2.0f;
    public Color attackingColor = new(128, 0, 255);

    private float enemyWidth;
    private float enemyHeight;
    private int collissionRayCount = 5;

    private float steadyTime;
    private int currentHealth;
    private Color originalColor;
    public bool isSteady = true;
    private int numHitReceived = 0;
    private float damageVisualEffectImpactTime;
    private SpriteRenderer enemySpriteRenderer;

    private Rigidbody2D rb;

    private bool movingRight = true;

    public Player player;
    public Transform playerTransform;
    public GameObject collectableHealth;
    public LayerMask collissionLayer;
    public LayerMask playerLayer;

    public SoundManager soundManager;
    private bool isFocusSoundPlaying = false;

    private void Start() {
        if (player == null)
            player = FindObjectOfType<Player>();

        if (soundManager == null)
            soundManager = FindObjectOfType<SoundManager>();

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
        if (steadyTime <= attackCooldown && isSteady) {
            steadyTime += Time.deltaTime;
        }

        bool sawPlayer;

        if ((playerTransform.position - transform.position).magnitude < detectionRange) {
            sawPlayer = true;
        } else { sawPlayer = false; }

        if (sawPlayer) {
            if (transform.position.y > playerTransform.position.y + playerHeightOffset) {
                if (rb.velocity.y > 0) {
                    if (isFocusSoundPlaying) {
                        isFocusSoundPlaying = false;
                        soundManager.StopTargetingEnemyFocusSound();
                    }

                    rb.AddForce(-rb.velocity.normalized * decelerationForce);
                }
            }

            if (isSteady) {
                if (transform.position.y < playerTransform.position.y + playerHeightOffset) {
                    if (isFocusSoundPlaying) {
                        isFocusSoundPlaying = false;
                        soundManager.StopTargetingEnemyFocusSound();
                    }

                    rb.AddForce(takePositionForce * Vector2.up);

                    steadyTime = 0;
                    isSteady = false;
                    CancelInvoke(nameof(MakeSteady));
                    Invoke(nameof(MakeSteady), hitCooldown);
                    enemySpriteRenderer.color = Color.Lerp(originalColor, attackingColor, 0);
                } else {
                    rb.velocity = Vector2.zero;

                    if (steadyTime < attackCooldown) {
                        if (!isFocusSoundPlaying) {
                            isFocusSoundPlaying = true;
                            soundManager.StopTargetingEnemyFocusSound();
                            soundManager.PlayTargetingEnemyFocusSound();
                        }
                        enemySpriteRenderer.color = Color.Lerp(originalColor, attackingColor, steadyTime / attackCooldown);
                    } else {
                        if (isFocusSoundPlaying) {
                            isFocusSoundPlaying = false;
                            soundManager.StopTargetingEnemyFocusSound();
                        }

                        Vector2 forceDirection = (playerTransform.position + Vector3.up - transform.position).normalized;
                        rb.AddForce(new Vector2(forceDirection.x * followingMoveForceX, forceDirection.y * followingMoveForceY), ForceMode2D.Impulse);

                        rb.velocity = rb.velocity.normalized * followingMaxSpeed;

                        steadyTime = 0;
                        isSteady = false;
                        CancelInvoke(nameof(MakeSteady));
                        Invoke(nameof(MakeSteady), attackDuration);
                        enemySpriteRenderer.color = Color.Lerp(originalColor, attackingColor, 0);
                    }
                }
            }
        } else {
            if (isFocusSoundPlaying) {
                isFocusSoundPlaying = false;
                soundManager.StopTargetingEnemyFocusSound();
            }

            steadyTime = 0;
            bool outOfBoundary = false;

            if (transform.position.x < leftBoundary) {
                rb.AddForce(Vector2.right * normalMoveForceX);
                outOfBoundary = true;
            } else if (transform.position.x > rightBoundary) {
                rb.AddForce(Vector2.left * normalMoveForceX);
                outOfBoundary = true;
            }
            if (transform.position.y > topBoundary) {
                rb.AddForce(Vector2.down * normalMoveForceY);
                Flip();
                outOfBoundary = true;
            } else if (transform.position.y < bottomBoundary) {
                rb.AddForce(Vector2.up * normalMoveForceY);
                Flip();
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

    private void MakeSteady() {
        isSteady = true;
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
        if (((1 << collision.gameObject.layer) & collissionLayer) != 0) {
            CancelInvoke(nameof(MakeSteady));
            Invoke(nameof(MakeSteady), hitCooldown);
        }

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
                } else {
                    player.TakeDamage(damage);
                }
            }
        }
    }

    private bool ShouldTakeDamage(Player player) {
        float positionIncrement = enemyWidth / (collissionRayCount - 1);

        for (float i = 0; i < collissionRayCount; i++) {
            if (i == 0)
                i = 0.5f;
            if (i == collissionRayCount - 1)
                i = collissionRayCount - 1.5f;

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

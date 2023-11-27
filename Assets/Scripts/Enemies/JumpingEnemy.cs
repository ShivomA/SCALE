
using UnityEngine;

public class JumpingEnemy : MonoBehaviour {
    public int damage = 12;
    public int strength = 0;
    public int maxHealth = 20;

    public float destroyedHealthPoints = 2.5f;
    public float destroyedHealthForceMagnitude = 5;

    public int numAirJump = 2;
    public float radius = 1.2f;
    public float jumpForce = 10.0f;
    public float jumpCooldown = 0.5f;
    public float normalMaxSpeed = 1.5f;
    public float detectionRange = 14.0f;
    public float airJumpCooldown = 1.2f;
    public float normalMoveForce = 10.0f;
    public float followingMaxSpeed = 4.0f;
    public float followingMoveForce = 0.5f;
    public float verticalDetectionRange = 10.0f;

    public float leftBoundary;
    public float rightBoundary;
    public float dangerBoundaryLeft;
    public float dangerBoundaryRight;
    public Color damageColor = Color.red;
    public float damageVisualEffectTime = 2.0f;

    public float groundCheckAngle = 30.0f;
    public float groundCheckRayCount = 7f;

    private float enemyWidth;
    private float enemyHeight;
    private int collissionRayCount = 5;

    private int numJumps;
    private float steadyTime;
    private float airJumpTime;
    private int currentHealth;
    private Color originalColor;
    private bool isGrounded = true;
    private int numHitReceived = 0;
    private float damageVisualEffectImpactTime;
    private SpriteRenderer enemySpriteRenderer;

    private Rigidbody2D rb;

    private bool movingRight = true;

    public Player player;
    public Transform playerTransform;
    public GameObject collectableHealth;
    public LayerMask playerLayer;
    public LayerMask groundLayer;

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
        radius = sizeX * boundSizeY / 2 + 0.1f;

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

    private void Jump() {
        bool shouldJump = false;
        if (numJumps == 0) {
            shouldJump = true;
        } else {
            if (playerTransform.position.y >= transform.position.y) {
                shouldJump = true;
            }
        }

        if (shouldJump) {
            float xForce = followingMoveForce * (playerTransform.position.x - transform.position.x);
            rb.AddForce(new Vector2(xForce, jumpForce), ForceMode2D.Impulse);

            numJumps += 1;
            steadyTime = 0;
            isGrounded = false;
            airJumpTime = airJumpCooldown * (numJumps - 1);
        }
        rb.velocity = new Vector2(Mathf.Clamp(rb.velocity.x, -followingMaxSpeed, followingMaxSpeed), rb.velocity.y);
    }

    private void MovementLogic() {
        if (steadyTime <= jumpCooldown && isGrounded) {
            steadyTime += Time.deltaTime;
        }
        if (airJumpTime <= airJumpCooldown * numAirJump) {
            airJumpTime += Time.deltaTime;
        }

        bool sawPlayer;

        if (Mathf.Abs(playerTransform.position.x - transform.position.x) < detectionRange &&
            Mathf.Abs(playerTransform.position.y - transform.position.y) < verticalDetectionRange) {
            sawPlayer = true;
        } else { sawPlayer = false; }

        if (sawPlayer) {
            if (dangerBoundaryLeft == dangerBoundaryRight) {
                if (isGrounded) {
                    if (steadyTime >= jumpCooldown) {
                        Jump();
                    }
                } else if (0 < numJumps && numJumps < numAirJump && airJumpTime >= airJumpCooldown * numJumps) {
                    Jump();
                }
            } else {
                if (playerTransform.position.x > dangerBoundaryLeft &&
                 playerTransform.position.x < dangerBoundaryRight) {
                    if (isGrounded) {
                        if (steadyTime >= jumpCooldown) {
                            Jump();
                        }
                    } else if (0 < numJumps && numJumps < numAirJump && airJumpTime >= airJumpCooldown * numJumps) {
                        Jump();
                    }
                }
            }

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
        float angleIncrement = groundCheckAngle / (groundCheckRayCount - 1);
        int i = 0;

        for (i = 0; i < groundCheckRayCount; i++) {
            float angle = -groundCheckAngle / 2.0f + i * angleIncrement;
            Vector2 direction = Quaternion.Euler(0, 0, angle) * Vector2.down;

            if (Physics2D.Raycast(transform.position, direction, radius, groundLayer)) {
                isGrounded = true;
                numJumps = 0;
                break;
            }
        }

        if (i == groundCheckRayCount) {
            isGrounded = false;
        } else {
            steadyTime = 0;
            airJumpTime = 0;
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

    private void OnCollisionExit2D(Collision2D collision) {
        float angleIncrement = groundCheckAngle / (groundCheckRayCount - 1);
        int i = 0;

        for (i = 0; i < groundCheckRayCount; i++) {
            float angle = -groundCheckAngle / 2.0f + i * angleIncrement;
            Vector2 direction = Quaternion.Euler(0, 0, angle) * Vector2.down;

            if (Physics2D.Raycast(transform.position, direction, radius, groundLayer)) {
                isGrounded = true;
                numJumps = 0;
                break;
            }
        }

        if (i == groundCheckRayCount) {
            isGrounded = false;
        } else {
            steadyTime = 0;
            airJumpTime = 0;
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

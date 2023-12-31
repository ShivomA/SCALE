using TMPro;
using UnityEngine;

public class Player : MonoBehaviour {
    public Rigidbody2D rb;
    public LevelController levelController;

    [Header("Min - Max Variables")]
    public float maxHealth = 100;
    public float damagePower = 5f;
    public float minMaxHealth = 40;
    public float maxMaxHealth = 100;
    public float minDamagePower = 4.0f;
    public float maxDamagePower = 8.0f;
    public float damageCooldownTime = 1.0f;
    public float damageCooldownEffectTime = 0.2f;

    public float damageTakenForceX = 5;
    public float damageTakenForceY = 5;

    public Color immuneColor = Color.red;

    public TextMeshProUGUI healthText;

    [Header("Health Conversion Variables")]
    private int numHitReceived;
    public float destroyedHealthPoints = 3;
    public float destroyedHealthForceMagnitude = 5;
    public GameObject collectableHealth;

    private Color originalColor;
    private SpriteRenderer playerSpriteRenderer;

    private float health;
    private bool canTakeDamage;
    private float damageCooldown;
    private float currentMinHealth;
    private float currentMaxHealth;

    public SoundManager soundManager;

    private void Start() {
        if (rb == null) {
            rb = GetComponent<Rigidbody2D>();
        }

        if (soundManager == null) {
            soundManager = FindObjectOfType<SoundManager>();
        }

        if (levelController == null) {
            levelController = FindObjectOfType<LevelController>();
        }

        health = maxHealth;
        canTakeDamage = true;
        damageCooldown = damageCooldownTime;

        currentMinHealth = health * minMaxHealth / maxHealth;
        currentMaxHealth = health * maxMaxHealth / maxHealth;
        healthText.text = (int)health + "/" + (int)maxHealth;

        playerSpriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = playerSpriteRenderer.color;
    }

    private void Update() {
        if (!canTakeDamage) {
            damageCooldown -= Time.deltaTime;
            if (damageCooldown <= 0) {
                canTakeDamage = true;
                damageCooldown = damageCooldownTime;
            }
        }

        if (!canTakeDamage) {
            if (damageCooldown > damageCooldownEffectTime) {
                float lerpValue = Mathf.PingPong(Time.time * 5.0f, 1.0f);
                playerSpriteRenderer.color = Color.Lerp(originalColor, immuneColor, lerpValue);
            } else {
                playerSpriteRenderer.color = originalColor;
            }
        } else {
            playerSpriteRenderer.color = originalColor;
        }
    }

    public void UpdateHealth(float sizeScale, float sizeScaleMin, float sizeScaleMax) {
        maxHealth = minMaxHealth + (sizeScale - sizeScaleMin) * (maxMaxHealth - minMaxHealth) / (sizeScaleMax - sizeScaleMin);
        health = currentMinHealth + (sizeScale - sizeScaleMin) * (currentMaxHealth - currentMinHealth) / (sizeScaleMax - sizeScaleMin);

        health = Mathf.Min(health, maxHealth);

        healthText.text = (int)health + "/" + (int)maxHealth;
    }

    public void UpdateDamagePower(float sizeScale, float sizeScaleMin, float sizeScaleMax) {
        damagePower = minDamagePower + (sizeScale - sizeScaleMin) * (maxDamagePower - minDamagePower) / (sizeScaleMax - sizeScaleMin);
    }

    public void TakeDamage(int damage) {
        if (canTakeDamage) {
            soundManager.PlayPlayerDamageSound();

            numHitReceived += 1;

            health -= damage;
            canTakeDamage = false;

            currentMinHealth = health * minMaxHealth / maxHealth;
            currentMaxHealth = health * maxMaxHealth / maxHealth;

            healthText.text = (int)health + "/" + (int)maxHealth;

            if (health <= 0)
                Die();
        }
    }

    public void GainHealth(float healthPoints) {
        health += healthPoints;
        health = Mathf.Min(health, maxHealth);

        currentMinHealth = health * minMaxHealth / maxHealth;
        currentMaxHealth = health * maxMaxHealth / maxHealth;

        healthText.text = (int)health + "/" + (int)maxHealth;
    }

    public void DieByFalling() {
        ResetPlayerStats(resetHealth: false);
        soundManager.PlayPlayerDeathByFallingSound();

        levelController.PlayerDied();
    }

    private void Die() {
        soundManager.PlayPlayerDeathSound();

        int lastNumHitReceived = numHitReceived;
        Vector3 deathPosition = transform.position;

        ResetPlayerStats();
        levelController.PlayerDied();

        if (collectableHealth != null) {
            for (int i = 0; i < lastNumHitReceived; i++) {
                Vector3 spawnPosition = deathPosition;
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
    }

    private void ResetPlayerStats(bool resetHealth = true) {
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0;

        if (resetHealth) {
            health = maxHealth;
            numHitReceived = 0;
            canTakeDamage = true;
            damageCooldown = damageCooldownTime;

            currentMinHealth = health * minMaxHealth / maxHealth;
            currentMaxHealth = health * maxMaxHealth / maxHealth;
            healthText.text = (int)health + "/" + (int)maxHealth;
        }
    }
}

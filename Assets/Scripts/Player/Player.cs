using TMPro;
using UnityEngine;

public class Player : MonoBehaviour {
    public float maxHealth = 100;
    public float damagePower = 5f;
    public float minMaxHealth = 40;
    public float maxMaxHealth = 100;
    public float minDamagePower = 4.0f;
    public float maxDamagePower = 8.0f;
    public float damageCooldownTime = 2.0f;
    public float damageCooldownEffectTime = 0.2f;

    public float damageTakenForceX = 5;
    public float damageTakenForceY = 5;

    public Color immuneColor = Color.red;

    public TextMeshProUGUI healthText;

    private Color originalColor;
    private SpriteRenderer playerSpriteRenderer;

    [SerializeField] private float health;
    private bool canTakeDamage;
    private float damageCooldown;
    private float currentMinHealth;
    private float currentMaxHealth;

    private void Start() {
        health = maxHealth;
        canTakeDamage = true;
        damageCooldown = damageCooldownTime;

        currentMinHealth = health * minMaxHealth / maxHealth;
        currentMaxHealth = health * maxMaxHealth / maxHealth;

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

    public void GainHealth(int healthPoints) {
        health += healthPoints;
        health = Mathf.Min(health, maxHealth);

        currentMinHealth = health * minMaxHealth / maxHealth;
        currentMaxHealth = health * maxMaxHealth / maxHealth;

        healthText.text = (int)health + "/" + (int)maxHealth;
    }

    public void TakeDamage(int damage) {
        if (canTakeDamage) {
            health -= damage;
            canTakeDamage = false;

            currentMinHealth = health * minMaxHealth / maxHealth;
            currentMaxHealth = health * maxMaxHealth / maxHealth;

            healthText.text = (int)health + "/" + (int)maxHealth;

            if (health <= 0)
                Die();
        } else {
            Debug.Log("Player is Immune");
        }
    }

    private void Die() {
        Debug.Log("Player is Dead");
    }
}

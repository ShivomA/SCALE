using UnityEngine;

public class Player : MonoBehaviour {
    public int maxHealth = 100;
    public float damageCooldownTime = 2.0f;
    public float damageCooldownEffectTime = 0.2f;

    public float damageTakenForceX = 5;
    public float damageTakenForceY = 5;

    public Color immuneColor = Color.red;

    private Color originalColor;
    private SpriteRenderer playerSpriteRenderer;

    private int health;
    private bool canTakeDamage;
    private float damageCooldown;

    private void Start() {
        health = maxHealth;
        canTakeDamage = true;
        damageCooldown = damageCooldownTime;

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

    public void TakeDamage(int damage) {
        if (canTakeDamage) {
            health -= damage;
            canTakeDamage = false;

            Debug.Log("Damage taken: " + damage);
            Debug.Log("Player health is: " + health);

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

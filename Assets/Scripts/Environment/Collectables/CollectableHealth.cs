using UnityEngine;

public class CollectableHealth : MonoBehaviour {
    public float healthPoints = 2;
    public float destroyTime = 120;

    void Start() {
        Destroy(gameObject, destroyTime);
    }

    public void UpdateHealthPoint(float newHealthPoints) {
        healthPoints = newHealthPoints;
        transform.localScale = new(healthPoints / 5, healthPoints / 5, healthPoints / 5);
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        if (collision.collider.gameObject.CompareTag("Player")) {
            collision.collider.GetComponent<Player>().GainHealth(healthPoints);
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.CompareTag("Player")) {
            collision.GetComponent<Player>().GainHealth(healthPoints);
            Destroy(gameObject);
        }
    }
}

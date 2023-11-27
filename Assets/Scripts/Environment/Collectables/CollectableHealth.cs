using UnityEngine;

public class CollectableHealth : MonoBehaviour {
    public float healthPoints = 2;
    public float destroyTime = 120;

    void Start() {
        UpdateHealthPoint(healthPoints);
        Destroy(gameObject, destroyTime);
    }

    public void UpdateHealthPoint(float newHealthPoints) {
        healthPoints = newHealthPoints;
        float newScale = Mathf.Clamp(healthPoints / 10, 0.175f, 0.4f);
        transform.localScale = new(newScale, newScale, newScale);
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

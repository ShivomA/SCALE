using UnityEngine;

public class CheckPoint : MonoBehaviour {
    public LevelController levelController;

    void Start() {
        if (levelController == null) {
            levelController = FindObjectOfType<LevelController>();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.CompareTag("Player")) {
            levelController.startPosition = transform.position;
        }
    }
}

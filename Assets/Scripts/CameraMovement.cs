using UnityEngine;

public class CameraMovement : MonoBehaviour {
    public Transform playerTransform;
    public Rigidbody2D playerRb;

    public float offsetX;

    private void Start() {
        if (playerTransform == null) { playerTransform = FindObjectOfType<Player>().transform; }
        if (playerRb == null) { playerRb = FindObjectOfType<Player>().GetComponent<Rigidbody2D>(); }

        Vector3 desiredPosition = new(playerTransform.position.x + offsetX, transform.position.y, transform.position.z);
        desiredPosition.x = Mathf.Max(0, desiredPosition.x);

        transform.position = desiredPosition;
    }

    void LateUpdate() {
        bool movingRight = playerRb.velocity.x >= 0;

        if (movingRight) {
            int multiplyingFactor = 1;
            if (playerTransform.position.x > transform.position.x - offsetX) { multiplyingFactor = 2; }

            float xPos = Mathf.Max(transform.position.x + multiplyingFactor * playerRb.velocity.x * Time.deltaTime);
            if (xPos > playerTransform.position.x + offsetX) { xPos = playerTransform.position.x + offsetX; }
            Vector3 desiredPosition = new(xPos, transform.position.y, transform.position.z);

            transform.position = desiredPosition;
        } else {
            int multiplyingFactor = 1;
            if (playerTransform.position.x < transform.position.x + offsetX) { multiplyingFactor = 2; }

            float xPos = Mathf.Max(transform.position.x + multiplyingFactor * playerRb.velocity.x * Time.deltaTime);
            if (xPos < playerTransform.position.x - offsetX) { xPos = playerTransform.position.x - offsetX; }
            Vector3 desiredPosition = new(xPos, transform.position.y, transform.position.z);

            transform.position = desiredPosition;
        }

    }
}

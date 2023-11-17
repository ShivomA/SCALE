using UnityEngine;

public class MovingPlatforms : MonoBehaviour {
    public float topBoundary;
    public float bottomBoundary;

    public float maxSpeed = 2f;
    public float moveForceUp = 30.0f;
    public float moveForceDown = 5.0f;
    public float platformWidth = 6.5f;
    public float platformHeight = 1.1f;

    public int collissionRayCount = 7;

    public LayerMask collissionLayerTop;
    public LayerMask collissionLayerBottom;

    private Rigidbody2D rb;
    public bool movingUp = true;

    void Start() {
        rb = GetComponent<Rigidbody2D>();

        float sizeX = transform.localScale.x;
        float sizeY = transform.localScale.y;
        float boundSizeX = GetComponent<SpriteRenderer>().sprite.bounds.size.x;
        float boundSizeY = GetComponent<SpriteRenderer>().sprite.bounds.size.y;
        platformWidth = sizeX * boundSizeX;
        platformHeight = sizeY * boundSizeY / 2 + 0.1f;

        if (topBoundary == bottomBoundary) {
            topBoundary = transform.position.y + 4;
            bottomBoundary = transform.position.y - 2;
        }
    }

    private void FixedUpdate() {
        MovementLogic();
    }

    private void MovementLogic() {
        if (movingUp) {
            rb.AddForce(Vector2.up * moveForceUp);
            if (transform.position.y >= topBoundary) {
                Flip();
            }
        } else {
            rb.AddForce(Vector2.down * moveForceDown);
            if (transform.position.x <= bottomBoundary) {
                Flip();
            }
        }

        rb.velocity = new Vector2(0, Mathf.Clamp(rb.velocity.y, -maxSpeed, maxSpeed));
    }

    private void Flip() {
        movingUp = !movingUp;
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        float positionIncrement = platformWidth / (collissionRayCount - 1);

        if (movingUp) {
            if (collision.collider.transform.position.y < transform.position.y)
                return;

            for (int i = 0; i < collissionRayCount; i++) {
                float originPositionX = transform.position.x - platformWidth / 2.0f + i * positionIncrement;
                Vector3 originPosition = new(originPositionX, transform.position.y, transform.position.z);

                if (Physics2D.Raycast(originPosition, Vector2.up, platformHeight, collissionLayerTop)) {
                    Flip();
                    break;
                }
            }
        } else {
            if (collision.collider.transform.position.y > transform.position.y)
                return;

            for (int i = 0; i < collissionRayCount; i++) {
                float originPositionX = transform.position.x - platformWidth / 2.0f + i * positionIncrement;
                Vector3 originPosition = new(originPositionX, transform.position.y, transform.position.z);

                if (Physics2D.Raycast(originPosition, Vector2.down, platformHeight, collissionLayerBottom)) {
                    Flip();
                    break;
                }
            }
        }
    }
}

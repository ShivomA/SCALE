using UnityEngine;

public class UnstableHorizontalMovingPlatform : MonoBehaviour {
    public float leftBoundary;
    public float rightBoundary;

    public float movingSpeed = 2f;
    public float maxBearableMass = 1f;
    public float platformWidth = 6.5f;
    public float platformHeight = 1.1f;
    public float decelerationRate = 5f;

    public LayerMask collissionLayer;

    private Rigidbody2D rb;
    public bool movingRight = true;

    void Start() {
        rb = GetComponent<Rigidbody2D>();

        float sizeX = transform.localScale.x;
        float sizeY = transform.localScale.y;
        float boundSizeX = GetComponent<SpriteRenderer>().sprite.bounds.size.x;
        float boundSizeY = GetComponent<SpriteRenderer>().sprite.bounds.size.y;
        platformWidth = sizeX * boundSizeX / 2 + 0.1f;
        platformHeight = sizeY * boundSizeY / 2 + 0.1f;

        if (leftBoundary == rightBoundary) {
            leftBoundary = transform.position.x - 10;
            rightBoundary = transform.position.x + 10;
        }
    }

    private void FixedUpdate() {
        MovementLogic();
    }

    private void MovementLogic() {
        if (movingRight) {
            rb.velocity = new Vector2(movingSpeed, Mathf.Min(rb.velocity.y, 0));
            if (transform.position.x >= rightBoundary) {
                Flip();
            }
        } else {
            rb.velocity = new Vector2(-movingSpeed, Mathf.Min(rb.velocity.y, 0));
            if (transform.position.x <= leftBoundary) {
                Flip();
            }
        }

        if (rb.velocity.y < 0) {
            float decelerationForceY = -rb.velocity.y * decelerationRate + maxBearableMass * 10;
            rb.AddForce(Vector2.up * decelerationForceY);
        }
    }

    private void Flip() {
        movingRight = !movingRight;
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        if (movingRight) {
            if (collision.collider.transform.position.x < transform.position.x)
                return;

            if (Physics2D.Raycast(transform.position, Vector2.right, platformWidth, collissionLayer)) 
                Flip();
        } else {
            if (collision.collider.transform.position.x > transform.position.x)
                return;

            if (Physics2D.Raycast(transform.position, Vector2.left, platformWidth, collissionLayer))
                Flip();
        }
    }
}

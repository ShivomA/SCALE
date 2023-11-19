using UnityEngine;

public class UnstableHorizontalMovingPlatform : MonoBehaviour {
    public float leftBoundary;
    public float rightBoundary;

    public float movingSpeed = 2f;
    public float platformWidth = 6.5f;
    public float platformHeight = 1.1f;
    public float decelerationRate = 5;
    public float decelerationForceX = 5;

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
            leftBoundary = transform.position.y - 4;
            rightBoundary = transform.position.y + 4;
        }
    }

    private void FixedUpdate() {
        MovementLogic();
    }

    private void MovementLogic() {
        if (movingRight) {
            rb.velocity = new Vector2(movingSpeed, rb.velocity.y);
            if (transform.position.y >= rightBoundary) {
                Flip();
            }
        } else {
            rb.velocity = new Vector2(-movingSpeed, rb.velocity.y);
            if (transform.position.x <= leftBoundary) {
                Flip();
            }
        }

        if (Mathf.Abs(rb.velocity.y) > 0) {
            float decelerationForceY = -rb.velocity.y * decelerationRate;
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

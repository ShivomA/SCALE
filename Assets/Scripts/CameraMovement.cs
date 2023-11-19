using UnityEngine;

public class CameraMovement : MonoBehaviour {
    [Header("Player Movement Variables")]
    public Transform playerTransform;
    public Rigidbody2D playerRb;

    public float playerOffsetX = 6;
    public float playerOffsetY = 8;

    public float minYPos = 6;
    public float minXPos = 13f;
    public float maxXPos = 500f;

    [Header("Background Variables")]
    public float[] layerSpeedX = new float[5] {1, 0.8f, 0.6f, 0.5f, 0 };
    public float[] layerSpeedY = new float[5] {0, 0, -0.5f, -0.7f, -1 };
    public GameObject[] Layer_Objects = new GameObject[5];

    private float[] startPos = new float[5];

    public Transform parent;
    public float offsetX = 2;

    private float sizeX;
    private float boundSizeX;

    private void Start() {
        if (playerTransform == null) { playerTransform = FindObjectOfType<Player>().transform; }
        if (playerRb == null) { playerRb = FindObjectOfType<Player>().GetComponent<Rigidbody2D>(); }

        Vector3 desiredPosition = new(playerTransform.position.x + playerOffsetX, transform.position.y, transform.position.z);
        desiredPosition.x = Mathf.Max(minXPos, desiredPosition.x);

        transform.position = desiredPosition;

        sizeX = Layer_Objects[0].transform.localScale.x * parent.localScale.x;
        boundSizeX = Layer_Objects[0].GetComponent<SpriteRenderer>().sprite.bounds.size.x;

        offsetX += transform.position.x % (boundSizeX * sizeX);


        for (int i = 0; i < startPos.Length; i++) {
            startPos[i] = transform.position.x;
        }
    }

    void Update() {
        bool movingRight = playerRb.velocity.x >= 0;
        float yPos;

        if (playerTransform.position.y >= playerOffsetY) {
            yPos = playerTransform.position.y - playerOffsetY + minYPos;
        } else { yPos = minYPos; }

        if (movingRight) {
            int multiplyingFactor = 1;
            if (playerTransform.position.x > transform.position.x - playerOffsetX) { multiplyingFactor = 2; }

            float xPos = transform.position.x + multiplyingFactor * playerRb.velocity.x * Time.deltaTime;
            if (xPos > playerTransform.position.x + playerOffsetX) {
                xPos = playerTransform.position.x + playerOffsetX;
            }
            xPos = Mathf.Max(minXPos, xPos);

            Vector3 desiredPosition = new(xPos, yPos, transform.position.z);
            transform.position = desiredPosition;
        } else {
            int multiplyingFactor = 1;
            if (playerTransform.position.x < transform.position.x + playerOffsetX) { multiplyingFactor = 2; }

            float xPos = transform.position.x + multiplyingFactor * playerRb.velocity.x * Time.deltaTime;
            if (xPos < playerTransform.position.x - playerOffsetX) {
                xPos = playerTransform.position.x - playerOffsetX;
            }
            xPos = Mathf.Max(minXPos, xPos);

            Vector3 desiredPosition = new(xPos, yPos, transform.position.z);
            transform.position = desiredPosition;
        }


        for (int i = 0; i < Layer_Objects.Length; i++) {
            float temp = transform.position.x * (1 - layerSpeedX[i]);
            float distanceX = transform.position.x * layerSpeedX[i];

            float distanceY = transform.position.y + (transform.position.y - minYPos) * layerSpeedY[i];

            Layer_Objects[i].transform.position = new Vector2(startPos[i] + distanceX - offsetX, distanceY);

            if (temp > startPos[i] + boundSizeX * sizeX - offsetX) {
                startPos[i] += boundSizeX * sizeX;
            } else if (temp < startPos[i] - boundSizeX * sizeX - offsetX) {
                startPos[i] -= boundSizeX * sizeX;
            }

        }
    }
}

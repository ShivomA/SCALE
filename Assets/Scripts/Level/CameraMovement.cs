using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour {
    [Header("Player Movement Variables")]
    public Transform playerTransform;
    public Rigidbody2D playerRb;

    public float playerOffsetX = 6;
    public float playerOffsetY = 8;
    public float cameraControlSpeed = 15;

    public float minYPos = 6;
    public float minXPos = 13f;
    public float maxXPos = 500f;

    [Header("Background Variables")]
    public List<float> layerSpeedX = new() { 1, 0.8f, 0.6f, 0.5f, 0 };
    public List<float> layerSpeedY = new() { 0, 0, -0.5f, -0.7f, -1 };
    public List<GameObject> Layer_Objects;

    private List<float> startPos = new() { 0, 0, 0, 0, 0 };

    public Transform parent;
    public float offsetX = 2;

    private float sizeX;
    private float boundSizeX;

    private void Start() {
        if (playerTransform == null) { playerTransform = FindObjectOfType<Player>().transform; }
        if (playerRb == null) { playerRb = FindObjectOfType<Player>().GetComponent<Rigidbody2D>(); }

        if (parent == null) { parent = transform.GetChild(0); }
        if (Layer_Objects.Count == 0) {
            for (int i = 0; i < parent.childCount; i++) {
                GameObject backgroundLayer = parent.GetChild(i).gameObject;
                Layer_Objects.Add(backgroundLayer);
            }
        }

        Vector3 desiredPosition = new(playerTransform.position.x + playerOffsetX, transform.position.y, transform.position.z);
        desiredPosition.x = Mathf.Clamp(desiredPosition.x, minXPos, maxXPos);

        transform.position = desiredPosition;

        sizeX = Layer_Objects[0].transform.localScale.x * parent.localScale.x;
        boundSizeX = Layer_Objects[0].GetComponent<SpriteRenderer>().sprite.bounds.size.x;

        offsetX += transform.position.x % (boundSizeX * sizeX);

        for (int i = 0; i < startPos.Count; i++) {
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
            xPos = Mathf.Clamp(xPos, minXPos, maxXPos);

            Vector3 desiredPosition = new(xPos, yPos, transform.position.z);
            transform.position = desiredPosition;
        } else {
            int multiplyingFactor = 1;
            if (playerTransform.position.x < transform.position.x + playerOffsetX) { multiplyingFactor = 2; }

            float xPos = transform.position.x + multiplyingFactor * playerRb.velocity.x * Time.deltaTime;
            if (xPos < playerTransform.position.x - playerOffsetX) {
                xPos = playerTransform.position.x - playerOffsetX;
            }
            xPos = Mathf.Clamp(xPos, minXPos, maxXPos);

            Vector3 desiredPosition = new(xPos, yPos, transform.position.z);
            transform.position = desiredPosition;
        }


        for (int i = 0; i < Layer_Objects.Count; i++) {
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

        if (Input.GetKey(KeyCode.E)) {
            float xPos = transform.position.x + cameraControlSpeed * Time.deltaTime;
            if (xPos > playerTransform.position.x + playerOffsetX) {
                xPos = playerTransform.position.x + playerOffsetX;
            }
            xPos = Mathf.Clamp(xPos, minXPos, maxXPos);

            Vector3 desiredPosition = new(xPos, yPos, transform.position.z);
            transform.position = desiredPosition;
        }
        if (Input.GetKey(KeyCode.Q)) {
            float xPos = transform.position.x - cameraControlSpeed * Time.deltaTime;
            if (xPos < playerTransform.position.x - playerOffsetX) {
                xPos = playerTransform.position.x - playerOffsetX;
            }
            xPos = Mathf.Clamp(xPos, minXPos, maxXPos);

            Vector3 desiredPosition = new(xPos, yPos, transform.position.z);
            transform.position = desiredPosition;
        }
    }
}

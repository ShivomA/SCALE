using System.Collections.Generic;
using UnityEngine;

public class ResetCameraPosition : MonoBehaviour {
    public Transform playerTransform;
    public CameraMovement cameraMovement;
    public PlayerController playerController;

    public float cameraMovementSpeed = 30;
    public float maxCameraMovementSpeed = 150;

    private List<float> startPos = new() { 0, 0, 0, 0, 0 };

    private float sizeX;
    private float boundSizeX;

    private void Start() {
        if (playerTransform == null) {
            playerTransform = FindObjectOfType<Player>().transform;
        }
        if (cameraMovement == null) {
            cameraMovement = GetComponent<CameraMovement>();
        }
        if (playerController == null) {
            playerController = FindObjectOfType<PlayerController>();
        }

        sizeX = cameraMovement.Layer_Objects[0].transform.localScale.x * cameraMovement.parent.localScale.x;
        boundSizeX = cameraMovement.Layer_Objects[0].GetComponent<SpriteRenderer>().sprite.bounds.size.x;

        for (int i = 0; i < startPos.Count; i++) {
            startPos[i] = transform.position.x;
        }
    }

    void Update() {
        float xPos;
        float yPos;

        if (playerTransform.position.y >= cameraMovement.playerOffsetY) {
            yPos = playerTransform.position.y - cameraMovement.playerOffsetY + cameraMovement.minYPos;
        } else { yPos = cameraMovement.minYPos; }

        if (playerTransform.position.x - transform.position.x < 0) {
            xPos = transform.position.x - cameraMovementSpeed * Time.deltaTime;
            if (transform.position.x <= playerTransform.position.x + cameraMovement.playerOffsetX || xPos <= cameraMovement.minXPos) { RemoveScript(); }
        } else {
            xPos = transform.position.x + cameraMovementSpeed * Time.deltaTime;
            if (transform.position.x >= playerTransform.position.x - cameraMovement.playerOffsetX || xPos >= cameraMovement.maxXPos) { RemoveScript(); }
        }
        cameraMovementSpeed += 10 * Time.deltaTime;
        cameraMovementSpeed = Mathf.Min(cameraMovementSpeed, maxCameraMovementSpeed);

        Vector3 desiredPosition = new(xPos, yPos, transform.position.z);
        transform.position = desiredPosition;


        for (int i = 0; i < cameraMovement.Layer_Objects.Count; i++) {
            float temp = transform.position.x * (1 - cameraMovement.layerSpeedX[i]);
            float distanceX = transform.position.x * cameraMovement.layerSpeedX[i];

            float distanceY = transform.position.y + (transform.position.y - cameraMovement.minYPos) * cameraMovement.layerSpeedY[i];

            cameraMovement.Layer_Objects[i].transform.position = new Vector2(startPos[i] + distanceX - cameraMovement.offsetX, distanceY);

            if (temp > startPos[i] + boundSizeX * sizeX - cameraMovement.offsetX) {
                startPos[i] += boundSizeX * sizeX;
            } else if (temp < startPos[i] - boundSizeX * sizeX - cameraMovement.offsetX) {
                startPos[i] -= boundSizeX * sizeX;
            }

        }
    }

    private void RemoveScript() {
        enabled = false;
        cameraMovementSpeed = 50;
        cameraMovement.enabled = true;
        playerController.enabled = true;
    }
}

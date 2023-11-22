using System.Collections.Generic;
using UnityEngine;

public class LevelController : MonoBehaviour {
    public CameraMovement cameraMovement;
    public PlayerController playerController;
    public ResetCameraPosition resetCameraPosition;

    public List<GameObject> unstablePlatforms;
    public List<GameObject> unstableObjects;

    public List<GameObject> activeUnstablePlatforms;
    public List<GameObject> activeUnstableObjects;

    private void Start() {
        if (cameraMovement == null) {
            cameraMovement = FindObjectOfType<CameraMovement>();
        }
        if (playerController == null) {
            playerController = FindObjectOfType<PlayerController>();
        }
        if (resetCameraPosition == null) {
            resetCameraPosition = FindObjectOfType<ResetCameraPosition>();
        }

        foreach (GameObject unstablePlatform in unstablePlatforms) {
            unstablePlatform.SetActive(false);
        }

        foreach (GameObject unstableObject in unstableObjects) {
            unstableObject.SetActive(false);
        }

        InitialiseLevelCompletely();
    }

    private void InitialiseLevelCompletely() {
        for (int i = 0; i < unstablePlatforms.Count; i++) {
            GameObject unstablePlatform = unstablePlatforms[i];
            Vector3 spawnPosition = unstablePlatform.transform.position;
            Quaternion spawnRotation = Quaternion.identity;

            GameObject spawnedUnstablePlatform = Instantiate(unstablePlatform, spawnPosition, spawnRotation);
            spawnedUnstablePlatform.SetActive(true);
            activeUnstablePlatforms.Add(spawnedUnstablePlatform);

        }

        for (int i = 0; i < unstableObjects.Count; i++) {
            GameObject unstableObject = unstableObjects[i];
            Vector3 spawnPosition = unstableObject.transform.position;
            Quaternion spawnRotation = Quaternion.identity;

            GameObject spawnedUnstableObject = Instantiate(unstableObject, spawnPosition, spawnRotation);
            spawnedUnstableObject.SetActive(true);
            activeUnstableObjects.Add(spawnedUnstableObject);
        }
    }

    public void ResetLevelCompletely() {
        while (activeUnstablePlatforms.Count != 0) {
            Destroy(activeUnstablePlatforms[0]);
            activeUnstablePlatforms.RemoveAt(0);
        }

        while (activeUnstableObjects.Count != 0) {
            Destroy(activeUnstableObjects[0]);
            activeUnstableObjects.RemoveAt(0);
        }

        InitialiseLevelCompletely();
    }

    private void InitialiseLevel() {
        for (int i = 0; i < unstablePlatforms.Count; i++) {
            GameObject unstablePlatform = unstablePlatforms[i];
            Vector3 spawnPosition = unstablePlatform.transform.position;
            Quaternion spawnRotation = Quaternion.identity;

            GameObject spawnedUnstablePlatform = Instantiate(unstablePlatform, spawnPosition, spawnRotation);
            spawnedUnstablePlatform.SetActive(true);
            activeUnstableObjects.Add(spawnedUnstablePlatform);
        }
    }

    public void ResetLevel() {
        while (activeUnstablePlatforms.Count != 0) {
            Destroy(activeUnstablePlatforms[0]);
            activeUnstablePlatforms.RemoveAt(0);
        }

        InitialiseLevel();
    }

    public void PlayerDied() {
        cameraMovement.enabled = false;
        playerController.enabled = false;

        Invoke(nameof(FocusPlayer), 1);
        Invoke(nameof(ResetLevel), 1);
    }

    private void FocusPlayer() {
        resetCameraPosition.enabled = true;
    }
}

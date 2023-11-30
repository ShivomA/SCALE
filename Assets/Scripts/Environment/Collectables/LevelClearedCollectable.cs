using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelClearedCollectable : MonoBehaviour {
    public GameObject wonMenuPanel;

    public List<GameObject> chains;
    public List<GameObject> enemies;

    private int totalChains;
    private int totalEnemies;
    private float stepPercentage;
    private bool isFinalCollectableAvailable = false;

    public GameObject wonText;
    public GameObject hintText;

    public SoundManager soundManager;

    private void Start() {
        if (soundManager == null)
            soundManager = FindObjectOfType<SoundManager>();

        if (enemies.Count == 0)
            enemies = new List<GameObject>(GameObject.FindGameObjectsWithTag("Enemy"));

        if (chains.Count == 0) {
            for (int i = 0; i < transform.childCount; i++) {
                GameObject chain = transform.GetChild(i).gameObject;
                chains.Add(chain);
            }
        }

        totalChains = chains.Count;
        totalEnemies = enemies.Count;
        stepPercentage = 100 / totalChains;
    }

    private void Update() {
        if (!isFinalCollectableAvailable) {
            CheckChainsRemained();
            if (chains.Count == 0) {
                UnlockFinalCollectable();
            }
        }
    }

    private void CheckChainsRemained() {
        if (enemies.Count == 0) {
            Destroy(chains[0]);
            chains.RemoveAt(0);
            return;
        }

        enemies.RemoveAll(enemy => enemy == null);

        for (int i = 0; i < totalChains; i++) {
            if (enemies.Count / totalEnemies * 100 < stepPercentage * i) {
                if (chains.Count == i + 1) {
                    Destroy(chains[0]);
                    chains.RemoveAt(0);
                    return;
                }
            }
        }
    }

    private void UnlockFinalCollectable() {
        isFinalCollectableAvailable = true;
        GetComponent<PolygonCollider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.CompareTag("Player")) {
            soundManager.PlayLevelCompletionSound();

            wonText.SetActive(true);
            Invoke(nameof(HideWonMessage), 3);
            GetComponent<SpriteRenderer>().enabled = false;

            if (wonMenuPanel) {
                StartCoroutine(ScaleDownCoroutine(collision.transform, collision.gameObject.GetComponent<PlayerController>()));
            }
        }
    }

    IEnumerator ScaleDownCoroutine(Transform playerTransform, PlayerController playerController) {
        float scaleSpeed = 0.2f;

        while (playerTransform.localScale.x > 0 && playerTransform.localScale.y > 0) {
            float newScale = Mathf.Max(playerTransform.localScale.x - scaleSpeed * Time.deltaTime, 0);
            playerTransform.localScale = new Vector3(newScale, newScale, newScale);

            playerController.sizeScale = newScale * 10;

            yield return null;
        }

        playerController.enabled = false;
        wonMenuPanel.SetActive(true);
        Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        if (collision.collider.gameObject.CompareTag("Player")) {
            hintText.SetActive(true);
            Invoke(nameof(HideHint), 3);
        }
    }

    private void HideHint() {
        hintText.SetActive(false);
    }

    private void HideWonMessage() {
        wonText.SetActive(false);
    }
}

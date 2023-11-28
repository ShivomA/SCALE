using System.Collections.Generic;
using UnityEngine;

public class Level2BossSpawner : MonoBehaviour {
    public List<GameObject> enemiesToSpawn;

    void Start() {
        foreach (GameObject enemy in enemiesToSpawn) {
            enemy.SetActive(false);
        }
    }

    private void OnDestroy() {
        foreach (GameObject enemy in enemiesToSpawn) {
            enemy.SetActive(true);
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

public class Level2BossSpawner : MonoBehaviour {
    public List<TargetingFlyingEnemy> enemiesToSpawn;

    void Start() {
        foreach (TargetingFlyingEnemy enemy in enemiesToSpawn) {
            enemy.enabled = false;
        }
    }

    private void OnDestroy() {
        foreach (TargetingFlyingEnemy enemy in enemiesToSpawn) {
            enemy.enabled = true;
        }
    }
}

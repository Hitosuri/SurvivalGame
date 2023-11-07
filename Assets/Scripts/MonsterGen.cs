using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterGen : MonoBehaviour
{
    [SerializeField]
    GameObject[] enemyPrefabs;
    [SerializeField]
    float spawnRate = 1f;
    [SerializeField]
    bool canSpawn = true;

    private GameObject spawnedEnemy;

    private void Start()
    {
        StartCoroutine(Spawner());
    }

    IEnumerator Spawner()
    {
        WaitForSeconds wait = new WaitForSeconds(spawnRate);

        while (canSpawn)
        {
            yield return wait;

            // Check if there is no spawned enemy
            if (spawnedEnemy == null)
            {
                // Wait for 3 seconds
                yield return new WaitForSeconds(3f);

                // Spawn a new enemy
                int rand = UnityEngine.Random.Range(0, enemyPrefabs.Length);
                GameObject enemyToSpawn = enemyPrefabs[rand];
                spawnedEnemy = Instantiate(enemyToSpawn, transform.position, Quaternion.identity);
            }
        }
    }
}
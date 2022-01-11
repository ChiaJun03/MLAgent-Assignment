using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Area")]
    [SerializeField]
    private GameObject staffLounge;
    [SerializeField]
    private GameObject waitingArea;
    [SerializeField]
    private GameObject openArea;

    [Header("Spawner")]
    [SerializeField]
    private GameObject enemyPrefab;

    [SerializeField]
    private int waveTotal = 5; // total number of wave
    public static int waveCounter;

    private float newWaveTimer = 0;
    private bool waitingForWave = true;

    private int zombiePerWave = 5;
    private int spawnCount;
    private int maxEnemyCount;
    public static int currentEnemyCount;
    public static int enemyKilled = 0;

    private GameObject[] zombies;



    // Start is called before the first frame update
    void Start()
    {
        currentEnemyCount = 0;
        waveCounter = 0;
        maxEnemyCount = waveCounter * zombiePerWave;
        zombies =  new GameObject[waveTotal * zombiePerWave];
    }

    // Update is called once per frame
    void Update()
    {
        checkWinCondition();
        enemySpawner();
    }

    private void checkWinCondition()
    {
        if (waitingForWave && waveCounter == waveTotal)
        {
            Debug.Log("Ended");
            Time.timeScale = 0f;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            SceneManager.LoadScene("EndScene");

        }

    }

    private void enemySpawner()
    {
        if (waitingForWave)
        {
            if(newWaveTimer >= 0)
            {
                newWaveTimer -= Time.deltaTime;
            }
            else
            {
                if (waveCounter != waveTotal)
                {
                    //Initialize new wave
                    waveCounter++;
                    maxEnemyCount = waveCounter * zombiePerWave;
                    currentEnemyCount = 0;
                    waitingForWave = false;
                    enemyKilled = 0;
                }
            }
        }
        else
        {
            if (currentEnemyCount < maxEnemyCount)
            {
                Vector3 spawnPosition = Vector3.zero;

                // Random area to spawn
                int areaIndex = Random.Range(0,100);
                if (areaIndex < 25) // Staff Lounge - 25%
                {
                    spawnPosition = new Vector3(Random.Range(-5.0f, 5.0f), 0.0f, Random.Range(-5.0f, 5.0f));
                    spawnPosition += staffLounge.transform.position;
                }
                else if (areaIndex >= 25 && areaIndex < 55) // Waiting Area - 30%
                {
                    spawnPosition = new Vector3(Random.Range(-4.5f, 4.5f), 0.0f, Random.Range(-7.5f, 7.5f));
                    spawnPosition += waitingArea.transform.position;
                }
                else // Open Area - 45%
                {
                    spawnPosition = new Vector3(Random.Range(-7.5f, 7.5f), 0.0f, Random.Range(-7.5f, 7.5f));
                    spawnPosition += openArea.transform.position;
                }
                
                zombies[currentEnemyCount] = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
                currentEnemyCount +=1;
                Debug.Log(currentEnemyCount);
            }
            else
            {
                bool allDead = true;
                // Check if all the zombies has dead
                foreach (GameObject zombie in zombies)
                {
                    if (zombie != null)
                    {
                        allDead = false;
                    } 
                }

                if (allDead) // If all zombie is dead, spawn new wave
                {
                    waitingForWave = true;
                    newWaveTimer = 5;
                }
            }
        }
        
    }

    // void OnDrawGizmos()
    // {
    //     // Draw a yellow cube at the transform position
    //     Gizmos.color = Color.red;
    //     Gizmos.DrawCube(staffLounge.transform.position, new Vector3(10, 1, 10));
    //     Gizmos.DrawCube(waitingArea.transform.position, new Vector3(9, 1, 15));
    //     Gizmos.DrawCube(openArea.transform.position, new Vector3(15, 1, 15));
    // }

}

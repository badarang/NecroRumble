using System.Numerics;
using LOONACIA.Unity.Managers;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class CrowSpawner : MonoBehaviour
{
    private GameObject crowPrefab;
    private int maxCrowNum = 10;

    private void Start()
    {
        crowPrefab = Resources.Load<GameObject>("Prefabs/Maps/Crow");
        if(SceneManagerEx.CurrentScene.SceneType is SceneType.Title)
        {
            maxCrowNum = 5;
            InvokeRepeating("SpawnCrowsTitle", 5f, 5f);
        }
        else
        {
            InvokeRepeating("SpawnCrows", 15f, 15f);
        }
        
    }

    private void SpawnCrows()
    {
        int currentCrowNum = GameObject.FindGameObjectsWithTag("Crow").Length;

        if (currentCrowNum < maxCrowNum)
        {
            int numToSpawn = Random.Range(3, 5);

            for (int i = 0; i < numToSpawn; i++)
            {
                Vector3 spawnPosition = GetRandomSpawnPosition();
                Instantiate(crowPrefab, spawnPosition, Quaternion.identity);
            }
        }
    }

    private void SpawnCrowsTitle()
    {
        int currentCrowNum = GameObject.FindGameObjectsWithTag("Crow").Length;

        if (currentCrowNum < maxCrowNum)
        {
            int numToSpawn = Random.Range(1, 2);

            for (int i = 0; i < numToSpawn; i++)
            {
                Vector3 spawnPosition = GetRandomSpawnPositionTitle();
                Instantiate(crowPrefab, spawnPosition, Quaternion.identity);
            }
        }
    }
    
    private Vector3 GetRandomSpawnPosition()
    {
        Vector3 playerPosition = GameManager.Instance.GetPlayer().transform.position;
        
        int xSign = Random.Range(0, 2) == 0 ? -1 : 1;
        int ySign = Random.Range(0, 2) == 0 ? -1 : 1;
        
        Vector3 randomOffset = new Vector2(Random.Range(20f, 25f) * xSign, Random.Range(10f, 15f) * ySign);
        return playerPosition + randomOffset;
    }
    
    private Vector3 GetRandomSpawnPositionTitle()
    {
        Vector3 playerPosition = Vector2.zero;
        
        int xSign = Random.Range(0, 2) == 0 ? -1 : 1;
        int ySign = Random.Range(0, 2) == 0 ? -1 : 1;
        
        Vector3 randomOffset = new Vector2(Random.Range(10f, 12f) * xSign, Random.Range(6f, 9f) * ySign);
        return playerPosition + randomOffset;
    }
}
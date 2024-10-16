using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudSpawner : MonoBehaviour
{
    private GameObject cloudContainer;
    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating("SpawnClouds", 15f, 15f);
        cloudContainer = Resources.Load<GameObject>("Prefabs/Maps/CloudContainer");
    }

    private void SpawnClouds()
    {
        int currentCloudNum = GameObject.FindGameObjectsWithTag("Cloud").Length;
        if (currentCloudNum < 10)
        {
            int numOfClouds = cloudContainer.transform.childCount;
            int spawnNum = Random.Range(1, numOfClouds);
            
            Vector3 spawnPosition = GetRandomSpawnPosition();
            GameObject cloud = Instantiate(cloudContainer, spawnPosition, Quaternion.identity);
            cloud.transform.GetChild(0).gameObject.SetActive(true);
            cloud.transform.GetChild(spawnNum).gameObject.SetActive(true);
        }
    }
    
    private Vector3 GetRandomSpawnPosition()
    {
        Vector3 playerPosition = GameManager.Instance.GetPlayer().transform.position;
        
        int xSign = Random.Range(0, 2) == 0 ? -1 : 1;
        int ySign = Random.Range(0, 2) == 0 ? -1 : 1;
        
        Vector3 randomOffset = new Vector2(Random.Range(25f, 30f) * xSign, Random.Range(15f, 20f) * ySign);
        return playerPosition + randomOffset;
    }
}

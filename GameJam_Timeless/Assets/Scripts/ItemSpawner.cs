using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] public Transform[] spawnPoints;
    [SerializeField] public float spawnInterval = 2f;
    
    private float timer;

    void Update()
    {
        timer += Time.deltaTime;
        
        if (timer >= spawnInterval)
        {
            SpawnItem();
            timer = 0f;
        }
    }

    void SpawnItem()
    {
        if (spawnPoints.Length == 0) return;
        
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        
        GameObject item = Instantiate(itemPrefab, spawnPoint.position, Quaternion.identity);
        
        if (item.GetComponent<Rigidbody>() == null)
        {
            item.AddComponent<Rigidbody>();
        }
        
        Destroy(item, 20f);
    }
}

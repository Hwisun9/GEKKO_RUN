using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    public GameObject[] obstaclePrefabs;  // 장애물 프리팹 배열
    public GameObject[] itemPrefabs;      // 아이템 프리팹 배열
    public float spawnRate = 2f;          // 생성 빈도
    public float minX, maxX;              // 생성 위치 범위

    private float timer = 0f;

    void Update()
    {
        if (!GameManager.Instance.isGameActive) return;

        timer += Time.deltaTime;

        if (timer >= spawnRate)
        {
            timer = 0f;
            SpawnObject();
        }
    }

    // 오브젝트 생성
    void SpawnObject()
    {
        // 아이템 또는 장애물 생성 랜덤 결정
        bool spawnItem = Random.Range(0, 100) < 30;  // 30% 확률로 아이템 생성

        GameObject[] prefabsToUse = spawnItem ? itemPrefabs : obstaclePrefabs;
        GameObject prefab = prefabsToUse[Random.Range(0, prefabsToUse.Length)];

        float randomX = Random.Range(minX, maxX);
        Vector3 spawnPosition = new Vector3(randomX, transform.position.y, 0);

        GameObject spawnedObject = Instantiate(prefab, spawnPosition, Quaternion.identity);
        spawnedObject.GetComponent<ObjectMovement>().SetSpeed(GameManager.Instance.gameSpeed);
    }
}

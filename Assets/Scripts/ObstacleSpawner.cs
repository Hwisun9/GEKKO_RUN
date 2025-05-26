using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    public GameObject[] obstaclePrefabs;  // ��ֹ� ������ �迭
    public GameObject[] itemPrefabs;      // ������ ������ �迭
    public float spawnRate = 2f;          // ���� ��
    public float minX, maxX;              // ���� ��ġ ����

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

    // ������Ʈ ����
    void SpawnObject()
    {
        // ������ �Ǵ� ��ֹ� ���� ���� ����
        bool spawnItem = Random.Range(0, 100) < 30;  // 30% Ȯ���� ������ ����

        GameObject[] prefabsToUse = spawnItem ? itemPrefabs : obstaclePrefabs;
        GameObject prefab = prefabsToUse[Random.Range(0, prefabsToUse.Length)];

        float randomX = Random.Range(minX, maxX);
        Vector3 spawnPosition = new Vector3(randomX, transform.position.y, 0);

        GameObject spawnedObject = Instantiate(prefab, spawnPosition, Quaternion.identity);
        spawnedObject.GetComponent<ObjectMovement>().SetSpeed(GameManager.Instance.gameSpeed);
    }
}

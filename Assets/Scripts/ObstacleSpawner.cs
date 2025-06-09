using UnityEngine;

// 주의: 이 클래스는 사용되지 않으며, 새로운 Spawner.cs로 대체되었습니다.
// 기존의 레거시 코드와의 호환성을 위해 남겨둡니다.
public class ObstacleSpawner : MonoBehaviour
{
    public GameObject[] obstaclePrefabs;  // 장애물 프리팹 배열
    public GameObject[] itemPrefabs;      // 아이템 프리팹 배열
    public float spawnRate = 2f;          // 생성 률
    public float minX, maxX;              // 생성 위치 범위

    private float timer = 0f;
    private bool isLegacySpawnerActive = false;  // 이 클래스는 더 이상 사용되지 않음

    void Start()
    {
        Debug.LogWarning("ObstacleSpawner is deprecated. Using new Spawner system instead.");
        
        // 새로운 Spawner가 있는지 확인
        Spawner newSpawner = FindObjectOfType<Spawner>();
        if (newSpawner == null)
        {
            Debug.LogError("New Spawner not found! Re-activating legacy spawner.");
            isLegacySpawnerActive = true;
        }
    }

    void Update()
    {
        // 새 스포너가 없는 경우에만 실행
        if (!isLegacySpawnerActive || !GameManager.Instance.isGameActive) return;

        timer += Time.deltaTime;

        if (timer >= spawnRate)
        {
            timer = 0f;
            SpawnObject();
        }
    }

    // 오브젝트 생성 - 레거시 지원용
    void SpawnObject()
    {
        // 아이템 또는 장애물 생성 여부 결정
        bool spawnItem = Random.Range(0, 100) < 30;  // 30% 확률로 아이템 생성

        GameObject[] prefabsToUse = spawnItem ? itemPrefabs : obstaclePrefabs;
        if (prefabsToUse.Length == 0) return;
        
        GameObject prefab = prefabsToUse[Random.Range(0, prefabsToUse.Length)];

        float randomX = Random.Range(minX, maxX);
        Vector3 spawnPosition = new Vector3(randomX, transform.position.y, 0);

        GameObject spawnedObject = Instantiate(prefab, spawnPosition, Quaternion.identity);
        
        // MovingObject 컴포넌트 활용
        MovingObject movingObj = spawnedObject.GetComponent<MovingObject>();
        if (movingObj == null)
        {
            movingObj = spawnedObject.AddComponent<MovingObject>();
        }
        movingObj.speed = GameManager.Instance.gameSpeed;
        
        // 리지드바디 추가 및 설정
        Rigidbody2D rb = spawnedObject.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = spawnedObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
        }
        rb.linearVelocity = Vector2.down * GameManager.Instance.gameSpeed;
    }
}

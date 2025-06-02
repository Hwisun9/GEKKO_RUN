using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class SpawnItem
{
    public GameObject prefab;
    public float spawnWeight = 1f;
    public ItemType itemType = ItemType.Normal;
    public float speedMultiplier = 1f;
    public int scoreValue = 0;
    public Color effectColor = Color.white;
    public float spawnChance = 1f;
}

public enum ItemType
{
    Normal,
    Obstacle,
    Magnet,
    Mushroom,
    Hide
}

[System.Serializable]
public class SpawnWave
{
    public string waveName;
    public float duration = 10f;
    public float spawnInterval = 2f;
    public float itemChance = 0.3f;
    public float magnetChance = 0.05f;
    public float buffItemChance = 0.08f;
    public float speedMultiplier = 1f;
    public SpawnItem[] availableItems;
}

//  활성 오브젝트 정보를 저장하는 클래스 (Collider 기반)
[System.Serializable]
public class ActiveObject
{
    public GameObject gameObject;
    public Collider2D collider;
    public float spawnTime;

    public ActiveObject(GameObject obj)
    {
        gameObject = obj;
        collider = obj.GetComponent<Collider2D>();
        spawnTime = Time.time;
    }

    public bool IsValid()
    {
        return gameObject != null && collider != null;
    }

    public Bounds GetBounds()
    {
        if (IsValid())
        {
            return collider.bounds;
        }
        return new Bounds();
    }
}

public class Spawner : MonoBehaviour
{
    [Header("기본 설정")]
    public SpawnItem[] allSpawnItems;
    public float spawnRangeX = 2.5f;
    public float spawnPositionY = 6f;
    public float baseSpawnSpeed = 3f;

    [Header(" Collider 기반 겹침 방지 설정")]
    public int maxSpawnAttempts = 15; // 최대 스폰 시도 횟수
    public float activeObjectTrackTime = 3f; // 활성 오브젝트 추적 시간 (초)
    public float colliderPadding = 0.1f; // Collider 간 최소 여백 (아주 작은 값)
    public bool showSpawnDebug = false; // 디버그 표시 여부
    public LayerMask spawnCheckLayers = -1; // 체크할 레이어 (기본: 모든 레이어)

    [Header("웨이브 시스템")]
    public SpawnWave[] waves;
    public bool useWaveSystem = true;

    [Header("난이도 조절")]
    public float difficultyIncreaseRate = 0.1f;
    public float maxDifficultyMultiplier = 3f;

    [Header("특수 패턴")]
    public bool enableSpecialPatterns = true;
    public float specialPatternChance = 0.1f;

    [Header("아이템 생성 확률")]
    public float magnetItemChance = 0.08f;
    public float mushroomItemChance = 0.05f;
    public float hideItemChance = 0.03f;

    // 기존 변수들
    private float timer = 0f;
    private float gameTime = 0f;
    private int currentWaveIndex = 0;
    private float waveTimer = 0f;
    private float currentDifficultyMultiplier = 1f;
    private SpawnWave currentWave;
    private bool isSpecialPatternActive = false;
    private Dictionary<ItemType, int> itemTypeCount = new Dictionary<ItemType, int>();

    //  Collider 기반 겹침 방지 관련 변수들
    private List<ActiveObject> activeObjects = new List<ActiveObject>();
    private int successfulSpawns = 0;
    private int failedSpawns = 0;

    void Start()
    {
        InitializeSpawner();
    }

    void InitializeSpawner()
    {
        if (useWaveSystem && waves.Length > 0)
        {
            currentWave = waves[0];
        }

        InitializeItemTypeCount();
        ValidateSpawnerSetup();

        //  활성 오브젝트 리스트 초기화
        activeObjects = new List<ActiveObject>();

        Debug.Log($" Collider-based spawn system initialized - Padding: {colliderPadding}");
    }

    void InitializeItemTypeCount()
    {
        itemTypeCount.Clear();
        foreach (ItemType type in System.Enum.GetValues(typeof(ItemType)))
        {
            itemTypeCount[type] = 0;
        }

        foreach (var item in allSpawnItems)
        {
            if (itemTypeCount.ContainsKey(item.itemType))
            {
                itemTypeCount[item.itemType]++;
            }
        }
    }

    void ValidateSpawnerSetup()
    {
        Debug.Log("=== Spawner Setup Validation ===");

        foreach (var kvp in itemTypeCount)
        {
            Debug.Log($"{kvp.Key}: {kvp.Value} items");

            if (kvp.Value == 0)
            {
                Debug.LogWarning($"No {kvp.Key} items found in allSpawnItems array!");
            }
        }

        if (itemTypeCount[ItemType.Normal] == 0)
        {
            Debug.LogError("No normal items found! Game may not work properly.");
        }

        if (itemTypeCount[ItemType.Obstacle] == 0)
        {
            Debug.LogError("No obstacles found! Game may be too easy.");
        }

        Debug.Log($"Total spawn items configured: {allSpawnItems.Length}");
    }

    void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.isGameActive) return;

        UpdateGameTime();
        UpdateActiveObjects(); //  활성 오브젝트 업데이트
        UpdateWaveSystem();
        UpdateDifficulty();
        UpdateSpawning();
    }

    void UpdateGameTime()
    {
        gameTime += Time.deltaTime;
    }

    //  활성 오브젝트 리스트 업데이트
    void UpdateActiveObjects()
    {
        // 삭제된 오브젝트나 시간이 지난 오브젝트, 화면 밖으로 나간 오브젝트 제거
        activeObjects.RemoveAll(obj =>
            !obj.IsValid() ||
            (Time.time - obj.spawnTime) > activeObjectTrackTime ||
            obj.gameObject.transform.position.y < -2f
        );
    }

    void UpdateWaveSystem()
    {
        if (!useWaveSystem || waves.Length == 0) return;

        waveTimer += Time.deltaTime;

        if (waveTimer >= currentWave.duration)
        {
            currentWaveIndex = (currentWaveIndex + 1) % waves.Length;
            currentWave = waves[currentWaveIndex];
            waveTimer = 0f;

            Debug.Log($"웨이브 변경: {currentWave.waveName}");
        }
    }

    void UpdateDifficulty()
    {
        currentDifficultyMultiplier = 1f + (gameTime * difficultyIncreaseRate);
        currentDifficultyMultiplier = Mathf.Min(currentDifficultyMultiplier, maxDifficultyMultiplier);
    }

    void UpdateSpawning()
    {
        timer += Time.deltaTime;

        float currentSpawnInterval = useWaveSystem ? currentWave.spawnInterval : 2f;
        currentSpawnInterval /= currentDifficultyMultiplier;

        if (timer >= currentSpawnInterval)
        {
            timer = 0f;

            if (enableSpecialPatterns && Random.value < specialPatternChance && !isSpecialPatternActive)
            {
                StartCoroutine(ExecuteSpecialPattern());
            }
            else
            {
                SpawnObject();
            }
        }
    }

    void SpawnObject()
    {
        SpawnItem itemToSpawn = SelectSpawnItem();
        if (itemToSpawn == null) return;

        //  Collider 기반 겹치지 않는 위치 찾기
        Vector3 spawnPos = FindNonOverlappingPosition(itemToSpawn);
        if (spawnPos == Vector3.zero) // 적절한 위치를 찾지 못한 경우
        {
            failedSpawns++;
            if (showSpawnDebug)
            {
                Debug.LogWarning($" Failed to find non-overlapping position for {itemToSpawn.itemType}");
            }
            return;
        }

        GameObject obj = Instantiate(itemToSpawn.prefab, spawnPos, Quaternion.identity);

        SetupObjectMovement(obj, itemToSpawn);
        SetupObjectProperties(obj, itemToSpawn);

        //  활성 오브젝트 리스트에 추가
        activeObjects.Add(new ActiveObject(obj));

        successfulSpawns++;

        if (showSpawnDebug)
        {
            Debug.Log($" Spawned: {obj.name} at {spawnPos} (Active objects: {activeObjects.Count})");
        }
    }

    //  Collider 기반 겹치지 않는 스폰 위치 찾기
    Vector3 FindNonOverlappingPosition(SpawnItem spawnItem)
    {
        // 임시로 오브젝트를 생성해서 Collider 크기 확인
        GameObject tempObj = Instantiate(spawnItem.prefab);
        tempObj.SetActive(false); // 비활성화해서 다른 시스템에 영향 안주게

        Collider2D tempCollider = tempObj.GetComponent<Collider2D>();
        if (tempCollider == null)
        {
            // Collider가 없으면 추가
            tempCollider = tempObj.AddComponent<BoxCollider2D>();
        }

        // 여러 위치 시도
        for (int attempt = 0; attempt < maxSpawnAttempts; attempt++)
        {
            Vector3 candidatePos = CalculateSpawnPosition();

            if (IsPositionValidForCollider(candidatePos, tempCollider))
            {
                Destroy(tempObj); // 임시 오브젝트 삭제
                return candidatePos;
            }
        }

        Destroy(tempObj); // 임시 오브젝트 삭제

        // 모든 시도가 실패한 경우, 기본 위치 반환 (가장 적게 겹치는 곳)
        return FindLeastOverlappingPosition(spawnItem);
    }

    //  Collider 기반 위치 유효성 검사
    bool IsPositionValidForCollider(Vector3 position, Collider2D checkCollider)
    {
        // 임시 위치로 이동
        Vector3 originalPos = checkCollider.transform.position;
        checkCollider.transform.position = position;

        // 활성 오브젝트들과 겹치는지 확인
        foreach (var activeObj in activeObjects)
        {
            if (!activeObj.IsValid()) continue;

            // Bounds 기반 겹침 검사
            if (DoCollidersOverlap(checkCollider, activeObj.collider))
            {
                checkCollider.transform.position = originalPos; // 원래 위치로 복원
                return false;
            }
        }

        checkCollider.transform.position = originalPos; // 원래 위치로 복원
        return true;
    }

    //  두 Collider가 겹치는지 확인
    bool DoCollidersOverlap(Collider2D collider1, Collider2D collider2)
    {
        // Bounds 확장 (padding 적용)
        Bounds bounds1 = collider1.bounds;
        Bounds bounds2 = collider2.bounds;

        // 패딩 적용
        bounds1.Expand(colliderPadding * 2f);

        return bounds1.Intersects(bounds2);
    }

    // 가장 적게 겹치는 위치 찾기 (최후의 수단)
    Vector3 FindLeastOverlappingPosition(SpawnItem spawnItem)
    {
        Vector3 bestPosition = CalculateSpawnPosition();
        int bestOverlapCount = int.MaxValue;

        // 임시 오브젝트 생성
        GameObject tempObj = Instantiate(spawnItem.prefab);
        tempObj.SetActive(false);
        Collider2D tempCollider = tempObj.GetComponent<Collider2D>();
        if (tempCollider == null)
        {
            tempCollider = tempObj.AddComponent<BoxCollider2D>();
        }

        // 여러 후보 위치 중 가장 적게 겹치는 곳 선택
        for (int i = 0; i < 8; i++)
        {
            Vector3 candidatePos = CalculateSpawnPosition();
            int overlapCount = CountOverlaps(candidatePos, tempCollider);

            if (overlapCount < bestOverlapCount)
            {
                bestOverlapCount = overlapCount;
                bestPosition = candidatePos;

                if (overlapCount == 0) break; // 겹치지 않는 위치를 찾으면 즉시 반환
            }
        }

        Destroy(tempObj);
        return bestPosition;
    }

    //  특정 위치에서 겹치는 오브젝트 개수 계산
    int CountOverlaps(Vector3 position, Collider2D checkCollider)
    {
        Vector3 originalPos = checkCollider.transform.position;
        checkCollider.transform.position = position;

        int overlapCount = 0;
        foreach (var activeObj in activeObjects)
        {
            if (!activeObj.IsValid()) continue;

            if (DoCollidersOverlap(checkCollider, activeObj.collider))
            {
                overlapCount++;
            }
        }

        checkCollider.transform.position = originalPos;
        return overlapCount;
    }

    SpawnItem SelectSpawnItem()
    {
        List<SpawnItem> availableItems = GetAvailableItems();

        SpawnItem selectedItem = null;

        selectedItem = TrySelectBuffItem(availableItems);
        if (selectedItem != null) return selectedItem;

        selectedItem = TrySelectMagnetItem(availableItems);
        if (selectedItem != null) return selectedItem;

        selectedItem = SelectNormalOrObstacle(availableItems);

        return selectedItem;
    }

    List<SpawnItem> GetAvailableItems()
    {
        if (useWaveSystem && currentWave.availableItems.Length > 0)
        {
            return new List<SpawnItem>(currentWave.availableItems);
        }
        else
        {
            return new List<SpawnItem>(allSpawnItems);
        }
    }

    SpawnItem TrySelectBuffItem(List<SpawnItem> availableItems)
    {
        float buffChance = useWaveSystem ? currentWave.buffItemChance : (mushroomItemChance + hideItemChance);

        if (Random.value < buffChance)
        {
            var buffItems = availableItems.FindAll(item =>
                item.itemType == ItemType.Mushroom || item.itemType == ItemType.Hide);

            if (buffItems.Count > 0)
            {
                Debug.Log(" Spawning buff item!");
                return SelectByWeight(buffItems);
            }
        }

        return null;
    }

    SpawnItem TrySelectMagnetItem(List<SpawnItem> availableItems)
    {
        float magnetChance = useWaveSystem ? currentWave.magnetChance : magnetItemChance;

        if (Random.value < magnetChance)
        {
            var magnetItems = availableItems.FindAll(item => item.itemType == ItemType.Magnet);
            if (magnetItems.Count > 0)
            {
                Debug.Log(" Spawning magnet item!");
                return SelectByWeight(magnetItems);
            }
        }

        return null;
    }

    SpawnItem SelectNormalOrObstacle(List<SpawnItem> availableItems)
    {
        availableItems.RemoveAll(item =>
            item.itemType == ItemType.Magnet ||
            item.itemType == ItemType.Mushroom ||
            item.itemType == ItemType.Hide);

        float itemChance = useWaveSystem ? currentWave.itemChance : 0.3f;
        bool shouldSpawnItem = Random.value < itemChance;

        if (shouldSpawnItem)
        {
            availableItems.RemoveAll(item => item.itemType != ItemType.Normal);
            Debug.Log(" Spawning normal item!");
        }
        else
        {
            availableItems.RemoveAll(item => item.itemType != ItemType.Obstacle);
            Debug.Log(" Spawning obstacle!");
        }

        if (availableItems.Count == 0)
        {
            Debug.LogWarning("No items available after filtering!");
            return null;
        }

        return SelectByWeight(availableItems);
    }

    SpawnItem SelectByWeight(List<SpawnItem> items)
    {
        var validItems = items.FindAll(item => Random.value < item.spawnChance);
        if (validItems.Count == 0) validItems = items;

        float totalWeight = 0f;
        foreach (var item in validItems)
        {
            totalWeight += item.spawnWeight;
        }

        float randomValue = Random.value * totalWeight;
        float currentWeight = 0f;

        foreach (var item in validItems)
        {
            currentWeight += item.spawnWeight;
            if (randomValue <= currentWeight)
            {
                return item;
            }
        }

        return validItems[validItems.Count - 1];
    }

    Vector3 CalculateSpawnPosition()
    {
        float randomX = Random.Range(-spawnRangeX, spawnRangeX);
        return new Vector3(randomX, spawnPositionY, 0f);
    }

    void SetupObjectMovement(GameObject obj, SpawnItem spawnItem)
    {
        Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = obj.AddComponent<Rigidbody2D>();
        }

        rb.gravityScale = 0;

        // 속도 계산
        float finalSpeed = baseSpawnSpeed;
        finalSpeed *= spawnItem.speedMultiplier;

        if (useWaveSystem)
        {
            finalSpeed *= currentWave.speedMultiplier;
        }

        finalSpeed *= currentDifficultyMultiplier;

        //  부스터 효과 적용
        if (BoosterSystem.Instance != null && BoosterSystem.Instance.IsBoosterActive())
        {
            finalSpeed *= BoosterSystem.Instance.GetSpeedMultiplier();
        }

        rb.linearVelocity = Vector2.down * finalSpeed;

        MovingObject movingComponent = obj.AddComponent<MovingObject>();
        movingComponent.speed = finalSpeed;
    }

    void SetupObjectProperties(GameObject obj, SpawnItem spawnItem)
    {
        string tagToSet = GetTagForItemType(spawnItem.itemType);
        obj.tag = tagToSet;

        ScoreItem scoreComponent = obj.GetComponent<ScoreItem>();
        if (scoreComponent == null)
        {
            scoreComponent = obj.AddComponent<ScoreItem>();
        }

        scoreComponent.scoreValue = spawnItem.scoreValue;
        scoreComponent.effectColor = spawnItem.effectColor;

        Collider2D collider = obj.GetComponent<Collider2D>();
        if (collider == null)
        {
            collider = obj.AddComponent<BoxCollider2D>();
        }
        collider.isTrigger = true;
    }

    string GetTagForItemType(ItemType itemType)
    {
        switch (itemType)
        {
            case ItemType.Normal:
                return "Item";
            case ItemType.Obstacle:
                return "Obstacle";
            case ItemType.Magnet:
                return "Magnet";
            case ItemType.Mushroom:
                return "Mushroom";
            case ItemType.Hide:
                return "Hide";
            default:
                Debug.LogWarning($"Unknown ItemType: {itemType}");
                return "Item";
        }
    }

    // 특수 패턴들 (Collider 기반 겹침 방지 적용)
    IEnumerator ExecuteSpecialPattern()
    {
        isSpecialPatternActive = true;

        int patternType = Random.Range(0, 4);

        switch (patternType)
        {
            case 0:
                yield return StartCoroutine(ItemRainPattern());
                break;
            case 1:
                yield return StartCoroutine(ZigzagPattern());
                break;
            case 2:
                yield return StartCoroutine(WavePattern());
                break;
            case 3:
                yield return StartCoroutine(BonusTimePattern());
                break;
        }

        isSpecialPatternActive = false;
    }

    //  수정된 특수 패턴들 (Collider 기반 겹침 방지 적용)
    IEnumerator ItemRainPattern()
    {
        Debug.Log(" 특수 패턴: 아이템 비!");

        for (int i = 0; i < 8; i++)
        {
            var itemsOnly = System.Array.FindAll(allSpawnItems, item => item.itemType == ItemType.Normal);
            if (itemsOnly.Length > 0)
            {
                var randomItem = itemsOnly[Random.Range(0, itemsOnly.Length)];

                // Collider 기반 겹치지 않는 위치 찾기
                Vector3 pos = FindNonOverlappingPosition(randomItem);
                if (pos != Vector3.zero)
                {
                    GameObject obj = Instantiate(randomItem.prefab, pos, Quaternion.identity);
                    SetupObjectMovement(obj, randomItem);
                    SetupObjectProperties(obj, randomItem);

                    // 활성 오브젝트 리스트에 추가
                    activeObjects.Add(new ActiveObject(obj));
                }
            }

            yield return new WaitForSeconds(0.25f);
        }
    }

    IEnumerator ZigzagPattern()
    {
        Debug.Log(" 특수 패턴: 지그재그!");

        bool leftSide = true;
        for (int i = 0; i < 5; i++)
        {
            float xPos = leftSide ? -spawnRangeX * 0.8f : spawnRangeX * 0.8f;
            Vector3 pos = new Vector3(xPos, spawnPositionY, 0);

            var randomItem = allSpawnItems[Random.Range(0, allSpawnItems.Length)];
            GameObject obj = Instantiate(randomItem.prefab, pos, Quaternion.identity);
            SetupObjectMovement(obj, randomItem);
            SetupObjectProperties(obj, randomItem);

            // 활성 오브젝트 리스트에 추가
            activeObjects.Add(new ActiveObject(obj));

            leftSide = !leftSide;
            yield return new WaitForSeconds(0.4f);
        }
    }

    IEnumerator WavePattern()
    {
        Debug.Log(" 특수 패턴: 웨이브!");

        for (int i = 0; i < 6; i++)
        {
            float xPos = Mathf.Sin(i * 0.5f) * spawnRangeX;
            Vector3 pos = new Vector3(xPos, spawnPositionY, 0);

            var randomItem = allSpawnItems[Random.Range(0, allSpawnItems.Length)];
            GameObject obj = Instantiate(randomItem.prefab, pos, Quaternion.identity);
            SetupObjectMovement(obj, randomItem);
            SetupObjectProperties(obj, randomItem);

            // 활성 오브젝트 리스트에 추가
            activeObjects.Add(new ActiveObject(obj));

            yield return new WaitForSeconds(0.3f);
        }
    }

    IEnumerator BonusTimePattern()
    {
        Debug.Log(" 특수 패턴: 보너스 타임!");

        var highValueItems = System.Array.FindAll(allSpawnItems,
            item => item.itemType == ItemType.Normal && item.scoreValue > 15);

        if (highValueItems.Length > 0)
        {
            for (int i = 0; i < 4; i++)
            {
                var bonusItem = highValueItems[Random.Range(0, highValueItems.Length)];

                Vector3 pos = FindNonOverlappingPosition(bonusItem);
                if (pos != Vector3.zero)
                {
                    GameObject obj = Instantiate(bonusItem.prefab, pos, Quaternion.identity);
                    SetupObjectMovement(obj, bonusItem);
                    SetupObjectProperties(obj, bonusItem);

                    // 활성 오브젝트 리스트에 추가
                    activeObjects.Add(new ActiveObject(obj));
                }

                yield return new WaitForSeconds(0.5f);
            }
        }
    }

    //  디버깅용 공개 메서드들
    public string GetCurrentWaveInfo()
    {
        if (useWaveSystem && currentWave != null)
        {
            return $"{currentWave.waveName} ({waveTimer:F1}s / {currentWave.duration}s)";
        }
        return "무한 모드";
    }

    public float GetCurrentDifficulty()
    {
        return currentDifficultyMultiplier;
    }

    public Dictionary<ItemType, int> GetItemTypeStats()
    {
        return new Dictionary<ItemType, int>(itemTypeCount);
    }

    public string GetSpawnStats()
    {
        float successRate = successfulSpawns + failedSpawns > 0 ?
            (float)successfulSpawns / (successfulSpawns + failedSpawns) * 100f : 100f;

        return $"Spawns: {successfulSpawns} success, {failedSpawns} failed ({successRate:F1}% success rate)";
    }

    public int GetActiveObjectCount()
    {
        return activeObjects.Count;
    }

    //  디버그 표시 (Scene 뷰에서)
    void OnDrawGizmos()
    {
        if (!showSpawnDebug || !Application.isPlaying) return;

        // 활성 오브젝트들의 Collider 경계 표시
        Gizmos.color = Color.cyan;
        foreach (var activeObj in activeObjects)
        {
            if (activeObj.IsValid())
            {
                Bounds bounds = activeObj.GetBounds();
                Gizmos.DrawWireCube(bounds.center, bounds.size);
            }
        }

        // 스폰 범위 표시
        Gizmos.color = Color.green;
        Vector3 leftBound = new Vector3(-spawnRangeX, spawnPositionY, 0);
        Vector3 rightBound = new Vector3(spawnRangeX, spawnPositionY, 0);
        Gizmos.DrawLine(leftBound, rightBound);
    }
}
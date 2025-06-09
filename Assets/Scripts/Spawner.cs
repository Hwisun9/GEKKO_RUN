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
public class DifficultyStage
{
    public string stageName = "Stage";
    public float startTime = 0f;        // 시작 시간 (초)
    public float itemChance = 0.7f;     // 아이템 스폰 확률
    public float obstacleChance = 0.3f; // 장애물 스폰 확률
    public float spawnInterval = 2.0f;  // 스폰 간격
    public float moveSpeed = 3.0f;      // 이동 속도
}

[System.Serializable]
public class SpecialItemLimit
{
    public ItemType itemType;
    public int maxCountPerMinute = 3; // 분당 최대 스폰 개수
    private int currentCount = 0;
    private float resetTimer = 0f;

    public void Update(float deltaTime)
    {
        resetTimer += deltaTime;
        if (resetTimer >= 60f)
        {
            resetTimer = 0f;
            currentCount = 0;
        }
    }

    public bool CanSpawn()
    {
        return currentCount < maxCountPerMinute;
    }

    public void IncrementCount()
    {
        currentCount++;
    }

    public int GetRemainingCount()
    {
        return maxCountPerMinute - currentCount;
    }
}

// 활성 오브젝트 정보를 저장하는 클래스 (Collider 기반)
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

    [Header("난이도 스테이지")]
    public DifficultyStage[] difficultyStages;
    private DifficultyStage currentStage;
    private int currentStageIndex = 0;

    [Header("특별 아이템 제한")]
    public SpecialItemLimit[] specialItemLimits;
    private Dictionary<ItemType, SpecialItemLimit> itemLimitMap = new Dictionary<ItemType, SpecialItemLimit>();

    [Header("부스터 시스템 설정")]
    public bool giveExtraMagnetOnBoost = true; // 부스터 활성화 시 자석 아이템 제공 여부
    public float boostMagnetDelay = 1.5f;      // 부스터 활성화 후 자석 아이템 제공 지연 시간

    [Header("Collider 기반 겹침 방지 설정")]
    public int maxSpawnAttempts = 15;
    public float activeObjectTrackTime = 3f;
    public float colliderPadding = 0.1f;
    public bool showSpawnDebug = false;
    public LayerMask spawnCheckLayers = -1;

    [Header("특수 패턴")]
    public bool enableSpecialPatterns = true;
    public float specialPatternChance = 0.1f;

    // 상태 변수
    private float timer = 0f;
    private float gameTime = 0f;
    private bool isSpecialPatternActive = false;
    private Dictionary<ItemType, int> itemTypeCount = new Dictionary<ItemType, int>();
    private List<ActiveObject> activeObjects = new List<ActiveObject>();
    private int successfulSpawns = 0;
    private int failedSpawns = 0;
    private bool boosterWasActive = false;

    void Start()
    {
        InitializeSpawner();
    }

    void InitializeSpawner()
    {
        // 특별 아이템 제한 초기화
        itemLimitMap.Clear();
        foreach (var limit in specialItemLimits)
        {
            itemLimitMap[limit.itemType] = limit;
        }

        // 아이템 타입 카운트 초기화
        InitializeItemTypeCount();

        // 난이도 스테이지 정렬 및 초기화
        System.Array.Sort(difficultyStages, (a, b) => a.startTime.CompareTo(b.startTime));
        if (difficultyStages.Length > 0)
        {
            currentStage = difficultyStages[0];
        }
        else
        {
            // 기본 스테이지 생성
            currentStage = new DifficultyStage
            {
                stageName = "Default",
                itemChance = 0.7f,
                obstacleChance = 0.3f,
                spawnInterval = 2.0f,
                moveSpeed = baseSpawnSpeed
            };
        }

        // 활성 오브젝트 리스트 초기화
        activeObjects = new List<ActiveObject>();

        Debug.Log("Spawner initialized with " + difficultyStages.Length + " difficulty stages");
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

    void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.isGameActive) return;

        UpdateGameTime();
        UpdateActiveObjects();
        UpdateSpecialItemLimits();
        UpdateDifficultyStage();
        UpdateSpawning();
        CheckBoosterStatus();
    }

    void UpdateGameTime()
    {
        gameTime += Time.deltaTime;
    }

    void UpdateActiveObjects()
    {
        // 삭제된 오브젝트나 시간이 지난 오브젝트, 화면 밖으로 나간 오브젝트 제거
        activeObjects.RemoveAll(obj =>
            !obj.IsValid() ||
            (Time.time - obj.spawnTime) > activeObjectTrackTime ||
            obj.gameObject.transform.position.y < -2f
        );
    }

    void UpdateSpecialItemLimits()
    {
        // 각 특별 아이템 제한 업데이트
        foreach (var limit in specialItemLimits)
        {
            limit.Update(Time.deltaTime);
        }
    }

    void UpdateDifficultyStage()
    {
        // 현재 게임 시간에 맞는 난이도 스테이지 업데이트
        for (int i = difficultyStages.Length - 1; i >= 0; i--)
        {
            if (gameTime >= difficultyStages[i].startTime)
            {
                if (currentStageIndex != i)
                {
                    currentStageIndex = i;
                    currentStage = difficultyStages[i];
                    Debug.Log("Difficulty stage changed to: " + currentStage.stageName);
                }
                break;
            }
        }
    }

    void UpdateSpawning()
    {
        timer += Time.deltaTime;

        float currentSpawnInterval = currentStage.spawnInterval;

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

    void CheckBoosterStatus()
    {
        // 부스터 상태 확인
        bool isBoosterActive = BoosterSystem.Instance != null && BoosterSystem.Instance.IsBoosterActive();

        // 부스터가 새로 활성화되었을 때 자석 아이템 추가 제공
        if (isBoosterActive && !boosterWasActive && giveExtraMagnetOnBoost)
        {
            StartCoroutine(SpawnExtraMagnetAfterDelay());
        }

        boosterWasActive = isBoosterActive;
    }

    IEnumerator SpawnExtraMagnetAfterDelay()
    {
        // 부스터 활성화 후 약간의 딜레이 후 자석 아이템 생성
        yield return new WaitForSeconds(boostMagnetDelay);

        var magnetItems = System.Array.FindAll(allSpawnItems, item => item.itemType == ItemType.Magnet);
        if (magnetItems.Length > 0)
        {
            // 자석 아이템 중 하나를 무작위로 선택
            SpawnItem magnetItem = magnetItems[Random.Range(0, magnetItems.Length)];

            // 화면 중앙 상단에 자석 아이템 생성
            Vector3 spawnPos = new Vector3(0, spawnPositionY, 0);
            GameObject obj = Instantiate(magnetItem.prefab, spawnPos, Quaternion.identity);

            // 자석 아이템 설정
            SetupObjectMovement(obj, magnetItem);
            SetupObjectProperties(obj, magnetItem);

            // 활성 오브젝트 리스트에 추가
            activeObjects.Add(new ActiveObject(obj));

            Debug.Log("Extra magnet item spawned due to booster activation!");
        }
    }

    void SpawnObject()
    {
        SpawnItem itemToSpawn = SelectSpawnItem();
        if (itemToSpawn == null) return;

        // 특별 아이템 제한 확인
        if (IsSpecialItem(itemToSpawn.itemType))
        {
            var limit = GetSpecialItemLimit(itemToSpawn.itemType);
            if (limit != null && !limit.CanSpawn())
            {
                Debug.Log($"{itemToSpawn.itemType} limit reached. Trying to spawn normal item instead.");
                itemToSpawn = GetRandomNormalItem();
                if (itemToSpawn == null) return;
            }
            else if (limit != null)
            {
                limit.IncrementCount();
                Debug.Log($"{itemToSpawn.itemType} spawned ({limit.GetRemainingCount()} remaining this minute)");
            }
        }

        // Collider 기반 겹치지 않는 위치 찾기
        Vector3 spawnPos = FindNonOverlappingPosition(itemToSpawn);
        if (spawnPos == Vector3.zero) // 적절한 위치를 찾지 못한 경우
        {
            failedSpawns++;
            if (showSpawnDebug)
            {
                Debug.LogWarning($"Failed to find non-overlapping position for {itemToSpawn.itemType}");
            }
            return;
        }

        GameObject obj = Instantiate(itemToSpawn.prefab, spawnPos, Quaternion.identity);

        SetupObjectMovement(obj, itemToSpawn);
        SetupObjectProperties(obj, itemToSpawn);

        // 활성 오브젝트 리스트에 추가
        activeObjects.Add(new ActiveObject(obj));

        successfulSpawns++;

        if (showSpawnDebug)
        {
            Debug.Log($"Spawned: {obj.name} at {spawnPos} (Active objects: {activeObjects.Count})");
        }
    }

    // Collider 기반 겹치지 않는 스폰 위치 찾기
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

    // Collider 기반 위치 유효성 검사
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

    // 두 Collider가 겹치는지 확인
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

    // 특정 위치에서 겹치는 오브젝트 개수 계산
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
        // 난이도 스테이지 기반 아이템 선택
        float itemChance = currentStage.itemChance;
        float obstacleChance = currentStage.obstacleChance;

        // 난이도에 따라 아이템/장애물 선택
        float randomValue = Random.value;
        
        if (randomValue < itemChance)
        {
            // 일반 아이템 또는 특별 아이템 선택
            float specialItemChance = 0.2f; // 특별 아이템 확률 (20%)
            
            if (Random.value < specialItemChance && HasAvailableSpecialItem())
            {
                return SelectSpecialItem();
            }
            else
            {
                return GetRandomNormalItem();
            }
        }
        else
        {
            // 장애물 선택
            return GetRandomObstacleItem();
        }
    }

    // 사용 가능한 특별 아이템이 있는지 확인
    bool HasAvailableSpecialItem()
    {
        foreach (var limit in specialItemLimits)
        {
            if (limit.CanSpawn())
            {
                // 해당 타입의 아이템이 있는지 확인
                if (allSpawnItems.Any(item => item.itemType == limit.itemType))
                {
                    return true;
                }
            }
        }
        return false;
    }

    // 특별 아이템 선택
    SpawnItem SelectSpecialItem()
    {
        // 제한에 걸리지 않은 특별 아이템 타입 수집
        List<ItemType> availableTypes = new List<ItemType>();
        foreach (var limit in specialItemLimits)
        {
            if (limit.CanSpawn())
            {
                availableTypes.Add(limit.itemType);
            }
        }

        if (availableTypes.Count == 0)
        {
            return GetRandomNormalItem();
        }

        // 랜덤하게 특별 아이템 타입 선택
        ItemType selectedType = availableTypes[Random.Range(0, availableTypes.Count)];
        
        // 해당 타입의 아이템 중에서 선택
        List<SpawnItem> itemsOfType = new List<SpawnItem>();
        foreach (var item in allSpawnItems)
        {
            if (item.itemType == selectedType)
            {
                itemsOfType.Add(item);
            }
        }

        if (itemsOfType.Count == 0)
        {
            return GetRandomNormalItem();
        }

        return SelectByWeight(itemsOfType);
    }

    // 무작위 일반 아이템 가져오기
    SpawnItem GetRandomNormalItem()
    {
        var normalItems = System.Array.FindAll(allSpawnItems, item => item.itemType == ItemType.Normal);
        if (normalItems.Length == 0)
        {
            Debug.LogWarning("No normal items available!");
            return null;
        }
        return SelectByWeight(new List<SpawnItem>(normalItems));
    }

    // 무작위 장애물 가져오기
    SpawnItem GetRandomObstacleItem()
    {
        var obstacleItems = System.Array.FindAll(allSpawnItems, item => item.itemType == ItemType.Obstacle);
        if (obstacleItems.Length == 0)
        {
            Debug.LogWarning("No obstacle items available!");
            return null;
        }
        return SelectByWeight(new List<SpawnItem>(obstacleItems));
    }

    // 가중치에 따른 아이템 선택
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

    // 스폰 위치 계산
    Vector3 CalculateSpawnPosition()
    {
        float randomX = Random.Range(-spawnRangeX, spawnRangeX);
        return new Vector3(randomX, spawnPositionY, 0f);
    }

    // 오브젝트 이동 설정
    void SetupObjectMovement(GameObject obj, SpawnItem spawnItem)
    {
        Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = obj.AddComponent<Rigidbody2D>();
        }

        rb.gravityScale = 0;

        // 속도 계산 - 모든 요소의 속도를 통일
        float finalSpeed = currentStage.moveSpeed;

        // 부스터 효과 적용
        if (BoosterSystem.Instance != null && BoosterSystem.Instance.IsBoosterActive())
        {
            finalSpeed *= BoosterSystem.Instance.GetSpeedMultiplier();
        }

        rb.linearVelocity = Vector2.down * finalSpeed;

        MovingObject movingComponent = obj.GetComponent<MovingObject>();
        if (movingComponent == null)
        {
            movingComponent = obj.AddComponent<MovingObject>();
        }
        movingComponent.speed = finalSpeed;
    }

    // 오브젝트 속성 설정
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

    // 아이템 타입에 따른 태그 가져오기
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

    // 특별 아이템 제한 가져오기
    SpecialItemLimit GetSpecialItemLimit(ItemType itemType)
    {
        if (itemLimitMap.ContainsKey(itemType))
        {
            return itemLimitMap[itemType];
        }
        return null;
    }

    // 특별 아이템 여부 확인
    bool IsSpecialItem(ItemType itemType)
    {
        return itemType == ItemType.Magnet || 
               itemType == ItemType.Mushroom || 
               itemType == ItemType.Hide;
    }

    // 특수 패턴들
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

    // 아이템 비 패턴
    IEnumerator ItemRainPattern()
    {
        Debug.Log("특수 패턴: 아이템 비!");

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

    // 지그재그 패턴
    IEnumerator ZigzagPattern()
    {
        Debug.Log("특수 패턴: 지그재그!");

        bool leftSide = true;
        for (int i = 0; i < 5; i++)
        {
            float xPos = leftSide ? -spawnRangeX * 0.8f : spawnRangeX * 0.8f;
            Vector3 pos = new Vector3(xPos, spawnPositionY, 0);

            // 패턴용 아이템 선택 (50% 확률로 아이템, 50% 확률로 장애물)
            SpawnItem itemToSpawn;
            if (Random.value < 0.5f)
            {
                itemToSpawn = GetRandomNormalItem();
            }
            else
            {
                itemToSpawn = GetRandomObstacleItem();
            }

            if (itemToSpawn != null)
            {
                GameObject obj = Instantiate(itemToSpawn.prefab, pos, Quaternion.identity);
                SetupObjectMovement(obj, itemToSpawn);
                SetupObjectProperties(obj, itemToSpawn);

                // 활성 오브젝트 리스트에 추가
                activeObjects.Add(new ActiveObject(obj));
            }

            leftSide = !leftSide;
            yield return new WaitForSeconds(0.4f);
        }
    }

    // 웨이브 패턴
    IEnumerator WavePattern()
    {
        Debug.Log("특수 패턴: 웨이브!");

        for (int i = 0; i < 6; i++)
        {
            float xPos = Mathf.Sin(i * 0.5f) * spawnRangeX;
            Vector3 pos = new Vector3(xPos, spawnPositionY, 0);

            // 웨이브 패턴용 아이템 선택 (주로 아이템)
            SpawnItem itemToSpawn;
            if (Random.value < 0.7f)
            {
                itemToSpawn = GetRandomNormalItem();
            }
            else
            {
                itemToSpawn = GetRandomObstacleItem();
            }

            if (itemToSpawn != null)
            {
                GameObject obj = Instantiate(itemToSpawn.prefab, pos, Quaternion.identity);
                SetupObjectMovement(obj, itemToSpawn);
                SetupObjectProperties(obj, itemToSpawn);

                // 활성 오브젝트 리스트에 추가
                activeObjects.Add(new ActiveObject(obj));
            }

            yield return new WaitForSeconds(0.3f);
        }
    }

    // 보너스 타임 패턴
    IEnumerator BonusTimePattern()
    {
        Debug.Log("특수 패턴: 보너스 타임!");

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

    // 현재 난이도 스테이지 정보 가져오기
    public string GetCurrentStageInfo()
    {
        if (currentStage != null)
        {
            return $"{currentStage.stageName} (Game Time: {gameTime:F1}s)";
        }
        return "Default Stage";
    }
    
    // 현재 난이도 스테이지 객체 가져오기
    public DifficultyStage GetCurrentDifficulty()
    {
        return currentStage;
    }

    // 스폰 상태 정보 가져오기
    public string GetSpawnStats()
    {
        float successRate = successfulSpawns + failedSpawns > 0 ?
            (float)successfulSpawns / (successfulSpawns + failedSpawns) * 100f : 100f;

        return $"Spawns: {successfulSpawns} success, {failedSpawns} failed ({successRate:F1}% success rate)";
    }

    // 특별 아이템 제한 정보 가져오기
    public string GetSpecialItemInfo()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        foreach (var limit in specialItemLimits)
        {
            sb.AppendLine($"{limit.itemType}: {limit.GetRemainingCount()}/{limit.maxCountPerMinute} remaining");
        }
        return sb.ToString();
    }

    // 활성 오브젝트 수 가져오기
    public int GetActiveObjectCount()
    {
        return activeObjects.Count;
    }

    // 디버그 표시 (Scene 뷰에서)
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

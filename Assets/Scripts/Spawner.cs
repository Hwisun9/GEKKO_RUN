using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SpawnItem
{
    public GameObject prefab;
    public float spawnWeight = 1f; // 생성 확률 가중치
    public ItemType itemType = ItemType.Normal; // 아이템 타입 (기존 isItem 대신 사용)
    public float speedMultiplier = 1f; // 속도 배율
    public int scoreValue = 0; // 점수 값
    public Color effectColor = Color.white; // 이펙트 색상
}

// 아이템 타입 열거형 추가
public enum ItemType
{
    Normal,    // 일반 아이템
    Obstacle,  // 장애물
    Magnet     // 자석 아이템
}

[System.Serializable]
public class SpawnWave
{
    public string waveName;
    public float duration = 10f; // 웨이브 지속 시간
    public float spawnInterval = 2f;
    public float itemChance = 0.3f; // 아이템 생성 확률 (0~1)
    public float magnetChance = 0.05f; // 자석 아이템 생성 확률 추가
    public float speedMultiplier = 1f;
    public SpawnItem[] availableItems; // 이 웨이브에서 생성 가능한 아이템들
}

public class Spawner : MonoBehaviour
{
    [Header("기본 설정")]
    public SpawnItem[] allSpawnItems; // 모든 생성 가능한 아이템/장애물
    public float spawnRangeX = 2.5f;
    public float spawnPositionY = 6f;
    public float baseSpawnSpeed = 3f;

    [Header("웨이브 시스템")]
    public SpawnWave[] waves;
    public bool useWaveSystem = true;

    [Header("난이도 조절")]
    public float difficultyIncreaseRate = 0.1f; // 초당 난이도 증가율
    public float maxDifficultyMultiplier = 3f;

    [Header("특수 패턴")]
    public bool enableSpecialPatterns = true;
    public float specialPatternChance = 0.1f; // 특수 패턴 확률

    [Header("자석 아이템 설정")]
    public float magnetItemChance = 0.1f; // 자석 아이템 생성 확률 (10%)

    // 내부 변수들
    private float timer = 0f;
    private float gameTime = 0f;
    private int currentWaveIndex = 0;
    private float waveTimer = 0f;
    private float currentDifficultyMultiplier = 1f;
    private SpawnWave currentWave;

    // 특수 패턴 관련
    private bool isSpecialPatternActive = false;
    private float specialPatternTimer = 0f;

    void Start()
    {
        if (useWaveSystem && waves.Length > 0)
        {
            currentWave = waves[0];
        }

        // 자석 아이템 설정 확인
        CheckMagnetItemSetup();
    }

    void CheckMagnetItemSetup()
    {
        int magnetItems = 0;
        foreach (var item in allSpawnItems)
        {
            if (item.itemType == ItemType.Magnet)
            {
                magnetItems++;
                Debug.Log($" Magnet item found: {item.prefab.name}");
            }
        }

        if (magnetItems == 0)
        {
            Debug.LogWarning(" No magnet items found in allSpawnItems array!");
        }
        else
        {
            Debug.Log($" Found {magnetItems} magnet item(s) in spawner");
        }
    }

    void Update()
    {
        // GameManager가 없으면 실행하지 않음
        if (GameManager.Instance == null || !GameManager.Instance.isGameActive) return;

        UpdateGameTime();
        UpdateWaveSystem();
        UpdateDifficulty();
        UpdateSpawning();
    }

    void UpdateGameTime()
    {
        gameTime += Time.deltaTime;
    }

    void UpdateWaveSystem()
    {
        if (!useWaveSystem || waves.Length == 0) return;

        waveTimer += Time.deltaTime;

        // 현재 웨이브가 끝났는지 확인
        if (waveTimer >= currentWave.duration)
        {
            // 다음 웨이브로 전환
            currentWaveIndex = (currentWaveIndex + 1) % waves.Length;
            currentWave = waves[currentWaveIndex];
            waveTimer = 0f;

            // 웨이브 변경 알림 (UI에서 표시할 수 있음)
            Debug.Log($"웨이브 변경: {currentWave.waveName}");
        }
    }

    void UpdateDifficulty()
    {
        // 시간에 따른 난이도 증가
        currentDifficultyMultiplier = 1f + (gameTime * difficultyIncreaseRate);
        currentDifficultyMultiplier = Mathf.Min(currentDifficultyMultiplier, maxDifficultyMultiplier);
    }

    void UpdateSpawning()
    {
        timer += Time.deltaTime;

        float currentSpawnInterval = useWaveSystem ? currentWave.spawnInterval : 2f;
        currentSpawnInterval /= currentDifficultyMultiplier; // 난이도에 따라 생성 간격 단축

        if (timer >= currentSpawnInterval)
        {
            timer = 0f;

            // 특수 패턴 확인
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
        // 생성할 오브젝트 결정
        SpawnItem itemToSpawn = SelectSpawnItem();
        if (itemToSpawn == null) return;

        // 생성 위치 계산
        Vector3 spawnPos = CalculateSpawnPosition();

        // 오브젝트 생성
        GameObject obj = Instantiate(itemToSpawn.prefab, spawnPos, Quaternion.identity);

        // 이동 설정
        SetupObjectMovement(obj, itemToSpawn);

        // 태그 및 추가 설정
        SetupObjectProperties(obj, itemToSpawn);

        // 디버그 로그
        Debug.Log($"Spawned: {obj.name} (Type: {itemToSpawn.itemType}, Tag: {obj.tag})");
    }

    SpawnItem SelectSpawnItem()
    {
        List<SpawnItem> availableItems;

        if (useWaveSystem && currentWave.availableItems.Length > 0)
        {
            availableItems = new List<SpawnItem>(currentWave.availableItems);
        }
        else
        {
            availableItems = new List<SpawnItem>(allSpawnItems);
        }

        //  자석 아이템 생성 확률 체크 (우선순위)
        float magnetChance = useWaveSystem ? currentWave.magnetChance : magnetItemChance;
        if (Random.value < magnetChance)
        {
            // 자석 아이템만 필터링
            var magnetItems = availableItems.FindAll(item => item.itemType == ItemType.Magnet);
            if (magnetItems.Count > 0)
            {
                Debug.Log(" Spawning magnet item!");
                return SelectByWeight(magnetItems);
            }
            else
            {
                Debug.LogWarning("No magnet items available in current item list!");
            }
        }

        // 일반 아이템 vs 장애물 결정
        float itemChance = useWaveSystem ? currentWave.itemChance : 0.3f;
        bool shouldSpawnItem = Random.value < itemChance;

        // 자석 아이템 제외하고 필터링
        availableItems.RemoveAll(item => item.itemType == ItemType.Magnet);

        if (shouldSpawnItem)
        {
            // 일반 아이템만
            availableItems.RemoveAll(item => item.itemType != ItemType.Normal);
        }
        else
        {
            // 장애물만  
            availableItems.RemoveAll(item => item.itemType != ItemType.Obstacle);
        }

        if (availableItems.Count == 0)
        {
            Debug.LogWarning("No items available after filtering!");
            return null;
        }

        // 가중치 기반 선택
        return SelectByWeight(availableItems);
    }

    SpawnItem SelectByWeight(List<SpawnItem> items)
    {
        float totalWeight = 0f;
        foreach (var item in items)
        {
            totalWeight += item.spawnWeight;
        }

        float randomValue = Random.value * totalWeight;
        float currentWeight = 0f;

        foreach (var item in items)
        {
            currentWeight += item.spawnWeight;
            if (randomValue <= currentWeight)
            {
                return item;
            }
        }

        return items[items.Count - 1]; // 안전장치
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

        // 속도 계산 (기본 속도 × 아이템 배율 × 웨이브 배율 × 난이도 배율)
        float finalSpeed = baseSpawnSpeed;
        finalSpeed *= spawnItem.speedMultiplier;

        if (useWaveSystem)
        {
            finalSpeed *= currentWave.speedMultiplier;
        }

        finalSpeed *= currentDifficultyMultiplier;

        rb.linearVelocity = Vector2.down * finalSpeed;

        // 이동 스크립트 추가 (경계 체크 및 자동 삭제용)
        MovingObject movingComponent = obj.AddComponent<MovingObject>();
        movingComponent.speed = finalSpeed;
    }

    void SetupObjectProperties(GameObject obj, SpawnItem spawnItem)
    {
        //  태그 설정 - ItemType에 따라 정확히 설정
        string tagToSet = "";

        switch (spawnItem.itemType)
        {
            case ItemType.Normal:
                tagToSet = "Item";
                break;
            case ItemType.Obstacle:
                tagToSet = "Obstacle";
                break;
            case ItemType.Magnet:
                tagToSet = "Magnet";  // 사용자가 설정한 태그명 사용
                break;
            default:
                tagToSet = "Item";
                Debug.LogWarning($"Unknown ItemType: {spawnItem.itemType}");
                break;
        }

        obj.tag = tagToSet;
        Debug.Log($" Set tag '{tagToSet}' for {obj.name} (ItemType: {spawnItem.itemType})");

        // 점수 값 설정
        ScoreItem scoreComponent = obj.GetComponent<ScoreItem>();
        if (scoreComponent == null)
        {
            scoreComponent = obj.AddComponent<ScoreItem>();
        }
        scoreComponent.scoreValue = spawnItem.scoreValue;
        scoreComponent.effectColor = spawnItem.effectColor;

        // 충돌 감지 설정
        Collider2D collider = obj.GetComponent<Collider2D>();
        if (collider == null)
        {
            collider = obj.AddComponent<BoxCollider2D>();
        }
        collider.isTrigger = true;
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
            // 일반 아이템만 생성
            var itemsOnly = System.Array.FindAll(allSpawnItems, item => item.itemType == ItemType.Normal);
            if (itemsOnly.Length > 0)
            {
                var randomItem = itemsOnly[Random.Range(0, itemsOnly.Length)];
                Vector3 pos = new Vector3(Random.Range(-spawnRangeX, spawnRangeX), spawnPositionY, 0);
                GameObject obj = Instantiate(randomItem.prefab, pos, Quaternion.identity);
                SetupObjectMovement(obj, randomItem);
                SetupObjectProperties(obj, randomItem);
            }

            yield return new WaitForSeconds(0.2f);
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

            var randomItem = allSpawnItems[Random.Range(0, allSpawnItems.Length)];
            GameObject obj = Instantiate(randomItem.prefab, pos, Quaternion.identity);
            SetupObjectMovement(obj, randomItem);
            SetupObjectProperties(obj, randomItem);

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

            var randomItem = allSpawnItems[Random.Range(0, allSpawnItems.Length)];
            GameObject obj = Instantiate(randomItem.prefab, pos, Quaternion.identity);
            SetupObjectMovement(obj, randomItem);
            SetupObjectProperties(obj, randomItem);

            yield return new WaitForSeconds(0.3f);
        }
    }

    // 보너스 타임 패턴
    IEnumerator BonusTimePattern()
    {
        Debug.Log("특수 패턴: 보너스 타임!");

        // 고점수 일반 아이템들만 생성
        var highValueItems = System.Array.FindAll(allSpawnItems,
            item => item.itemType == ItemType.Normal && item.scoreValue > 15);

        if (highValueItems.Length > 0)
        {
            for (int i = 0; i < 4; i++)
            {
                var bonusItem = highValueItems[Random.Range(0, highValueItems.Length)];
                Vector3 pos = new Vector3(Random.Range(-spawnRangeX, spawnRangeX), spawnPositionY, 0);
                GameObject obj = Instantiate(bonusItem.prefab, pos, Quaternion.identity);
                SetupObjectMovement(obj, bonusItem);
                SetupObjectProperties(obj, bonusItem);

                yield return new WaitForSeconds(0.5f);
            }
        }
    }

    // 현재 상태 정보 반환 (UI에서 사용 가능)
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
}

// 이동하는 오브젝트를 위한 컴포넌트
public class MovingObject : MonoBehaviour
{
    public float speed = 3f;
    public float destroyBoundary = -7f;

    void Update()
    {
        // 아래로 이동
        transform.Translate(Vector3.down * speed * Time.deltaTime);

        // 화면 밖으로 나가면 삭제
        if (transform.position.y < destroyBoundary)
        {
            Destroy(gameObject);
        }
    }
}

// 점수 아이템을 위한 컴포넌트 - 수정된 버전
public class ScoreItem : MonoBehaviour
{
    public int scoreValue = 10;
    public Color effectColor = Color.white;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // 디버그 로그 추가
            Debug.Log($" Player collided with: {gameObject.name}, Tag: {gameObject.tag}");

            // GameManager가 존재하는지 확인
            if (GameManager.Instance != null)
            {
                // 아이템 타입별 처리
                if (gameObject.CompareTag("Item"))
                {
                    Debug.Log(" Normal item collected!");

                    // 콤보 시스템 처리
                    if (ComboSystem.Instance != null)
                    {
                        ComboSystem.Instance.OnItemCollected();
                    }

                    // 점수 및 아이템 수집 처리
                    GameManager.Instance.AddScore(scoreValue);
                    GameManager.Instance.AddCollectedItem(); // 사운드 재생 포함
                }
                else if (gameObject.CompareTag("Obstacle"))
                {
                    Debug.Log(" Obstacle hit!");
                        
                }
                else if (gameObject.CompareTag("Magnet"))  // 태그명 수정: MagnetItem → Magnet
                {
                    Debug.Log(" Magnet item collected!");

                    // 자석 효과 활성화
                    GameManager.Instance.ActivateMagnet();

                    // 자석 아이템도 점수 제공
                    GameManager.Instance.AddScore(scoreValue);
                    GameManager.Instance.AddCollectedItem(); // 사운드 재생
                }
                else
                {
                    Debug.LogWarning($" Unknown item tag: {gameObject.tag}");
                }
            }
            else
            {
                Debug.LogError(" GameManager.Instance is null!");
            }

            // 이펙트 생성
            CreateEffect();

            // 오브젝트 삭제
            Destroy(gameObject);
        }
    }

    void CreateEffect()
    {
        // 간단한 이펙트 생성
        GameObject effect = new GameObject("CollectionEffect");
        effect.transform.position = transform.position;

        // 아이템 타입에 따른 이펙트 색상 설정
        if (gameObject.CompareTag("Magnet"))  // 태그명 수정
        {
            // 자석 아이템은 특별한 이펙트
            CreateMagnetEffect(effect);
        }
        else
        {
            // 일반 아이템 이펙트
            CreateNormalEffect(effect);
        }

        // 이펙트 자동 삭제
        Destroy(effect, 1f);
    }

    void CreateNormalEffect(GameObject effect)
    {
        // 일반 아이템 수집 이펙트
        StartCoroutine(ScaleEffect(effect.transform));
    }

    void CreateMagnetEffect(GameObject effect)
    {
        // 자석 아이템 특별 이펙트
        Debug.Log(" Creating magnet collection effect!");
        StartCoroutine(MagnetScaleEffect(effect.transform));
    }

    System.Collections.IEnumerator ScaleEffect(Transform effectTransform)
    {
        Vector3 startScale = Vector3.zero;
        Vector3 endScale = Vector3.one * 1.5f;
        float duration = 0.3f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            effectTransform.localScale = Vector3.Lerp(startScale, endScale, t);
            yield return null;
        }

        // 페이드 아웃
        while (elapsed < duration * 2)
        {
            elapsed += Time.deltaTime;
            float t = (elapsed - duration) / duration;
            effectTransform.localScale = Vector3.Lerp(endScale, Vector3.zero, t);
            yield return null;
        }
    }

    System.Collections.IEnumerator MagnetScaleEffect(Transform effectTransform)
    {
        Vector3 startScale = Vector3.zero;
        Vector3 endScale = Vector3.one * 2f; // 자석은 더 크게
        float duration = 0.5f; // 자석은 더 오래
        float elapsed = 0f;

        // 확대
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            effectTransform.localScale = Vector3.Lerp(startScale, endScale, t);

            // 회전 효과 추가
            effectTransform.Rotate(0, 0, 720 * Time.deltaTime);
            yield return null;
        }

        // 페이드 아웃
        while (elapsed < duration * 2)
        {
            elapsed += Time.deltaTime;
            float t = (elapsed - duration) / duration;
            effectTransform.localScale = Vector3.Lerp(endScale, Vector3.zero, t);
            effectTransform.Rotate(0, 0, 360 * Time.deltaTime);
            yield return null;
        }
    }
}
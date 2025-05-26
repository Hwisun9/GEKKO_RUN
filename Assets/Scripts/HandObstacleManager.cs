using System.Collections;
using UnityEngine;

public class HandObstacleManager : MonoBehaviour
{
    [Header("손 설정")]
    public GameObject leftHandPrefab;
    public GameObject rightHandPrefab;
    public GameObject warningIconPrefab;

    [Header("위치 설정")]
    public float handBottomY = -4f;
    public float handTopY = -1f;
    public float leftHandX = -2.8f;
    public float rightHandX = 2.8f;
    public float[] grabPositionsX = { -2f, -1f, 0f, 1f, 2f };

    [Header("타이밍 설정")]
    public float minGrabInterval = 3f;
    public float maxGrabInterval = 6f;
    public float warningDuration = 1.5f;
    public float handGrabDuration = 1f;
    public float handMoveSpeed = 5f;

    [Header("게임 진행에 따른 조절")]
    public bool adjustWithDifficulty = true;
    public float minIntervalAtMaxDifficulty = 1.5f;

    // 내부 변수
    private GameObject currentLeftHand;
    private GameObject currentRightHand;
    private Vector3 leftHandOriginalPos;
    private Vector3 rightHandOriginalPos;
    private bool isGrabbing = false;
    private Coroutine grabCoroutine;
    private int grabCount = 0; // 잡기 횟수 카운트

    void Start()
    {
        Debug.Log("HandObstacleManager Start() 호출");

        // 원래 위치 설정
        leftHandOriginalPos = new Vector3(leftHandX, handBottomY, 0);
        rightHandOriginalPos = new Vector3(rightHandX, handBottomY, 0);

        // 양손 생성
        CreateHands();

        // 2초 후에 시작
        Invoke("DelayedStart", 2f);
    }

    void DelayedStart()
    {
        Debug.Log("DelayedStart 호출");

        // GameManager 확인
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager.Instance가 여전히 null입니다!");
            return;
        }

        if (!GameManager.Instance.isGameActive)
        {
            Debug.LogWarning("게임이 비활성 상태입니다.");
        }

        StartGrabSequence();
    }

    void CreateHands()
    {
        Debug.Log("CreateHands 호출");

        // 기존 손들 제거
        if (currentLeftHand != null) Destroy(currentLeftHand);
        if (currentRightHand != null) Destroy(currentRightHand);

        // 왼손 생성
        if (leftHandPrefab != null)
        {
            currentLeftHand = Instantiate(leftHandPrefab, leftHandOriginalPos, Quaternion.identity);
            currentLeftHand.name = "LeftHand";
            SetupHand(currentLeftHand);
            Debug.Log($"왼손 생성 완료 - 위치: {leftHandOriginalPos}");
        }
        else
        {
            Debug.LogError("leftHandPrefab이 null입니다!");
        }

        // 오른손 생성
        if (rightHandPrefab != null)
        {
            currentRightHand = Instantiate(rightHandPrefab, rightHandOriginalPos, Quaternion.identity);
            currentRightHand.name = "RightHand";
            SetupHand(currentRightHand);
            Debug.Log($"오른손 생성 완료 - 위치: {rightHandOriginalPos}");
        }
        else
        {
            Debug.LogError("rightHandPrefab이 null입니다!");
        }
    }

    void SetupHand(GameObject hand)
    {
        if (hand == null) return;

        // 충돌체 설정
        Collider2D collider = hand.GetComponent<Collider2D>();
        if (collider == null)
        {
            collider = hand.AddComponent<BoxCollider2D>();
        }
        collider.isTrigger = true;

        // BoxCollider2D 크기 설정
        if (collider is BoxCollider2D boxCollider)
        {
            boxCollider.size = new Vector2(1.2f, 1.2f);
        }

        // HandGrabCollider 컴포넌트 추가
        HandGrabCollider grabCollider = hand.GetComponent<HandGrabCollider>();
        if (grabCollider == null)
        {
            grabCollider = hand.AddComponent<HandGrabCollider>();
        }

        Debug.Log($"{hand.name} 설정 완료");
    }

    void StartGrabSequence()
    {
        Debug.Log("StartGrabSequence 호출");

        if (grabCoroutine != null)
        {
            StopCoroutine(grabCoroutine);
        }

        grabCoroutine = StartCoroutine(GrabSequenceRoutine());
    }

    IEnumerator GrabSequenceRoutine()
    {
        Debug.Log("GrabSequenceRoutine 시작!");

        while (true) // 무한 루프로 변경
        {
            // GameManager 체크
            if (GameManager.Instance == null || !GameManager.Instance.isGameActive)
            {
                Debug.Log("게임이 비활성 상태이므로 잠시 대기...");
                yield return new WaitForSeconds(1f);
                continue;
            }

            Debug.Log("잡기 시퀀스 실행 중...");

            // 다음 잡기까지 대기 시간 계산
            float waitTime = CalculateGrabInterval();
            Debug.Log($"다음 잡기까지 대기 시간: {waitTime}초");

            yield return new WaitForSeconds(waitTime);

            Debug.Log("잡기 실행 시작!");
            yield return StartCoroutine(ExecuteGrab());
        }
    }

    float CalculateGrabInterval()
    {
        // 더 랜덤한 간격을 위해 가중치 적용
        float[] intervals = { 1.5f, 2.0f, 2.5f, 3.0f, 3.5f, 4.0f, 5.0f, 6.0f };
        float[] weights = { 0.05f, 0.1f, 0.15f, 0.25f, 0.2f, 0.15f, 0.07f, 0.03f }; // 2.5~3.5초가 가장 높은 확률

        float totalWeight = 0f;
        foreach (float weight in weights) totalWeight += weight;

        float randomValue = Random.value * totalWeight;
        float currentWeight = 0f;

        for (int i = 0; i < intervals.Length; i++)
        {
            currentWeight += weights[i];
            if (randomValue <= currentWeight)
            {
                return intervals[i];
            }
        }

        return Random.Range(minGrabInterval, maxGrabInterval);
    }

    IEnumerator ExecuteGrab()
    {
        if (isGrabbing)
        {
            Debug.Log("이미 잡기 중이므로 스킵");
            yield break;
        }

        isGrabbing = true;
        grabCount++;
        Debug.Log($"=== 잡기 시작 (#{grabCount}) ===");

        // 7번 중 1번은 양손 동시 잡기
        bool isDoubleGrab = (grabCount % 5 == 0);

        if (isDoubleGrab)
        {
            Debug.Log("양손 동시 잡기 패턴!");
            yield return StartCoroutine(ExecuteDoubleGrab());
        }
        else
        {
            Debug.Log("단일 손 잡기 패턴");
            yield return StartCoroutine(ExecuteSingleGrab());
        }

        isGrabbing = false;
        Debug.Log("=== 잡기 완료 ===");
    }

    IEnumerator ExecuteSingleGrab()
    {
        // 1. 잡을 위치 선택 (더 랜덤하게)
        float grabX = SelectRandomGrabPosition();
        Vector3 grabPosition = new Vector3(grabX, handTopY, 0);
        Debug.Log($"단일 잡기 위치 선택: {grabPosition}");

        // 2. 어느 손을 사용할지 결정
        GameObject handToUse;
        Vector3 originalPosition;

        if (grabX < 0)
        {
            handToUse = currentLeftHand;
            originalPosition = leftHandOriginalPos;
            Debug.Log("왼손 사용 결정");
        }
        else
        {
            handToUse = currentRightHand;
            originalPosition = rightHandOriginalPos;
            Debug.Log("오른손 사용 결정");
        }

        if (handToUse == null)
        {
            Debug.LogError("사용할 손이 null입니다!");
            yield break;
        }

        // 3. 경고 표시
        GameObject warningIcon = null;
        if (warningIconPrefab != null)
        {
            warningIcon = Instantiate(warningIconPrefab);
            warningIcon.transform.position = new Vector3(grabX, handTopY + 0.5f, 0);
            Debug.Log("경고 아이콘 생성");
        }

        // 4. 경고 시간 대기
        yield return new WaitForSeconds(warningDuration);

        // 5. 경고 아이콘 제거
        if (warningIcon != null)
        {
            Destroy(warningIcon);
            Debug.Log("경고 아이콘 제거");
        }

        // 6. 손을 잡는 위치로 이동
        Debug.Log($"손 이동 시작: {handToUse.transform.position} -> {grabPosition}");
        yield return StartCoroutine(MoveHandToPosition(handToUse, grabPosition));
        Debug.Log("손 이동 완료");

        // 7. 잡기 상태 활성화
        HandGrabCollider grabCollider = handToUse.GetComponent<HandGrabCollider>();
        if (grabCollider != null)
        {
            grabCollider.EnableGrabbing(true);
            Debug.Log("잡기 상태 활성화");
        }

        // 8. 잡기 시간 대기
        yield return new WaitForSeconds(handGrabDuration);

        // 9. 잡기 상태 비활성화
        if (grabCollider != null)
        {
            grabCollider.EnableGrabbing(false);
            Debug.Log("잡기 상태 비활성화");
        }

        // 10. 손을 원래 위치로 복귀
        Debug.Log($"손 복귀 시작: {handToUse.transform.position} -> {originalPosition}");
        yield return StartCoroutine(MoveHandToPosition(handToUse, originalPosition));
        Debug.Log("손 복귀 완료");
    }

    IEnumerator ExecuteDoubleGrab()
    {
        // 1. 양손 잡기 위치 선택
        float leftGrabX = SelectRandomGrabPosition(true); // 왼쪽용
        float rightGrabX = SelectRandomGrabPosition(false); // 오른쪽용

        Vector3 leftGrabPosition = new Vector3(leftGrabX, handTopY, 0);
        Vector3 rightGrabPosition = new Vector3(rightGrabX, handTopY, 0);

        Debug.Log($"양손 잡기 위치 - 왼손: {leftGrabPosition}, 오른손: {rightGrabPosition}");

        // 2. 양쪽 경고 표시
        GameObject leftWarning = null;
        GameObject rightWarning = null;

        if (warningIconPrefab != null)
        {
            leftWarning = Instantiate(warningIconPrefab);
            leftWarning.transform.position = new Vector3(leftGrabX, handTopY + 0.5f, 0);

            rightWarning = Instantiate(warningIconPrefab);
            rightWarning.transform.position = new Vector3(rightGrabX, handTopY + 0.5f, 0);

            Debug.Log("양쪽 경고 아이콘 생성");
        }

        // 3. 경고 시간 대기 (양손일 때는 조금 더 길게)
        yield return new WaitForSeconds(warningDuration + 0.5f);

        // 4. 경고 아이콘 제거
        if (leftWarning != null) Destroy(leftWarning);
        if (rightWarning != null) Destroy(rightWarning);
        Debug.Log("양쪽 경고 아이콘 제거");

        // 5. 양손 동시 이동 시작
        Coroutine leftMove = StartCoroutine(MoveHandToPosition(currentLeftHand, leftGrabPosition));
        Coroutine rightMove = StartCoroutine(MoveHandToPosition(currentRightHand, rightGrabPosition));

        // 6. 양손 이동 완료 대기
        yield return leftMove;
        yield return rightMove;
        Debug.Log("양손 이동 완료");

        // 7. 양손 잡기 상태 활성화
        HandGrabCollider leftGrabber = currentLeftHand.GetComponent<HandGrabCollider>();
        HandGrabCollider rightGrabber = currentRightHand.GetComponent<HandGrabCollider>();

        if (leftGrabber != null) leftGrabber.EnableGrabbing(true);
        if (rightGrabber != null) rightGrabber.EnableGrabbing(true);
        Debug.Log("양손 잡기 상태 활성화");

        // 8. 잡기 시간 대기 (양손일 때는 조금 더 길게)
        yield return new WaitForSeconds(handGrabDuration + 0.3f);

        // 9. 양손 잡기 상태 비활성화
        if (leftGrabber != null) leftGrabber.EnableGrabbing(false);
        if (rightGrabber != null) rightGrabber.EnableGrabbing(false);
        Debug.Log("양손 잡기 상태 비활성화");

        // 10. 양손 동시 복귀
        Coroutine leftReturn = StartCoroutine(MoveHandToPosition(currentLeftHand, leftHandOriginalPos));
        Coroutine rightReturn = StartCoroutine(MoveHandToPosition(currentRightHand, rightHandOriginalPos));

        yield return leftReturn;
        yield return rightReturn;
        Debug.Log("양손 복귀 완료");
    }

    float SelectRandomGrabPosition(bool? forLeftHand = null)
    {
        if (forLeftHand == true)
        {
            // 왼손용 - 왼쪽과 중앙 위치만
            float[] leftPositions = { -2f, -1f, 0f };
            return leftPositions[Random.Range(0, leftPositions.Length)];
        }
        else if (forLeftHand == false)
        {
            // 오른손용 - 오른쪽과 중앙 위치만
            float[] rightPositions = { 0f, 1f, 2f };
            return rightPositions[Random.Range(0, rightPositions.Length)];
        }
        else
        {
            // 단일 잡기용 - 모든 위치, 가중치 적용
            float[] positions = { -2f, -1f, 0f, 1f, 2f };
            float[] weights = { 0.2f, 0.25f, 0.1f, 0.25f, 0.2f }; // 중앙은 확률 낮게

            float totalWeight = 0f;
            foreach (float weight in weights) totalWeight += weight;

            float randomValue = Random.value * totalWeight;
            float currentWeight = 0f;

            for (int i = 0; i < positions.Length; i++)
            {
                currentWeight += weights[i];
                if (randomValue <= currentWeight)
                {
                    return positions[i];
                }
            }

            return grabPositionsX[Random.Range(0, grabPositionsX.Length)];
        }
    }

    IEnumerator MoveHandToPosition(GameObject hand, Vector3 targetPosition)
    {
        if (hand == null)
        {
            Debug.LogError("이동할 손이 null입니다!");
            yield break;
        }

        Vector3 startPosition = hand.transform.position;
        float distance = Vector3.Distance(startPosition, targetPosition);
        float moveTime = distance / handMoveSpeed;
        float elapsed = 0f;

        Debug.Log($"손 이동 시작: {startPosition} -> {targetPosition}, 거리: {distance:F2}, 예상 시간: {moveTime:F2}초");

        while (elapsed < moveTime)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / moveTime;
            if (progress > 1f) progress = 1f;

            Vector3 currentPos = Vector3.Lerp(startPosition, targetPosition, progress);
            hand.transform.position = currentPos;

            yield return null;
        }

        // 최종 위치 확실히 설정
        hand.transform.position = targetPosition;
        Debug.Log($"손 이동 완료: 최종 위치 {hand.transform.position}");
    }

    // 테스트 메서드들
    [ContextMenu("Test Grab")]
    public void TestGrab()
    {
        if (!isGrabbing)
        {
            Debug.Log("수동 잡기 테스트 실행");
            StartCoroutine(ExecuteGrab());
        }
        else
        {
            Debug.Log("이미 잡기 중입니다");
        }
    }

    [ContextMenu("Reset Hand Positions")]
    public void ResetHandPositions()
    {
        if (currentLeftHand != null)
        {
            currentLeftHand.transform.position = leftHandOriginalPos;
            Debug.Log($"왼손 위치 리셋: {leftHandOriginalPos}");
        }
        if (currentRightHand != null)
        {
            currentRightHand.transform.position = rightHandOriginalPos;
            Debug.Log($"오른손 위치 리셋: {rightHandOriginalPos}");
        }
    }

    void OnDisable()
    {
        if (grabCoroutine != null)
        {
            StopCoroutine(grabCoroutine);
        }
    }
}
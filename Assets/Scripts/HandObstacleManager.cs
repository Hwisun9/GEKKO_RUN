using System.Collections;
using UnityEngine;

public class HandObstacleManager : MonoBehaviour
{
    [Header("�� ����")]
    public GameObject leftHandPrefab;
    public GameObject rightHandPrefab;
    public GameObject warningIconPrefab;

    [Header("��ġ ����")]
    public float handBottomY = -4f;
    public float handTopY = -1f;
    public float leftHandX = -2.8f;
    public float rightHandX = 2.8f;
    public float[] grabPositionsX = { -2f, -1f, 0f, 1f, 2f };

    [Header("Ÿ�̹� ����")]
    public float minGrabInterval = 3f;
    public float maxGrabInterval = 6f;
    public float warningDuration = 1.5f;
    public float handGrabDuration = 1f;
    public float handMoveSpeed = 5f;

    [Header("���� ���࿡ ���� ����")]
    public bool adjustWithDifficulty = true;
    public float minIntervalAtMaxDifficulty = 1.5f;

    // ���� ����
    private GameObject currentLeftHand;
    private GameObject currentRightHand;
    private Vector3 leftHandOriginalPos;
    private Vector3 rightHandOriginalPos;
    private bool isGrabbing = false;
    private Coroutine grabCoroutine;
    private int grabCount = 0; // ��� Ƚ�� ī��Ʈ

    void Start()
    {
        Debug.Log("HandObstacleManager Start() ȣ��");

        // ���� ��ġ ����
        leftHandOriginalPos = new Vector3(leftHandX, handBottomY, 0);
        rightHandOriginalPos = new Vector3(rightHandX, handBottomY, 0);

        // ��� ����
        CreateHands();

        // 2�� �Ŀ� ����
        Invoke("DelayedStart", 2f);
    }

    void DelayedStart()
    {
        Debug.Log("DelayedStart ȣ��");

        // GameManager Ȯ��
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager.Instance�� ������ null�Դϴ�!");
            return;
        }

        if (!GameManager.Instance.isGameActive)
        {
            Debug.LogWarning("������ ��Ȱ�� �����Դϴ�.");
        }

        StartGrabSequence();
    }

    void CreateHands()
    {
        Debug.Log("CreateHands ȣ��");

        // ���� �յ� ����
        if (currentLeftHand != null) Destroy(currentLeftHand);
        if (currentRightHand != null) Destroy(currentRightHand);

        // �޼� ����
        if (leftHandPrefab != null)
        {
            currentLeftHand = Instantiate(leftHandPrefab, leftHandOriginalPos, Quaternion.identity);
            currentLeftHand.name = "LeftHand";
            SetupHand(currentLeftHand);
            Debug.Log($"�޼� ���� �Ϸ� - ��ġ: {leftHandOriginalPos}");
        }
        else
        {
            Debug.LogError("leftHandPrefab�� null�Դϴ�!");
        }

        // ������ ����
        if (rightHandPrefab != null)
        {
            currentRightHand = Instantiate(rightHandPrefab, rightHandOriginalPos, Quaternion.identity);
            currentRightHand.name = "RightHand";
            SetupHand(currentRightHand);
            Debug.Log($"������ ���� �Ϸ� - ��ġ: {rightHandOriginalPos}");
        }
        else
        {
            Debug.LogError("rightHandPrefab�� null�Դϴ�!");
        }
    }

    void SetupHand(GameObject hand)
    {
        if (hand == null) return;

        // �浹ü ����
        Collider2D collider = hand.GetComponent<Collider2D>();
        if (collider == null)
        {
            collider = hand.AddComponent<BoxCollider2D>();
        }
        collider.isTrigger = true;

        // BoxCollider2D ũ�� ����
        if (collider is BoxCollider2D boxCollider)
        {
            boxCollider.size = new Vector2(1.2f, 1.2f);
        }

        // HandGrabCollider ������Ʈ �߰�
        HandGrabCollider grabCollider = hand.GetComponent<HandGrabCollider>();
        if (grabCollider == null)
        {
            grabCollider = hand.AddComponent<HandGrabCollider>();
        }

        Debug.Log($"{hand.name} ���� �Ϸ�");
    }

    void StartGrabSequence()
    {
        Debug.Log("StartGrabSequence ȣ��");

        if (grabCoroutine != null)
        {
            StopCoroutine(grabCoroutine);
        }

        grabCoroutine = StartCoroutine(GrabSequenceRoutine());
    }

    IEnumerator GrabSequenceRoutine()
    {
        Debug.Log("GrabSequenceRoutine ����!");

        while (true) // ���� ������ ����
        {
            // GameManager üũ
            if (GameManager.Instance == null || !GameManager.Instance.isGameActive)
            {
                Debug.Log("������ ��Ȱ�� �����̹Ƿ� ��� ���...");
                yield return new WaitForSeconds(1f);
                continue;
            }

            Debug.Log("��� ������ ���� ��...");

            // ���� ������ ��� �ð� ���
            float waitTime = CalculateGrabInterval();
            Debug.Log($"���� ������ ��� �ð�: {waitTime}��");

            yield return new WaitForSeconds(waitTime);

            Debug.Log("��� ���� ����!");
            yield return StartCoroutine(ExecuteGrab());
        }
    }

    float CalculateGrabInterval()
    {
        // �� ������ ������ ���� ����ġ ����
        float[] intervals = { 1.5f, 2.0f, 2.5f, 3.0f, 3.5f, 4.0f, 5.0f, 6.0f };
        float[] weights = { 0.05f, 0.1f, 0.15f, 0.25f, 0.2f, 0.15f, 0.07f, 0.03f }; // 2.5~3.5�ʰ� ���� ���� Ȯ��

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
            Debug.Log("�̹� ��� ���̹Ƿ� ��ŵ");
            yield break;
        }

        isGrabbing = true;
        grabCount++;
        Debug.Log($"=== ��� ���� (#{grabCount}) ===");

        // 7�� �� 1���� ��� ���� ���
        bool isDoubleGrab = (grabCount % 5 == 0);

        if (isDoubleGrab)
        {
            Debug.Log("��� ���� ��� ����!");
            yield return StartCoroutine(ExecuteDoubleGrab());
        }
        else
        {
            Debug.Log("���� �� ��� ����");
            yield return StartCoroutine(ExecuteSingleGrab());
        }

        isGrabbing = false;
        Debug.Log("=== ��� �Ϸ� ===");
    }

    IEnumerator ExecuteSingleGrab()
    {
        // 1. ���� ��ġ ���� (�� �����ϰ�)
        float grabX = SelectRandomGrabPosition();
        Vector3 grabPosition = new Vector3(grabX, handTopY, 0);
        Debug.Log($"���� ��� ��ġ ����: {grabPosition}");

        // 2. ��� ���� ������� ����
        GameObject handToUse;
        Vector3 originalPosition;

        if (grabX < 0)
        {
            handToUse = currentLeftHand;
            originalPosition = leftHandOriginalPos;
            Debug.Log("�޼� ��� ����");
        }
        else
        {
            handToUse = currentRightHand;
            originalPosition = rightHandOriginalPos;
            Debug.Log("������ ��� ����");
        }

        if (handToUse == null)
        {
            Debug.LogError("����� ���� null�Դϴ�!");
            yield break;
        }

        // 3. ��� ǥ��
        GameObject warningIcon = null;
        if (warningIconPrefab != null)
        {
            warningIcon = Instantiate(warningIconPrefab);
            warningIcon.transform.position = new Vector3(grabX, handTopY + 0.5f, 0);
            Debug.Log("��� ������ ����");
        }

        // 4. ��� �ð� ���
        yield return new WaitForSeconds(warningDuration);

        // 5. ��� ������ ����
        if (warningIcon != null)
        {
            Destroy(warningIcon);
            Debug.Log("��� ������ ����");
        }

        // 6. ���� ��� ��ġ�� �̵�
        Debug.Log($"�� �̵� ����: {handToUse.transform.position} -> {grabPosition}");
        yield return StartCoroutine(MoveHandToPosition(handToUse, grabPosition));
        Debug.Log("�� �̵� �Ϸ�");

        // 7. ��� ���� Ȱ��ȭ
        HandGrabCollider grabCollider = handToUse.GetComponent<HandGrabCollider>();
        if (grabCollider != null)
        {
            grabCollider.EnableGrabbing(true);
            Debug.Log("��� ���� Ȱ��ȭ");
        }

        // 8. ��� �ð� ���
        yield return new WaitForSeconds(handGrabDuration);

        // 9. ��� ���� ��Ȱ��ȭ
        if (grabCollider != null)
        {
            grabCollider.EnableGrabbing(false);
            Debug.Log("��� ���� ��Ȱ��ȭ");
        }

        // 10. ���� ���� ��ġ�� ����
        Debug.Log($"�� ���� ����: {handToUse.transform.position} -> {originalPosition}");
        yield return StartCoroutine(MoveHandToPosition(handToUse, originalPosition));
        Debug.Log("�� ���� �Ϸ�");
    }

    IEnumerator ExecuteDoubleGrab()
    {
        // 1. ��� ��� ��ġ ����
        float leftGrabX = SelectRandomGrabPosition(true); // ���ʿ�
        float rightGrabX = SelectRandomGrabPosition(false); // �����ʿ�

        Vector3 leftGrabPosition = new Vector3(leftGrabX, handTopY, 0);
        Vector3 rightGrabPosition = new Vector3(rightGrabX, handTopY, 0);

        Debug.Log($"��� ��� ��ġ - �޼�: {leftGrabPosition}, ������: {rightGrabPosition}");

        // 2. ���� ��� ǥ��
        GameObject leftWarning = null;
        GameObject rightWarning = null;

        if (warningIconPrefab != null)
        {
            leftWarning = Instantiate(warningIconPrefab);
            leftWarning.transform.position = new Vector3(leftGrabX, handTopY + 0.5f, 0);

            rightWarning = Instantiate(warningIconPrefab);
            rightWarning.transform.position = new Vector3(rightGrabX, handTopY + 0.5f, 0);

            Debug.Log("���� ��� ������ ����");
        }

        // 3. ��� �ð� ��� (����� ���� ���� �� ���)
        yield return new WaitForSeconds(warningDuration + 0.5f);

        // 4. ��� ������ ����
        if (leftWarning != null) Destroy(leftWarning);
        if (rightWarning != null) Destroy(rightWarning);
        Debug.Log("���� ��� ������ ����");

        // 5. ��� ���� �̵� ����
        Coroutine leftMove = StartCoroutine(MoveHandToPosition(currentLeftHand, leftGrabPosition));
        Coroutine rightMove = StartCoroutine(MoveHandToPosition(currentRightHand, rightGrabPosition));

        // 6. ��� �̵� �Ϸ� ���
        yield return leftMove;
        yield return rightMove;
        Debug.Log("��� �̵� �Ϸ�");

        // 7. ��� ��� ���� Ȱ��ȭ
        HandGrabCollider leftGrabber = currentLeftHand.GetComponent<HandGrabCollider>();
        HandGrabCollider rightGrabber = currentRightHand.GetComponent<HandGrabCollider>();

        if (leftGrabber != null) leftGrabber.EnableGrabbing(true);
        if (rightGrabber != null) rightGrabber.EnableGrabbing(true);
        Debug.Log("��� ��� ���� Ȱ��ȭ");

        // 8. ��� �ð� ��� (����� ���� ���� �� ���)
        yield return new WaitForSeconds(handGrabDuration + 0.3f);

        // 9. ��� ��� ���� ��Ȱ��ȭ
        if (leftGrabber != null) leftGrabber.EnableGrabbing(false);
        if (rightGrabber != null) rightGrabber.EnableGrabbing(false);
        Debug.Log("��� ��� ���� ��Ȱ��ȭ");

        // 10. ��� ���� ����
        Coroutine leftReturn = StartCoroutine(MoveHandToPosition(currentLeftHand, leftHandOriginalPos));
        Coroutine rightReturn = StartCoroutine(MoveHandToPosition(currentRightHand, rightHandOriginalPos));

        yield return leftReturn;
        yield return rightReturn;
        Debug.Log("��� ���� �Ϸ�");
    }

    float SelectRandomGrabPosition(bool? forLeftHand = null)
    {
        if (forLeftHand == true)
        {
            // �޼տ� - ���ʰ� �߾� ��ġ��
            float[] leftPositions = { -2f, -1f, 0f };
            return leftPositions[Random.Range(0, leftPositions.Length)];
        }
        else if (forLeftHand == false)
        {
            // �����տ� - �����ʰ� �߾� ��ġ��
            float[] rightPositions = { 0f, 1f, 2f };
            return rightPositions[Random.Range(0, rightPositions.Length)];
        }
        else
        {
            // ���� ���� - ��� ��ġ, ����ġ ����
            float[] positions = { -2f, -1f, 0f, 1f, 2f };
            float[] weights = { 0.2f, 0.25f, 0.1f, 0.25f, 0.2f }; // �߾��� Ȯ�� ����

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
            Debug.LogError("�̵��� ���� null�Դϴ�!");
            yield break;
        }

        Vector3 startPosition = hand.transform.position;
        float distance = Vector3.Distance(startPosition, targetPosition);
        float moveTime = distance / handMoveSpeed;
        float elapsed = 0f;

        Debug.Log($"�� �̵� ����: {startPosition} -> {targetPosition}, �Ÿ�: {distance:F2}, ���� �ð�: {moveTime:F2}��");

        while (elapsed < moveTime)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / moveTime;
            if (progress > 1f) progress = 1f;

            Vector3 currentPos = Vector3.Lerp(startPosition, targetPosition, progress);
            hand.transform.position = currentPos;

            yield return null;
        }

        // ���� ��ġ Ȯ���� ����
        hand.transform.position = targetPosition;
        Debug.Log($"�� �̵� �Ϸ�: ���� ��ġ {hand.transform.position}");
    }

    // �׽�Ʈ �޼����
    [ContextMenu("Test Grab")]
    public void TestGrab()
    {
        if (!isGrabbing)
        {
            Debug.Log("���� ��� �׽�Ʈ ����");
            StartCoroutine(ExecuteGrab());
        }
        else
        {
            Debug.Log("�̹� ��� ���Դϴ�");
        }
    }

    [ContextMenu("Reset Hand Positions")]
    public void ResetHandPositions()
    {
        if (currentLeftHand != null)
        {
            currentLeftHand.transform.position = leftHandOriginalPos;
            Debug.Log($"�޼� ��ġ ����: {leftHandOriginalPos}");
        }
        if (currentRightHand != null)
        {
            currentRightHand.transform.position = rightHandOriginalPos;
            Debug.Log($"������ ��ġ ����: {rightHandOriginalPos}");
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
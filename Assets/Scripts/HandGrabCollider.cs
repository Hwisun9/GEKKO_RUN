using UnityEngine;

public class HandGrabCollider : MonoBehaviour
{
    private bool isGrabbingActive = false;
    private Collider2D handCollider;

    void Awake()
    {
        Debug.Log($"HandGrabCollider Awake: {gameObject.name}");
        SetupCollider();
    }

    void Start()
    {
        Debug.Log($"HandGrabCollider Start: {gameObject.name}");
    }

    void SetupCollider()
    {
        handCollider = GetComponent<Collider2D>();
        if (handCollider == null)
        {
            Debug.LogError($"{gameObject.name}에 Collider2D가 없습니다!");
            return;
        }

        handCollider.isTrigger = true;
        Debug.Log($"{gameObject.name} 콜라이더 설정 완료");

        // 초기에는 비활성화
        EnableGrabbing(false);
    }

    public void EnableGrabbing(bool enable)
    {
        isGrabbingActive = enable;

        if (handCollider != null)
        {
            handCollider.enabled = enable;
        }

        Debug.Log($"{gameObject.name} 잡기 상태 변경: {enable}");

        // 시각적 효과
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.color = enable ? Color.red : Color.white;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[{gameObject.name}] 충돌 감지: {other.name} (태그: {other.tag}) - 잡기 활성: {isGrabbingActive}");

        if (isGrabbingActive && other.CompareTag("Player"))
        {
            Debug.Log("!!! 플레이어가 손에 잡혔습니다 !!!");

            if (GameManager.Instance != null)
            {
                Debug.Log("GameManager.GameOver() 호출");
                GameManager.Instance.TakeDamage();
            }
            else
            {
                Debug.LogError("GameManager.Instance가 null입니다!");
            }

            // 한 번만 실행되도록
            EnableGrabbing(false);
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (isGrabbingActive && other.CompareTag("Player"))
        {
            Debug.Log($"[{gameObject.name}] Stay 충돌: {other.name}");
        }
    }
}
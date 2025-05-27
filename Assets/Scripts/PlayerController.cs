using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Rigidbody2D rb;
    private Animator animator;
    private Vector2 targetPosition;
    private bool isMoving = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        targetPosition = rb.position;
    }

    void Update()
    {
        // 게임이 활성화되어 있을 때만 조작 가능
        if (GameManager.Instance == null || !GameManager.Instance.isGameActive) return;

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                Vector3 worldPos = Camera.main.ScreenToWorldPoint(touch.position);
                targetPosition = new Vector2(worldPos.x, worldPos.y);
                isMoving = true;
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            targetPosition = new Vector2(worldPos.x, worldPos.y);
            isMoving = true;
        }

        if (animator != null)
        {
            animator.SetFloat("Speed", isMoving ? 1 : 0);
        }
    }

    void FixedUpdate()
    {
        if (isMoving)
        {
            Vector2 currentPos = rb.position;
            Vector2 direction = (targetPosition - currentPos).normalized;
            Vector2 newPosition = Vector2.MoveTowards(currentPos, targetPosition, moveSpeed * Time.fixedDeltaTime);
            rb.MovePosition(newPosition);

            if (Vector2.Distance(newPosition, targetPosition) < 0.1f)
            {
                isMoving = false;
                rb.linearVelocity = Vector2.zero;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("충돌 발생: " + other.gameObject.name + " / Tag: " + other.tag);

        if (other.CompareTag("Item"))
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(10);
                GameManager.Instance.AddCollectedItem(); // 아이템 수집 사운드 재생
            }
            Destroy(other.gameObject);
        }
        else if (other.CompareTag("Obstacle"))
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.TakeDamage(); // 라이프 감소
            }
            Destroy(other.gameObject);
        }
        else if (other.CompareTag("Magnet"))
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ActivateMagnet(); // 자석 효과 활성화
                GameManager.Instance.AddScore(5); // 자석 아이템도 약간의 점수
            }
            Destroy(other.gameObject);
        }
    }
}
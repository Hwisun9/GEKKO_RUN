using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Rigidbody2D rb;
    private Animator animator;
    private Vector2 targetPosition;
    private bool isMoving = false;

    //  무적 상태 관련
    private bool isInvincible = false;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    //  Speed 파라미터 존재 여부 확인용
    private bool hasSpeedParameter = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>(); // 
        targetPosition = rb.position;

        //  원본 색상 저장
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        //  Speed 파라미터 존재 여부 확인
        CheckSpeedParameter();
    }

    //  Speed 파라미터 존재 여부 확인 메서드
    void CheckSpeedParameter()
    {
        if (animator != null)
        {
            foreach (AnimatorControllerParameter param in animator.parameters)
            {
                if (param.name == "Speed")
                {
                    hasSpeedParameter = true;
                    Debug.Log(" Speed parameter found in Player Animator");
                    return;
                }
            }
            Debug.LogWarning(" Speed parameter not found in Player Animator. Animation updates will be skipped.");
        }
    }

    void Update()
    {
        // 게임이 활성화되어 있을 때만 조작 가능
        if (GameManager.Instance == null || !GameManager.Instance.isGameActive) return;

        HandleInput();
        UpdateAnimator();
    }

    void HandleInput()
    {
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
    }

    //  안전한 Animator 업데이트 메서드
    void UpdateAnimator()
    {
        if (animator != null && hasSpeedParameter)
        {
            try
            {
                animator.SetFloat("Speed", isMoving ? 1 : 0);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Failed to update Speed parameter: {e.Message}");
                hasSpeedParameter = false; // 더 이상 시도하지 않음
            }
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
            HandleItemCollision(other);
        }
        else if (other.CompareTag("Obstacle"))
        {
            HandleObstacleCollision(other);
        }
        else if (other.CompareTag("Magnet"))
        {
            HandleMagnetCollision(other);
        }
        else if (other.CompareTag("Mushroom"))
        {
            HandleMushroomCollision(other);
        }
        else if (other.CompareTag("Hide"))
        {
            HandleHideCollision(other);
        }
    }

    void HandleItemCollision(Collider2D other)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(10);
            GameManager.Instance.AddCollectedItem(); // 아이템 수집 사운드 재생
        }
        Destroy(other.gameObject);
    }

    //  수정된 장애물 충돌 처리
    void HandleObstacleCollision(Collider2D other)
    {
        //  부스터 활성화 중이면 장애물 파괴
        if (BoosterSystem.Instance != null && BoosterSystem.Instance.IsBoosterActive())
        {
            Debug.Log(" Booster active - Destroying obstacle!");
            BoosterSystem.Instance.DestroyObstacle(other.gameObject);
            return; // 데미지 받지 않음
        }

        // 투명화 상태가 아니고 무적 상태가 아닐 때만 데미지
        if (GameManager.Instance != null && !GameManager.Instance.IsHideActive() && !isInvincible)
        {
            GameManager.Instance.TakeDamage(); // 라이프 감소
            Destroy(other.gameObject);
        }
        else if (GameManager.Instance != null && (GameManager.Instance.IsHideActive() || isInvincible))
        {
            Debug.Log("Player passed through obstacle while protected!");
        }
    }

    void HandleMagnetCollision(Collider2D other)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ActivateMagnet(); // 자석 효과 활성화
            GameManager.Instance.AddScore(10); // 자석 아이템도 약간의 점수
        }
        Destroy(other.gameObject);
    }

    void HandleMushroomCollision(Collider2D other)
    {
        Debug.Log("Mushroom collected! Player will shrink.");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ActivateShrink();
            GameManager.Instance.AddScore(10); // 버프 아이템은 점수를 더 많이
            GameManager.Instance.AddCollectedItem();
        }

        Destroy(other.gameObject);
    }

    void HandleHideCollision(Collider2D other)
    {
        Debug.Log("Hide Potion collected! Player will become transparent.");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ActivateHide();
            GameManager.Instance.AddScore(10); // 더 강력한 효과이므로 점수도 더 높게
            GameManager.Instance.AddCollectedItem();
        }

        Destroy(other.gameObject);
    }

    //  무적 상태 설정
    public void SetInvincible(bool invincible)
    {
        isInvincible = invincible;

        if (spriteRenderer != null)
        {
            if (invincible)
            {
                // 무적 상태일 때 색상 변경 (황금색)
                spriteRenderer.color = Color.yellow;
                Debug.Log(" Player is now invincible!");
            }
            else
            {
                // 원본 색상 복원
                spriteRenderer.color = originalColor;
                Debug.Log(" Player invincibility ended!");
            }
        }
    }

    //  현재 무적 상태 확인
    public bool IsInvincible() => isInvincible;
}
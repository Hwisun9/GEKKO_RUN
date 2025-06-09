using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Rigidbody2D rb;
    private Animator animator;
    private Vector2 targetPosition;
    private bool isMoving = false;

    // 무적 상태 관련
    private bool isInvincible = false;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    // 효과 관리
    private PlayerEffects playerEffects;

    // Speed 파라미터 존재 여부 확인용
    private bool hasSpeedParameter = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerEffects = GetComponent<PlayerEffects>();
        
        // PlayerEffects 컴포넌트가 없으면 추가
        if (playerEffects == null)
        {
            playerEffects = gameObject.AddComponent<PlayerEffects>();
        }
        
        targetPosition = rb.position;

        // 원본 색상 저장
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        // Speed 파라미터 존재 여부 확인
        CheckSpeedParameter();
    }

    // Speed 파라미터 존재 여부 확인 메서드
    void CheckSpeedParameter()
    {
        if (animator != null)
        {
            foreach (AnimatorControllerParameter param in animator.parameters)
            {
                if (param.name == "Speed")
                {
                    hasSpeedParameter = true;
                    Debug.Log("Speed parameter found in Player Animator");
                    return;
                }
            }
            Debug.LogWarning("Speed parameter not found in Player Animator. Animation updates will be skipped.");
        }
    }

    void Update()
    {
        // 게임이 활성화되어 있을 때만 조작 가능
        if (GameManager.Instance == null || !GameManager.Instance.isGameActive) return;

        HandleInput();
        
        // 애니메이터 업데이트는 매 프레임이 아닌 주기적으로 수행
        if (Time.frameCount % 2 == 0) // 2프레임마다 한 번씩만 실행
        {
            UpdateAnimator();
        }
    }

    private Vector3 _cachedCameraPos;
    private Camera _mainCamera;
    
    void HandleInput()
    {
        // Camera.main은 비용이 높은 호출이므로 캐싱
        if (_mainCamera == null)
        {
            _mainCamera = Camera.main;
            if (_mainCamera != null)
            {
                _cachedCameraPos = _mainCamera.transform.position;
            }
        }
        
        if (_mainCamera == null) return;
        
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                Vector3 worldPos = _mainCamera.ScreenToWorldPoint(touch.position);
                targetPosition = new Vector2(worldPos.x, worldPos.y);
                isMoving = true;
            }
        }
        else if (Input.GetMouseButtonDown(0))
        {
            Vector3 worldPos = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
            targetPosition = new Vector2(worldPos.x, worldPos.y);
            isMoving = true;
        }
    }

    // 안전한 Animator 업데이트 메서드
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
        // Debug 로그를 개발 모드에서만 활성화하거나 제거
        // Debug.Log("충돌 발생: " + other.gameObject.name + " / Tag: " + other.tag);

        string tag = other.tag;
        
        // CompareTag는 string 비교보다 효율적임
        if (tag == "Item")
        {
            HandleItemCollision(other);
        }
        else if (tag == "Obstacle")
        {
            HandleObstacleCollision(other);
        }
        else if (tag == "Magnet")
        {
            HandleMagnetCollision(other);
        }
        else if (tag == "Mushroom")
        {
            HandleMushroomCollision(other);
        }
        else if (tag == "Hide")
        {
            HandleHideCollision(other);
        }
    }

    void HandleItemCollision(Collider2D other)
    {
        GameManager gameManager = GameManager.Instance;
        if (gameManager != null)
        {
            gameManager.AddScore(10);
            // 아이템 수집 사운드만 재생하고 콤보 시스템은 여기서만 호출
            gameManager.PlayItemCollectSound();
        }
        
        // 콤보 시스템 업데이트
        ComboSystem comboSystem = ComboSystem.Instance;
        if (comboSystem != null)
        {
            comboSystem.OnItemCollected();
        }
        
        // 객체 파괴 대신 비활성화 (재사용 가능하도록)
        other.gameObject.SetActive(false);
    }

    // 최적화된 장애물 충돌 처리
    void HandleObstacleCollision(Collider2D other)
    {
        BoosterSystem boosterSystem = BoosterSystem.Instance;
        GameManager gameManager = GameManager.Instance;
        ComboSystem comboSystem = ComboSystem.Instance;
        
        // 부스터 활성화 중이면 장애물 파괴 및 콤보 증가
        if (boosterSystem != null && boosterSystem.IsBoosterActive())
        {
            // Debug.Log("Booster active - Destroying obstacle and increasing combo!");
            
            // 장애물 파괴
            boosterSystem.DestroyObstacle(other.gameObject);
            
            // 콤보 증가 (아이템을 획득한 것처럼 처리)
            if (comboSystem != null)
            {
                comboSystem.OnItemCollected();
            }
            
            // 점수 추가 (일반 아이템 획득과 동일하게)
            if (gameManager != null)
            {
                gameManager.AddScore(10);
                // 사운드만 재생하고 콤보는 위에서 직접 호출
                gameManager.PlayItemCollectSound();
            }
            
            return; // 데미지 받지 않음
        }

        // 투명화 상태가 아니고 무적 상태가 아닐 때만 데미지
        if (gameManager != null && 
            !playerEffects.IsHideActive() && 
            !isInvincible)
        {
            gameManager.TakeDamage(); // 라이프 감소
            
            // 콤보 리셋 (일반 상태에서 장애물과 충돌한 경우)
            if (comboSystem != null)
            {
                comboSystem.ResetCombo();
            }
            
            // 객체 파괴 대신 비활성화 (재사용 가능하도록)
            other.gameObject.SetActive(false);
        }
        else
        {
            // Debug.Log("Player passed through obstacle while protected!");
        }
    }

    void HandleMagnetCollision(Collider2D other)
    {
        GameManager gameManager = GameManager.Instance;
        if (gameManager != null)
        {
            gameManager.ActivateMagnet(); // 자석 효과 활성화
            gameManager.AddScore(10); // 자석 아이템도 약간의 점수
            gameManager.PlayItemCollectSound(); // 사운드만 재생
        }
        
        // 콤보 시스템 업데이트
        ComboSystem comboSystem = ComboSystem.Instance;
        if (comboSystem != null)
        {
            comboSystem.OnItemCollected();
        }
        
        // 객체 파괴 대신 비활성화 (재사용 가능하도록)
        other.gameObject.SetActive(false);
    }

    void HandleMushroomCollision(Collider2D other)
    {
        // Debug.Log("Mushroom collected! Player will shrink.");

        if (playerEffects != null)
        {
            playerEffects.ActivateShrink(); // 새로운 효과 시스템 사용
            
            GameManager gameManager = GameManager.Instance;
            if (gameManager != null)
            {
                gameManager.AddScore(10); // 버프 아이템은 점수를 더 많이
                gameManager.PlayItemCollectSound(); // 사운드만 재생
            }
        }
        
        // 콤보 시스템 업데이트
        ComboSystem comboSystem = ComboSystem.Instance;
        if (comboSystem != null)
        {
            comboSystem.OnItemCollected();
        }
        
        // 객체 파괴 대신 비활성화 (재사용 가능하도록)
        other.gameObject.SetActive(false);
    }

    void HandleHideCollision(Collider2D other)
    {
        // Debug.Log("Hide Potion collected! Player will become transparent.");

        if (playerEffects != null)
        {
            playerEffects.ActivateHide(); // 새로운 효과 시스템 사용
            
            GameManager gameManager = GameManager.Instance;
            if (gameManager != null)
            {
                gameManager.AddScore(10); // 더 강력한 효과이므로 점수도 더 높게
                gameManager.PlayItemCollectSound(); // 사운드만 재생
            }
        }
        
        // 콤보 시스템 업데이트
        ComboSystem comboSystem = ComboSystem.Instance;
        if (comboSystem != null)
        {
            comboSystem.OnItemCollected();
        }
        
        // 객체 파괴 대신 비활성화 (재사용 가능하도록)
        other.gameObject.SetActive(false);
    }

    // 무적 상태 설정
    public void SetInvincible(bool invincible)
    {
        isInvincible = invincible;

        if (spriteRenderer != null)
        {
            if (invincible)
            {
                // 무적 상태일 때 색상 변경 (황금색)
                spriteRenderer.color = Color.yellow;
                Debug.Log("Player is now invincible!");
            }
            else
            {
                // 원본 색상 복원
                spriteRenderer.color = originalColor;
                Debug.Log("Player invincibility ended!");
            }
        }
    }

    // 현재 무적 상태 확인
    public bool IsInvincible() => isInvincible;
    
    // 새로운 효과 시스템 상태 확인 메소드들
    public bool IsShrinkActive() => playerEffects?.IsShrinkActive() ?? false;
    public bool IsHideActive() => playerEffects?.IsHideActive() ?? false;
}
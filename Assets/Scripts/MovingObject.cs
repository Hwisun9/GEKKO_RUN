using UnityEngine;

public class MovingObject : MonoBehaviour
{
    public float speed = 3f;
    public float destroyBoundary = -14f; // 전체 화면 밖으로 완전히 나갈 것을 확실히 함

    private Rigidbody2D rb;
    private Vector2 _velocityCache;
    
    void Awake()
    {
        // 필요한 컴포넌트 캐싱
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous; // 최적화: 충돌 감지 개선
            rb.interpolation = RigidbodyInterpolation2D.Interpolate; // 최적화: 움직임 부드럽게
        }
    }
    
    void OnEnable()
    {
        // 객체가 활성화될 때 초기화
        if (rb != null)
        {
            _velocityCache = Vector2.down * speed;
            rb.linearVelocity = _velocityCache;
        }
    }

    void FixedUpdate()
    {
        // Update 대신 FixedUpdate에서 물리 처리 (더 효율적)
        if (rb != null && rb.linearVelocity != _velocityCache)
        {
            rb.linearVelocity = _velocityCache;
        }
    }
    
    void Update()
    {
        // 화면 밖으로 나가면 처리 - 더 넓은 범위 적용
        if (transform.position.y < destroyBoundary) // 전체 화면 밖으로 완전히 나갈 것을 확실히 함
        {
            // 객체 풀링 활용
            if (ObjectPool.Instance != null)
            {
                string tag = gameObject.tag + "_" + gameObject.name.Replace("(Clone)", "");
                gameObject.SetActive(false);
                ObjectPool.Instance.ReturnToPool(tag, gameObject);
            }
            else
            {
                // 객체 풀이 없으면 제거
                Destroy(gameObject);
            }
        }
    }

    // 속도 동적 설정 메서드 (부스터 시스템 등에서 호출될 수 있음)
    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
        
        // 속도 캐시 업데이트
        _velocityCache = Vector2.down * speed;
        
        // Rigidbody2D가 있으면 즉시 속도 업데이트
        if (rb != null)
        {
            rb.linearVelocity = _velocityCache;
        }
    }

    // 부스터 시스템 효과를 받기 위한 메서드
    public void ApplyBoosterEffect(float multiplier)
    {
        SetSpeed(speed * multiplier);
    }

    // 부스터 시스템 효과 해제를 위한 메서드
    public void RemoveBoosterEffect(float multiplier)
    {
        SetSpeed(speed / multiplier);
    }
}

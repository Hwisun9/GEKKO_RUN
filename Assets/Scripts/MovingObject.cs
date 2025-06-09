using UnityEngine;

public class MovingObject : MonoBehaviour
{
    public float speed = 3f;
    public float destroyBoundary = -7f;

    private Rigidbody2D rb;

    void Start()
    {
        // Rigidbody2D 참조 획득
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
        }

        // 초기 속도 설정
        rb.linearVelocity = Vector2.down * speed;
    }

    void Update()
    {
        // 화면 밖으로 나가면 제거
        if (transform.position.y < destroyBoundary)
        {
            Destroy(gameObject);
        }
    }

    // 속도 동적 설정 메서드 (부스터 시스템 등에서 호출될 수 있음)
    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
        
        // Rigidbody2D가 있으면 속도 업데이트
        if (rb != null)
        {
            rb.linearVelocity = Vector2.down * speed;
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

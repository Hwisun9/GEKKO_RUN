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

        animator.SetFloat("Speed", isMoving ? 1 : 0);
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
            }
            else
            {
                Debug.LogWarning("GameManager.Instance is null!");
            }
            Destroy(other.gameObject);
        }
        else if (other.CompareTag("Obstacle"))
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.GameOver();
            }
            else
            {
                Debug.LogWarning("GameManager.Instance is null!");
            }
        }
    }

}

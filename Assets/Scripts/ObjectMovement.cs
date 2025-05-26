using UnityEngine;

public class ObjectMovement : MonoBehaviour
{
    private float speed;

    void Update()
    {
        if (!GameManager.Instance.isGameActive) return;

        // 아래로 이동
        transform.Translate(Vector3.down * speed * Time.deltaTime);

        // 화면 밖으로 나갔을 때 제거
        if (transform.position.y < -10f)
        {
            Destroy(gameObject);
        }
    }

    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }
}

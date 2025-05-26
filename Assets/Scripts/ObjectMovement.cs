using UnityEngine;

public class ObjectMovement : MonoBehaviour
{
    private float speed;

    void Update()
    {
        if (!GameManager.Instance.isGameActive) return;

        // �Ʒ��� �̵�
        transform.Translate(Vector3.down * speed * Time.deltaTime);

        // ȭ�� ������ ������ �� ����
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

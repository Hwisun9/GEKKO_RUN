using UnityEngine;

public class MovingObject : MonoBehaviour
{
    public float speed = 3f;
    public float destroyBoundary = -7f;

    void Update()
    {
        // �Ʒ��� �̵�
        //transform.Translate(Vector3.down * speed * Time.deltaTime);

        // ȭ�� ������ ������ ����
        if (transform.position.y < destroyBoundary)
        {
            Destroy(gameObject);
        }
    }
}

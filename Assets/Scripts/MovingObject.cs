using UnityEngine;

public class MovingObject : MonoBehaviour
{
    public float speed = 3f;
    public float destroyBoundary = -7f;

    void Update()
    {
        // 아래로 이동
        //transform.Translate(Vector3.down * speed * Time.deltaTime);

        // 화면 밖으로 나가면 삭제
        if (transform.position.y < destroyBoundary)
        {
            Destroy(gameObject);
        }
    }
}

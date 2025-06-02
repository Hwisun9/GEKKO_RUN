using UnityEngine;

public class HandGrabCollider : MonoBehaviour
{
    private bool isGrabbingActive = false;
    private Collider2D handCollider;

    void Awake()
    {
        Debug.Log($"HandGrabCollider Awake: {gameObject.name}");
        SetupCollider();
    }

    void Start()
    {
        Debug.Log($"HandGrabCollider Start: {gameObject.name}");
    }

    void SetupCollider()
    {
        handCollider = GetComponent<Collider2D>();
        if (handCollider == null)
        {
            Debug.LogError($"{gameObject.name}�� Collider2D�� �����ϴ�!");
            return;
        }

        handCollider.isTrigger = true;
        Debug.Log($"{gameObject.name} �ݶ��̴� ���� �Ϸ�");

        // �ʱ⿡�� ��Ȱ��ȭ
        EnableGrabbing(false);
    }

    public void EnableGrabbing(bool enable)
    {
        isGrabbingActive = enable;

        if (handCollider != null)
        {
            handCollider.enabled = enable;
        }

        Debug.Log($"{gameObject.name} ��� ���� ����: {enable}");

        // �ð��� ȿ��
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.color = enable ? Color.red : Color.white;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[{gameObject.name}] �浹 ����: {other.name} (�±�: {other.tag}) - ��� Ȱ��: {isGrabbingActive}");

        if (isGrabbingActive && other.CompareTag("Player"))
        {
            Debug.Log("!!! �÷��̾ �տ� �������ϴ� !!!");

            if (GameManager.Instance != null)
            {
                Debug.Log("GameManager.GameOver() ȣ��");
                GameManager.Instance.TakeDamage();
            }
            else
            {
                Debug.LogError("GameManager.Instance�� null�Դϴ�!");
            }

            // �� ���� ����ǵ���
            EnableGrabbing(false);
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (isGrabbingActive && other.CompareTag("Player"))
        {
            Debug.Log($"[{gameObject.name}] Stay �浹: {other.name}");
        }
    }
}
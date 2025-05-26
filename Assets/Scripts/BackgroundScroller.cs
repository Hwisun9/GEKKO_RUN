using UnityEngine;

public class BackgroundRepeat : MonoBehaviour
{
    [Header("��ũ�� ����")]
    public float baseScrollSpeed = 0.5f; // �⺻ ��ũ�� �ӵ�
    public bool syncWithGameSpeed = true; // ���� �ӵ��� ����ȭ ����
    public float speedMultiplier = 0.1f; // ���� �ӵ� �ݿ� ����

    private Material thisMaterial;
    private float currentScrollSpeed;

    void Start()
    {
        thisMaterial = GetComponent<Renderer>().material;
        currentScrollSpeed = baseScrollSpeed;
    }

    void Update()
    {
        // ���� �ӵ��� ����ȭ
        if (syncWithGameSpeed && GameManager.Instance != null)
        {
            // GameManager�� ���� ���̵��� �ӵ��� �ݿ�
            float difficultyMultiplier = 1f;

            // Spawner���� ���̵� ���� ��������
            Spawner spawner = FindObjectOfType<Spawner>();
            if (spawner != null)
            {
                difficultyMultiplier = spawner.GetCurrentDifficulty();
            }

            // ��� ��ũ�� �ӵ� ���
            currentScrollSpeed = baseScrollSpeed * (1f + (difficultyMultiplier - 1f) * speedMultiplier);
        }

        // ��� ��ũ�� ����
        Vector2 newOffset = thisMaterial.mainTextureOffset;
        newOffset.Set(0, newOffset.y + (currentScrollSpeed * Time.deltaTime));
        thisMaterial.mainTextureOffset = newOffset;
    }
}
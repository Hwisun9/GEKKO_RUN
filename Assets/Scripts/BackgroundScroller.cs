using UnityEngine;

public class BackgroundRepeat : MonoBehaviour
{
    [Header("��ũ�� ����")]
    public float baseScrollSpeed = 0.5f; // �⺻ ��ũ�� �ӵ�
    public bool syncWithGameSpeed = true; // ���� �ӵ��� ����ȭ ����
    public float speedMultiplier = 0.1f; // ���� �ӵ� �ݿ� ����

    private Material thisMaterial;
    private float currentScrollSpeed;

    //  �ν��� ȿ�� ������ ���� ���� �ӵ� �����
    [System.NonSerialized] // Inspector�� ǥ������ ����
    public float originalBaseScrollSpeed; // BoosterSystem���� ������ �� �ֵ��� public

    void Start()
    {
        thisMaterial = GetComponent<Renderer>().material;
        currentScrollSpeed = baseScrollSpeed;

        //  ���� �ӵ� ����
        originalBaseScrollSpeed = baseScrollSpeed;

        Debug.Log($" BackgroundRepeat initialized - Base speed: {baseScrollSpeed}");
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

            //  �ν��� ȿ�� Ȯ��
            float boosterMultiplier = 1f;
            if (BoosterSystem.Instance != null && BoosterSystem.Instance.IsBoosterActive())
            {
                boosterMultiplier = BoosterSystem.Instance.GetSpeedMultiplier();
            }

            // ��� ��ũ�� �ӵ� ��� (���̵� + �ν��� ȿ��)
            currentScrollSpeed = baseScrollSpeed *
                                (1f + (difficultyMultiplier - 1f) * speedMultiplier) *
                                boosterMultiplier;
        }
        else
        {
            //  ����ȭ�� ������� �ʴ��� �ν��� ȿ���� ����
            float boosterMultiplier = 1f;
            if (BoosterSystem.Instance != null && BoosterSystem.Instance.IsBoosterActive())
            {
                boosterMultiplier = BoosterSystem.Instance.GetSpeedMultiplier();
            }

            currentScrollSpeed = baseScrollSpeed * boosterMultiplier;
        }

        // ��� ��ũ�� ����
        Vector2 newOffset = thisMaterial.mainTextureOffset;
        newOffset.Set(0, newOffset.y + (currentScrollSpeed * Time.deltaTime));
        thisMaterial.mainTextureOffset = newOffset;
    }

    //  ���� ��ũ�� �ӵ� Ȯ�� (������)
    public float GetCurrentScrollSpeed() => currentScrollSpeed;

    //  �ν��� ȿ�� ���� ���� (BoosterSystem���� ȣ��)
    public void SetBoosterSpeed(float multiplier)
    {
        baseScrollSpeed = originalBaseScrollSpeed * multiplier;
        Debug.Log($" Background speed boosted: {baseScrollSpeed} (x{multiplier})");
    }

    //  �ν��� ȿ�� ���� (BoosterSystem���� ȣ��)
    public void ResetBoosterSpeed()
    {
        baseScrollSpeed = originalBaseScrollSpeed;
        Debug.Log($" Background speed reset: {baseScrollSpeed}");
    }

    //  ���� �ӵ� ���� (���� ���� �� ȣ��)
    public void ResetToOriginalSpeed()
    {
        baseScrollSpeed = originalBaseScrollSpeed;
        currentScrollSpeed = baseScrollSpeed;
    }

    //  ����� ����
    public string GetSpeedInfo()
    {
        return $"Base: {baseScrollSpeed:F2}, Current: {currentScrollSpeed:F2}, Original: {originalBaseScrollSpeed:F2}";
    }
}
using UnityEngine;

public class BackgroundRepeat : MonoBehaviour
{
    [Header("스크롤 설정")]
    public float baseScrollSpeed = 0.5f; // 기본 스크롤 속도
    public bool syncWithGameSpeed = true; // 게임 속도와 동기화 여부
    public float speedMultiplier = 0.1f; // 게임 속도 반영 비율

    private Material thisMaterial;
    private float currentScrollSpeed;

    void Start()
    {
        thisMaterial = GetComponent<Renderer>().material;
        currentScrollSpeed = baseScrollSpeed;
    }

    void Update()
    {
        // 게임 속도와 동기화
        if (syncWithGameSpeed && GameManager.Instance != null)
        {
            // GameManager의 현재 난이도나 속도를 반영
            float difficultyMultiplier = 1f;

            // Spawner에서 난이도 정보 가져오기
            Spawner spawner = FindObjectOfType<Spawner>();
            if (spawner != null)
            {
                difficultyMultiplier = spawner.GetCurrentDifficulty();
            }

            // 배경 스크롤 속도 계산
            currentScrollSpeed = baseScrollSpeed * (1f + (difficultyMultiplier - 1f) * speedMultiplier);
        }

        // 배경 스크롤 적용
        Vector2 newOffset = thisMaterial.mainTextureOffset;
        newOffset.Set(0, newOffset.y + (currentScrollSpeed * Time.deltaTime));
        thisMaterial.mainTextureOffset = newOffset;
    }
}
using UnityEngine;

public class BackgroundRepeat : MonoBehaviour
{
    [Header("스크롤 설정")]
    public float baseScrollSpeed = 0.5f; // 기본 스크롤 속도
    public bool syncWithGameSpeed = true; // 게임 속도와 동기화 여부
    public float speedMultiplier = 0.1f; // 게임 속도 반영 비율

    private Material thisMaterial;
    private float currentScrollSpeed;

    //  부스터 효과 적용을 위한 원본 속도 저장용
    [System.NonSerialized] // Inspector에 표시하지 않음
    public float originalBaseScrollSpeed; // BoosterSystem에서 접근할 수 있도록 public

    void Start()
    {
        thisMaterial = GetComponent<Renderer>().material;
        currentScrollSpeed = baseScrollSpeed;

        //  원본 속도 저장
        originalBaseScrollSpeed = baseScrollSpeed;

        Debug.Log($" BackgroundRepeat initialized - Base speed: {baseScrollSpeed}");
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

            //  부스터 효과 확인
            float boosterMultiplier = 1f;
            if (BoosterSystem.Instance != null && BoosterSystem.Instance.IsBoosterActive())
            {
                boosterMultiplier = BoosterSystem.Instance.GetSpeedMultiplier();
            }

            // 배경 스크롤 속도 계산 (난이도 + 부스터 효과)
            currentScrollSpeed = baseScrollSpeed *
                                (1f + (difficultyMultiplier - 1f) * speedMultiplier) *
                                boosterMultiplier;
        }
        else
        {
            //  동기화를 사용하지 않더라도 부스터 효과는 적용
            float boosterMultiplier = 1f;
            if (BoosterSystem.Instance != null && BoosterSystem.Instance.IsBoosterActive())
            {
                boosterMultiplier = BoosterSystem.Instance.GetSpeedMultiplier();
            }

            currentScrollSpeed = baseScrollSpeed * boosterMultiplier;
        }

        // 배경 스크롤 적용
        Vector2 newOffset = thisMaterial.mainTextureOffset;
        newOffset.Set(0, newOffset.y + (currentScrollSpeed * Time.deltaTime));
        thisMaterial.mainTextureOffset = newOffset;
    }

    //  현재 스크롤 속도 확인 (디버깅용)
    public float GetCurrentScrollSpeed() => currentScrollSpeed;

    //  부스터 효과 직접 적용 (BoosterSystem에서 호출)
    public void SetBoosterSpeed(float multiplier)
    {
        baseScrollSpeed = originalBaseScrollSpeed * multiplier;
        Debug.Log($" Background speed boosted: {baseScrollSpeed} (x{multiplier})");
    }

    //  부스터 효과 해제 (BoosterSystem에서 호출)
    public void ResetBoosterSpeed()
    {
        baseScrollSpeed = originalBaseScrollSpeed;
        Debug.Log($" Background speed reset: {baseScrollSpeed}");
    }

    //  원본 속도 복원 (게임 시작 시 호출)
    public void ResetToOriginalSpeed()
    {
        baseScrollSpeed = originalBaseScrollSpeed;
        currentScrollSpeed = baseScrollSpeed;
    }

    //  디버깅 정보
    public string GetSpeedInfo()
    {
        return $"Base: {baseScrollSpeed:F2}, Current: {currentScrollSpeed:F2}, Original: {originalBaseScrollSpeed:F2}";
    }
}
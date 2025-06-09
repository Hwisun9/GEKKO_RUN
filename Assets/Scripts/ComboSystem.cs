using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ComboSystem : MonoBehaviour
{
    [Header("콤보 설정")]
    public float comboTimeLimit = 3f; // 콤보 제한시간
    public int specialComboCount = 5; // 특수 보너스 콤보 수
    public int boosterComboCount = 10; // 부스터 활성화 콤보 수

    [Header("점수 설정")]
    public int baseItemScore = 5; // 기본 아이템 점수
    public int comboMultiplier = 5; // 콤보당 추가 점수 승수
    public int specialComboBonus = 50; // 5콤보 특수 보너스
    public int boosterComboBonus = 100; // 10콤보 부스터 보너스

    // 상태 변수
    private int currentCombo = 0;
    private float comboTimer = 0f;
    private bool isComboActive = false;

    // 이벤트
    public System.Action<int> OnComboChanged; // 콤보 변경 시
    public System.Action<int> OnComboAchieved; // 특별 콤보 달성 시
    public System.Action OnComboReset; // 콤보 리셋 시
    public System.Action OnBoosterTriggered; // 부스터 활성화 시

    public static ComboSystem Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (isComboActive)
        {
            comboTimer -= Time.deltaTime;
            if (comboTimer <= 0f)
            {
                ResetCombo();
            }
        }
    }

    // 아이템 획득 시 호출
    public void OnItemCollected()
    {
        currentCombo++;
        comboTimer = comboTimeLimit;
        isComboActive = true;

        // 점수 계산
        int totalScore = CalculateScore();
        GameManager.Instance.AddScore(totalScore);

        // 이벤트 발생
        OnComboChanged?.Invoke(currentCombo);
        OnComboAchieved?.Invoke(currentCombo);

        Debug.Log($"콤보: {currentCombo}, 획득 점수: {totalScore}");

        // 10의 배수 콤보마다 부스터 활성화 (부스터가 이미 활성화되어 있지 않은 경우에만)
        if (currentCombo % boosterComboCount == 0)
        {
            // 부스터가 현재 활성화되어 있는지 확인
            bool isBoosterCurrentlyActive = BoosterSystem.Instance != null && BoosterSystem.Instance.IsBoosterActive();
            
            // 부스터가 활성화되어 있지 않은 경우에만 트리거
            if (!isBoosterCurrentlyActive)
            {
                Debug.Log($"{boosterComboCount}의 배수 콤보 달성! 부스터 활성화!");
                TriggerBooster();
            }
            else
            {
                Debug.Log($"{boosterComboCount}의 배수 콤보 달성했으나 부스터가 이미 활성화되어 있어 무시함");
            }
        }
        // 5의 배수 콤보마다 특별 보너스
        else if (currentCombo % specialComboCount == 0)
        {
            OnSpecialComboAchieved();
        }
    }

    // 새로운 점수 계산 로직
    int CalculateScore()
    {
        // 기본 점수
        int score = baseItemScore;
        
        // 콤보에 따른 추가 점수 (선형적으로 증가)
        int comboBonus = (currentCombo - 1) * comboMultiplier;
        score += comboBonus;
        
        // 특별 콤보 보너스 (5의 배수)
        if (currentCombo % specialComboCount == 0)
        {
            score += specialComboBonus;
        }
        
        // 부스터 콤보 보너스 (10의 배수)
        if (currentCombo % boosterComboCount == 0)
        {
            score += boosterComboBonus;
        }
        
        return score;
    }

    // 부스터 활성화
    void TriggerBooster()
    {
        // BoosterSystem에 부스터 활성화 요청
        if (BoosterSystem.Instance != null)
        {
            BoosterSystem.Instance.ActivateBooster();
        }
        else
        {
            Debug.LogWarning("BoosterSystem not found!");
        }

        // 이벤트 발생
        OnBoosterTriggered?.Invoke();

        // 특수 효과 (플래시, 사운드 등)
        if (GameManager.Instance != null)
        {
            // 황금색 플래시 효과
            GameManager.Instance.TriggerFlashEffect(Color.yellow, 0.3f);
        }
    }

    void OnSpecialComboAchieved()
    {
        Debug.Log($"{specialComboCount}의 배수 콤보 달성! 특별 보너스!");
        // 특수 효과 추가
    }

    public void ResetCombo()
    {
        if (currentCombo > 0)
        {
            Debug.Log($"콤보 리셋! (최고 콤보: {currentCombo})");
            OnComboReset?.Invoke();
        }

        currentCombo = 0;
        comboTimer = 0f;
        isComboActive = false;
        OnComboChanged?.Invoke(currentCombo);
    }

    // 현재 콤보 정보 반환
    public int GetCurrentCombo() => currentCombo;
    public float GetComboTimeLeft() => comboTimer;
    public float GetComboTimeRatio() => comboTimer / comboTimeLimit;
    public bool IsBoosterComboReached() => currentCombo >= boosterComboCount;
    
    // 점수 계산 정보를 외부에서 접근할 수 있는 메서드
    public int GetScoreForCombo(int combo)
    {
        int score = baseItemScore;
        int comboBonus = (combo - 1) * comboMultiplier;
        score += comboBonus;
        
        if (combo % specialComboCount == 0)
        {
            score += specialComboBonus;
        }
        
        if (combo % boosterComboCount == 0)
        {
            score += boosterComboBonus;
        }
        
        return score;
    }
}
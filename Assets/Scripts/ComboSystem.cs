using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ComboSystem : MonoBehaviour
{
    [Header("콤보 설정")]
    public float comboTimeLimit = 3f; // 콤보 제한시간
    public int specialComboCount = 5; // 특별 보너스 콤보 수

    [Header("점수 설정")]
    public int baseItemScore = 10;
    public int[] comboBonus = { 0, 10, 20, 30, 50 }; // 콤보별 추가 점수
    public int specialComboBonus = 50; // 5콤보 특별 보너스

    // 현재 상태
    private int currentCombo = 0;
    private float comboTimer = 0f;
    private bool isComboActive = false;

    // 이벤트
    public System.Action<int> OnComboChanged; // 콤보 변경 시
    public System.Action<int> OnComboAchieved; // 특정 콤보 달성 시
    public System.Action OnComboReset; // 콤보 리셋 시

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

        // 5콤보 달성 시 특별 처리
        if (currentCombo % specialComboCount == 0)
        {
            OnSpecialComboAchieved();
        }
    }

    int CalculateScore()
    {
        int score = baseItemScore;

        // 콤보 보너스 추가
        if (currentCombo <= comboBonus.Length)
        {
            score += comboBonus[currentCombo - 1];
        }
        else
        {
            score += comboBonus[comboBonus.Length - 1]; // 최대 보너스
        }

        // 5콤보 특별 보너스
        if (currentCombo % specialComboCount == 0)
        {
            score += specialComboBonus;
        }

        return score;
    }

    void OnSpecialComboAchieved()
    {
        Debug.Log($" {specialComboCount}콤보 달성! 특별 보너스!");
        // 특별 효과 실행
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

    // 현재 콤보 상태 반환
    public int GetCurrentCombo() => currentCombo;
    public float GetComboTimeLeft() => comboTimer;
    public float GetComboTimeRatio() => comboTimer / comboTimeLimit;
}
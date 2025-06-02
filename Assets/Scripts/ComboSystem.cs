using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ComboSystem : MonoBehaviour
{
    [Header("�޺� ����")]
    public float comboTimeLimit = 3f; // �޺� ���ѽð�
    public int specialComboCount = 5; // Ư�� ���ʽ� �޺� ��
    public int boosterComboCount = 10; //  �ν��� �ߵ� �޺� ��

    [Header("���� ����")]
    public int baseItemScore = 10;
    public int[] comboBonus = { 0, 10, 20, 30, 50 }; // �޺��� �߰� ����
    public int specialComboBonus = 50; // 5�޺� Ư�� ���ʽ�
    public int boosterComboBonus = 100; //  10�޺� �ν��� ���ʽ�

    // ���� ����
    private int currentCombo = 0;
    private float comboTimer = 0f;
    private bool isComboActive = false;

    // �̺�Ʈ
    public System.Action<int> OnComboChanged; // �޺� ���� ��
    public System.Action<int> OnComboAchieved; // Ư�� �޺� �޼� ��
    public System.Action OnComboReset; // �޺� ���� ��
    public System.Action OnBoosterTriggered; //  �ν��� �ߵ� ��

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

    // ������ ȹ�� �� ȣ��
    public void OnItemCollected()
    {
        currentCombo++;
        comboTimer = comboTimeLimit;
        isComboActive = true;

        // ���� ���
        int totalScore = CalculateScore();
        GameManager.Instance.AddScore(totalScore);

        // �̺�Ʈ �߻�
        OnComboChanged?.Invoke(currentCombo);
        OnComboAchieved?.Invoke(currentCombo);

        Debug.Log($"�޺�: {currentCombo}, ȹ�� ����: {totalScore}");

        //  10�޺� �ν��� üũ
        if (currentCombo == boosterComboCount)
        {
            TriggerBooster();
        }
        // 5�޺� �޼� �� Ư�� ó��
        else if (currentCombo % specialComboCount == 0)
        {
            OnSpecialComboAchieved();
        }
    }

    int CalculateScore()
    {
        int score = baseItemScore;

        // �޺� ���ʽ� �߰�
        if (currentCombo <= comboBonus.Length)
        {
            score += comboBonus[currentCombo - 1];
        }
        else
        {
            score += comboBonus[comboBonus.Length - 1]; // �ִ� ���ʽ�
        }

        //  10�޺� �ν��� ���ʽ�
        if (currentCombo == boosterComboCount)
        {
            score += boosterComboBonus;
        }
        // 5�޺� Ư�� ���ʽ�
        else if (currentCombo % specialComboCount == 0)
        {
            score += specialComboBonus;
        }

        return score;
    }

    //  �ν��� �ߵ�
    void TriggerBooster()
    {
        Debug.Log($" {boosterComboCount}�޺� �޼�! �ν��� �ߵ�!");

        // BoosterSystem�� �ν��� Ȱ��ȭ ��û
        BoosterSystem boosterSystem = FindObjectOfType<BoosterSystem>();
        if (boosterSystem != null)
        {
            boosterSystem.ActivateBooster();
        }
        else
        {
            Debug.LogWarning("BoosterSystem not found!");
        }

        // �̺�Ʈ �߻�
        OnBoosterTriggered?.Invoke();

        // Ư�� ȿ�� (��½��, ���� ��)
        if (GameManager.Instance != null)
        {
            // Ȳ�ݻ� ��½�� ȿ��
            GameManager.Instance.TriggerFlashEffect(Color.yellow, 0.3f);
        }
    }

    void OnSpecialComboAchieved()
    {
        Debug.Log($" {specialComboCount}�޺� �޼�! Ư�� ���ʽ�!");
        // Ư�� ȿ�� ����
    }

    public void ResetCombo()
    {
        if (currentCombo > 0)
        {
            Debug.Log($"�޺� ����! (�ְ� �޺�: {currentCombo})");
            OnComboReset?.Invoke();
        }

        currentCombo = 0;
        comboTimer = 0f;
        isComboActive = false;
        OnComboChanged?.Invoke(currentCombo);
    }

    // ���� �޺� ���� ��ȯ
    public int GetCurrentCombo() => currentCombo;
    public float GetComboTimeLeft() => comboTimer;
    public float GetComboTimeRatio() => comboTimer / comboTimeLimit;
    public bool IsBoosterComboReached() => currentCombo >= boosterComboCount; // 
}
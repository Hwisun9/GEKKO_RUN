using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ComboUI : MonoBehaviour
{
    [Header("UI ��ҵ�")]
    public TextMeshProUGUI comboCountText; // "3 COMBO!" �ؽ�Ʈ
    public TextMeshProUGUI comboScoreText; // "+25 ����!" �ؽ�Ʈ
    public Slider comboTimerBar; // �޺� Ÿ�̸� ��
    public GameObject comboPanel; // ��ü �޺� UI �г�
    public GameObject specialComboEffect; // 5�޺� Ư�� ȿ�� �г�

    [Header("�ִϸ��̼� ����")]
    public float textScaleAnimation = 1.3f; // �ؽ�Ʈ ũ�� �ִϸ��̼�
    public float animationDuration = 0.3f; // �ִϸ��̼� ���ӽð�
    public Color[] comboColors = { // �޺��� ����
        Color.white,    // 1�޺�
        Color.green,    // 2�޺�  
        Color.blue,     // 3�޺�
        Color.magenta,  // 4�޺�
        Color.red       // 5�޺�+
    };

    [Header("Ư�� ȿ�� ����")]
    public float specialEffectDuration = 2f; // 5�޺� ȿ�� ���ӽð�
    public string specialComboMessage = "AWESOME!"; // 5�޺� �޽���

    // ���� ����
    private Coroutine currentAnimation;
    private Vector3 originalTextScale;
    private bool isUIActive = false;

    void Start()
    {
        // �ʱ� ����
        if (comboCountText != null)
        {
            originalTextScale = comboCountText.transform.localScale;
        }

        // �ʱ� UI ���� ���� (�г��� Ȱ��ȭ ���·� �ΰ� ���� ��Ҹ� ����)
        SetupInitialUI();

        // ComboSystem ������ �������� Ȯ���ϰ� ����
        StartCoroutine(DelayedComboSystemConnection());
    }

    void SetupInitialUI()
    {
        // �г��� Ȱ��ȭ ���·� �����ϵ�, ���� ��ҵ鸸 ����
        if (comboPanel != null)
        {
            comboPanel.SetActive(true); // �г��� Ȱ��ȭ ����
        }

        // ���� UI ��ҵ� ����
        if (comboCountText != null) comboCountText.gameObject.SetActive(false);
        if (comboScoreText != null) comboScoreText.gameObject.SetActive(false);
        if (comboTimerBar != null) comboTimerBar.gameObject.SetActive(false);
        if (specialComboEffect != null) specialComboEffect.SetActive(false);

        isUIActive = false;
        Debug.Log("ComboUI �ʱ� ���� �Ϸ�");
    }

    IEnumerator DelayedComboSystemConnection()
    {
        // ComboSystem�� �ʱ�ȭ�� ������ ���
        int attempts = 0;
        while (ComboSystem.Instance == null && attempts < 50) // 5�� ���
        {
            attempts++;
            yield return new WaitForSeconds(0.1f);
        }

        if (ComboSystem.Instance != null)
        {
            Debug.Log("ComboSystem ���� ����!");

            // �޺� �ý��� �̺�Ʈ ����
            ComboSystem.Instance.OnComboChanged += UpdateComboDisplay;
            ComboSystem.Instance.OnComboAchieved += OnComboAchieved;
            ComboSystem.Instance.OnComboReset += OnComboReset;
        }
        else
        {
            Debug.LogError("ComboSystem ���� ����! ComboSystem�� ���� �ִ��� Ȯ���ϼ���.");
        }
    }

    void Update()
    {
        // �޺� Ÿ�̸� �� ������Ʈ
        if (isUIActive && ComboSystem.Instance != null)
        {
            UpdateTimerBar();
        }
    }

    void UpdateComboDisplay(int comboCount)
    {
        if (comboCount > 0)
        {
            ShowComboUI();

            // �޺� �ؽ�Ʈ ������Ʈ
            if (comboCountText != null)
            {
                comboCountText.text = $"{comboCount} COMBO!";

                // �޺��� ���� ����
                Color comboColor = GetComboColor(comboCount);
                comboCountText.color = comboColor;
            }

            // ���� �ؽ�Ʈ ������Ʈ
            if (comboScoreText != null)
            {
                int score = CalculateDisplayScore(comboCount);
                comboScoreText.text = $"+{score}";
                comboScoreText.color = GetComboColor(comboCount);
            }
        }
        else
        {
            HideComboUI();
        }
    }

    void OnComboAchieved(int comboCount)
    {
        Debug.Log($"UI: {comboCount}�޺� �޼�!");

        // �ؽ�Ʈ �ִϸ��̼� ����
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
        }
        currentAnimation = StartCoroutine(ComboTextAnimation());

        // 5�޺� �޼� �� Ư�� ȿ��
        if (comboCount % 5 == 0)
        {
            StartCoroutine(SpecialComboEffect());
        }
    }

    void OnComboReset()
    {
        Debug.Log("UI: �޺� ����");
        HideComboUI();

        // ���� ���� �ִϸ��̼� ����
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
            currentAnimation = null;
        }
    }

    void ShowComboUI()
    {
        if (!isUIActive)
        {
            isUIActive = true;
            Debug.Log("�޺� UI Ȱ��ȭ");

            // ���� UI ��ҵ� Ȱ��ȭ
            if (comboCountText != null) comboCountText.gameObject.SetActive(true);
            if (comboScoreText != null) comboScoreText.gameObject.SetActive(true);
            if (comboTimerBar != null) comboTimerBar.gameObject.SetActive(true);
        }
    }

    void HideComboUI()
    {
        if (isUIActive)
        {
            isUIActive = false;
            Debug.Log("�޺� UI ��Ȱ��ȭ");

            // ���� UI ��ҵ� ��Ȱ��ȭ
            if (comboCountText != null) comboCountText.gameObject.SetActive(false);
            if (comboScoreText != null) comboScoreText.gameObject.SetActive(false);
            if (comboTimerBar != null) comboTimerBar.gameObject.SetActive(false);
        }
    }

    void UpdateTimerBar()
    {
        if (comboTimerBar != null && ComboSystem.Instance != null)
        {
            float timeRatio = ComboSystem.Instance.GetComboTimeRatio();
            comboTimerBar.value = timeRatio;

            // Ÿ�̸� �� ���� ���� (�ð��� �������� ������)
            Image fillImage = comboTimerBar.fillRect?.GetComponent<Image>();
            if (fillImage != null)
            {
                fillImage.color = Color.Lerp(Color.red, Color.green, timeRatio);
            }
        }
    }

    Color GetComboColor(int comboCount)
    {
        if (comboCount <= 0) return Color.white;

        int colorIndex = Mathf.Min(comboCount - 1, comboColors.Length - 1);
        return comboColors[colorIndex];
    }

    int CalculateDisplayScore(int comboCount)
    {
        // ComboSystem�� ������ ��� ����
        int baseScore = 10;
        int[] bonuses = { 0, 5, 10, 15, 25 };

        int bonus = 0;
        if (comboCount <= bonuses.Length)
        {
            bonus = bonuses[comboCount - 1];
        }
        else
        {
            bonus = bonuses[bonuses.Length - 1];
        }

        int totalScore = baseScore + bonus;

        // 5�޺� Ư�� ���ʽ�
        if (comboCount % 5 == 0)
        {
            totalScore += 50;
        }

        return totalScore;
    }

    IEnumerator ComboTextAnimation()
    {
        if (comboCountText == null) yield break;

        Transform textTransform = comboCountText.transform;
        Vector3 startScale = originalTextScale;
        Vector3 targetScale = originalTextScale * textScaleAnimation;

        // ũ�� Ȯ�� �ִϸ��̼�
        float elapsed = 0f;
        while (elapsed < animationDuration / 2f)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / (animationDuration / 2f);

            textTransform.localScale = Vector3.Lerp(startScale, targetScale, progress);
            yield return null;
        }

        // ũ�� ��� �ִϸ��̼�
        elapsed = 0f;
        while (elapsed < animationDuration / 2f)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / (animationDuration / 2f);

            textTransform.localScale = Vector3.Lerp(targetScale, startScale, progress);
            yield return null;
        }

        // ���� ũ�� ����
        textTransform.localScale = startScale;
    }

    IEnumerator SpecialComboEffect()
    {
        Debug.Log(" 5�޺� Ư�� ȿ�� ����!");

        if (specialComboEffect != null)
        {
            // Ư�� ȿ�� �г� Ȱ��ȭ
            specialComboEffect.SetActive(true);

            // Ư�� �޽��� ����
            Text specialText = specialComboEffect.GetComponentInChildren<Text>();
            if (specialText != null)
            {
                specialText.text = specialComboMessage;
                specialText.color = Color.yellow;
            }

            // ȭ�� ��ü ȿ�� �ִϸ��̼�
            yield return StartCoroutine(ScreenFlashEffect());

            // ���� �ð� ���
            yield return new WaitForSeconds(specialEffectDuration);

            // Ư�� ȿ�� �г� ��Ȱ��ȭ
            specialComboEffect.SetActive(false);
        }

        // �߰� ȿ����
        yield return StartCoroutine(SpecialTextAnimation());
    }

    IEnumerator ScreenFlashEffect()
    {
        // ȭ�� �÷��� ȿ��
        Image flashImage = specialComboEffect?.GetComponent<Image>();
        if (flashImage != null)
        {
            Color originalColor = flashImage.color;

            // ��� �÷���
            for (int i = 0; i < 3; i++)
            {
                flashImage.color = Color.white;
                yield return new WaitForSeconds(0.1f);
                flashImage.color = originalColor;
                yield return new WaitForSeconds(0.1f);
            }
        }
    }

    IEnumerator SpecialTextAnimation()
    {
        if (comboCountText == null) yield break;

        // ������ ���� ȿ��
        float duration = 1f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            // ������ ���� ���
            float hue = (elapsed / duration) % 1f;
            Color rainbowColor = Color.HSVToRGB(hue, 1f, 1f);

            comboCountText.color = rainbowColor;

            // ũ�⵵ ���ݾ� ��ȭ
            float scale = 1f + Mathf.Sin(elapsed * 10f) * 0.1f;
            comboCountText.transform.localScale = originalTextScale * scale;

            yield return null;
        }

        // ���� ���·� ����
        comboCountText.transform.localScale = originalTextScale;
    }

    void OnDestroy()
    {
        // �̺�Ʈ ���� ����
        if (ComboSystem.Instance != null)
        {
            ComboSystem.Instance.OnComboChanged -= UpdateComboDisplay;
            ComboSystem.Instance.OnComboAchieved -= OnComboAchieved;
            ComboSystem.Instance.OnComboReset -= OnComboReset;
        }
    }
}

// �޺� UI ���� Ŭ���� - ���� �ؽ�Ʈ �ִϸ��̼ǿ�
public class ScoreTextAnimator : MonoBehaviour
{
    [Header("�ִϸ��̼� ����")]
    public float floatSpeed = 2f; // ���� �������� �ӵ�
    public float fadeSpeed = 1f; // ���̵� �ƿ� �ӵ�
    public float lifetime = 2f; // ���� �ð�

    private Text textComponent;
    private Vector3 startPosition;
    private float timer = 0f;

    void Start()
    {
        textComponent = GetComponent<Text>();
        startPosition = transform.position;

        // �ڵ� ���� ����
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        timer += Time.deltaTime;

        // ���� �������� �ִϸ��̼�
        Vector3 newPos = startPosition + Vector3.up * (timer * floatSpeed);
        transform.position = newPos;

        // ���̵� �ƿ� ȿ��
        if (textComponent != null)
        {
            float alpha = 1f - (timer / lifetime);
            Color color = textComponent.color;
            color.a = alpha;
            textComponent.color = color;
        }
    }

    // ���� �ؽ�Ʈ ������ ���� �޼���
    public static GameObject CreateScoreText(Vector3 position, string text, Color color, Transform parent = null)
    {
        // �⺻ �ؽ�Ʈ ������Ʈ ����
        GameObject scoreTextObj = new GameObject("ScoreText");

        if (parent != null)
        {
            scoreTextObj.transform.SetParent(parent, false);
        }

        // Text ������Ʈ �߰�
        Text textComp = scoreTextObj.AddComponent<Text>();
        textComp.text = text;
        textComp.color = color;
        textComp.fontSize = 24;
        textComp.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        textComp.alignment = TextAnchor.MiddleCenter;

        // ��ġ ����
        scoreTextObj.transform.position = position;

        // �ִϸ����� �߰�
        scoreTextObj.AddComponent<ScoreTextAnimator>();

        return scoreTextObj;
    }
}
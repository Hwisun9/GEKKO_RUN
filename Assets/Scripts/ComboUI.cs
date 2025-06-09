using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ComboUI : MonoBehaviour
{
    [Header("UI 요소들")]
    public TextMeshProUGUI comboCountText; // "3 COMBO!" 텍스트
    public TextMeshProUGUI comboScoreText; // "+25 점수!" 텍스트
    public Slider comboTimerBar; // 콤보 타이머 바
    public GameObject comboPanel; // 전체 콤보 UI 패널
    public GameObject specialComboEffect; // 5콤보 특수 효과 패널

    [Header("애니메이션 설정")]
    public float textScaleAnimation = 1.3f; // 텍스트 크기 애니메이션
    public float animationDuration = 0.3f; // 애니메이션 지속시간
    public Color[] comboColors = { // 콤보별 색상
        Color.white,    // 1콤보
        Color.green,    // 2콤보  
        Color.blue,     // 3콤보
        Color.magenta,  // 4콤보
        Color.red       // 5콤보+
    };

    [Header("특수 효과 설정")]
    public float specialEffectDuration = 2f; // 5콤보 효과 지속시간
    public string specialComboMessage = "AWESOME!"; // 5콤보 메시지

    // 내부 변수
    private Coroutine currentAnimation;
    private Vector3 originalTextScale;
    private bool isUIActive = false;

    void Start()
    {
        // 초기 설정
        if (comboCountText != null)
        {
            originalTextScale = comboCountText.transform.localScale;
        }

        // 초기 UI 상태 설정 (패널은 활성화 상태로 두고 내부 요소만 숨김)
        SetupInitialUI();

        // ComboSystem 연결이 지연될 경우 대비
        StartCoroutine(DelayedComboSystemConnection());
    }

    void SetupInitialUI()
    {
        // 패널은 활성화 상태로 유지하되, 내부 요소들만 숨김
        if (comboPanel != null)
        {
            comboPanel.SetActive(true); // 패널은 활성화 유지
        }

        // 내부 UI 요소들 숨김
        if (comboCountText != null) comboCountText.gameObject.SetActive(false);
        if (comboScoreText != null) comboScoreText.gameObject.SetActive(false);
        if (comboTimerBar != null) comboTimerBar.gameObject.SetActive(false);
        if (specialComboEffect != null) specialComboEffect.SetActive(false);

        isUIActive = false;
        Debug.Log("ComboUI 초기 설정 완료");
    }

    IEnumerator DelayedComboSystemConnection()
    {
        // ComboSystem이 초기화될 때까지 대기
        int attempts = 0;
        while (ComboSystem.Instance == null && attempts < 50) // 5초 대기
        {
            attempts++;
            yield return new WaitForSeconds(0.1f);
        }

        if (ComboSystem.Instance != null)
        {
            Debug.Log("ComboSystem 연결 성공!");

            // 콤보 시스템 이벤트 연결
            ComboSystem.Instance.OnComboChanged += UpdateComboDisplay;
            ComboSystem.Instance.OnComboAchieved += OnComboAchieved;
            ComboSystem.Instance.OnComboReset += OnComboReset;
        }
        else
        {
            Debug.LogError("ComboSystem 연결 실패! ComboSystem이 씬에 있는지 확인하세요.");
        }
    }

    void Update()
    {
        // 콤보 타이머 바 업데이트
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

            // 콤보 텍스트 업데이트
            if (comboCountText != null)
            {
                comboCountText.text = $"{comboCount} COMBO!";

                // 콤보별 색상 설정
                Color comboColor = GetComboColor(comboCount);
                comboCountText.color = comboColor;
            }

            // 점수 텍스트 업데이트
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
        Debug.Log($"UI: {comboCount}콤보 달성!");

        // 텍스트 애니메이션 실행
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
        }
        currentAnimation = StartCoroutine(ComboTextAnimation());

        // 5콤보 달성 시 특수 효과
        if (comboCount % 5 == 0)
        {
            StartCoroutine(SpecialComboEffect());
        }
    }

    void OnComboReset()
    {
        Debug.Log("UI: 콤보 리셋");
        HideComboUI();

        // 현재 실행 애니메이션 중지
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
            Debug.Log("콤보 UI 활성화");

            // 내부 UI 요소들 활성화
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
            Debug.Log("콤보 UI 비활성화");

            // 내부 UI 요소들 비활성화
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

            // 타이머 바 색상 설정 (시간이 줄어들면 빨개짐)
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
        // ComboSystem에서 점수 계산 로직 가져오기
        if (ComboSystem.Instance != null)
        {
            // ComboSystem에서 제공하는 점수 계산 메서드 사용
            return ComboSystem.Instance.GetScoreForCombo(comboCount);
        }
        else
        {
            // ComboSystem을 찾을 수 없는 경우 기본 점수 계산
            int baseScore = 10;
            int comboBonus = (comboCount - 1) * 5; // 기본 승수 5 사용
            int totalScore = baseScore + comboBonus;
            
            // 5의 배수 콤보 보너스
            if (comboCount % 5 == 0)
            {
                totalScore += 50;
            }
            
            // 10의 배수 콤보 보너스
            if (comboCount % 10 == 0)
            {
                totalScore += 100;
            }
            
            return totalScore;
        }
    }

    IEnumerator ComboTextAnimation()
    {
        if (comboCountText == null) yield break;

        Transform textTransform = comboCountText.transform;
        Vector3 startScale = originalTextScale;
        Vector3 targetScale = originalTextScale * textScaleAnimation;

        // 크기 확대 애니메이션
        float elapsed = 0f;
        while (elapsed < animationDuration / 2f)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / (animationDuration / 2f);

            textTransform.localScale = Vector3.Lerp(startScale, targetScale, progress);
            yield return null;
        }

        // 크기 축소 애니메이션
        elapsed = 0f;
        while (elapsed < animationDuration / 2f)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / (animationDuration / 2f);

            textTransform.localScale = Vector3.Lerp(targetScale, startScale, progress);
            yield return null;
        }

        // 원래 크기 복원
        textTransform.localScale = startScale;
    }

    IEnumerator SpecialComboEffect()
    {
        Debug.Log("5콤보 특수 효과 시작!");

        if (specialComboEffect != null)
        {
            // 특수 효과 패널 활성화
            specialComboEffect.SetActive(true);

            // 특수 메시지 설정
            Text specialText = specialComboEffect.GetComponentInChildren<Text>();
            if (specialText != null)
            {
                specialText.text = specialComboMessage;
                specialText.color = Color.yellow;
            }

            // 화면 전체 효과 애니메이션
            yield return StartCoroutine(ScreenFlashEffect());

            // 일정 시간 대기
            yield return new WaitForSeconds(specialEffectDuration);

            // 특수 효과 패널 비활성화
            specialComboEffect.SetActive(false);
        }

        // 추가 효과들
        yield return StartCoroutine(SpecialTextAnimation());
    }

    IEnumerator ScreenFlashEffect()
    {
        // 화면 플래시 효과
        Image flashImage = specialComboEffect?.GetComponent<Image>();
        if (flashImage != null)
        {
            Color originalColor = flashImage.color;

            // 여러 플래시
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

        // 무지개 색상 효과
        float duration = 1f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            // 무지개 색상 생성
            float hue = (elapsed / duration) % 1f;
            Color rainbowColor = Color.HSVToRGB(hue, 1f, 1f);

            comboCountText.color = rainbowColor;

            // 크기도 약간씩 변화
            float scale = 1f + Mathf.Sin(elapsed * 10f) * 0.1f;
            comboCountText.transform.localScale = originalTextScale * scale;

            yield return null;
        }

        // 원래 상태로 복원
        comboCountText.transform.localScale = originalTextScale;
    }

    void OnDestroy()
    {
        // 이벤트 연결 해제
        if (ComboSystem.Instance != null)
        {
            ComboSystem.Instance.OnComboChanged -= UpdateComboDisplay;
            ComboSystem.Instance.OnComboAchieved -= OnComboAchieved;
            ComboSystem.Instance.OnComboReset -= OnComboReset;
        }
    }
}

// 점수 UI 애니메이션 클래스 - 공중 텍스트 애니메이션용
public class ScoreTextAnimator : MonoBehaviour
{
    [Header("애니메이션 설정")]
    public float floatSpeed = 2f; // 위로 올라가는 속도
    public float fadeSpeed = 1f; // 페이드 아웃 속도
    public float lifetime = 2f; // 생존 시간

    private Text textComponent;
    private Vector3 startPosition;
    private float timer = 0f;

    void Start()
    {
        textComponent = GetComponent<Text>();
        startPosition = transform.position;

        // 자동 삭제 설정
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        timer += Time.deltaTime;

        // 위로 올라가는 애니메이션
        Vector3 newPos = startPosition + Vector3.up * (timer * floatSpeed);
        transform.position = newPos;

        // 페이드 아웃 효과
        if (textComponent != null)
        {
            float alpha = 1f - (timer / lifetime);
            Color color = textComponent.color;
            color.a = alpha;
            textComponent.color = color;
        }
    }

    // 점수 텍스트 생성을 위한 메서드
    public static GameObject CreateScoreText(Vector3 position, string text, Color color, Transform parent = null)
    {
        // 기본 텍스트 오브젝트 생성
        GameObject scoreTextObj = new GameObject("ScoreText");

        if (parent != null)
        {
            scoreTextObj.transform.SetParent(parent, false);
        }

        // Text 컴포넌트 추가
        Text textComp = scoreTextObj.AddComponent<Text>();
        textComp.text = text;
        textComp.color = color;
        textComp.fontSize = 24;
        textComp.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        textComp.alignment = TextAnchor.MiddleCenter;

        // 위치 설정
        scoreTextObj.transform.position = position;

        // 애니메이터 추가
        scoreTextObj.AddComponent<ScoreTextAnimator>();

        return scoreTextObj;
    }
}
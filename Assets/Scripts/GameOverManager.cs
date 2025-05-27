// GameOverManager.cs - 문제 해결된 버전
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class GameOverManager : MonoBehaviour
{
    [Header("UI Components")]
    public CanvasGroup gameOverCanvasGroup;
    public TextMeshProUGUI gameOverText;

    [Header("Game Stats Display")]
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI playTimeText;
    public TextMeshProUGUI highScoreText;
    public TextMeshProUGUI newRecordText;

    [Header("Animation Settings")]
    public float fadeDuration = 1f;
    public float statsAnimationDelay = 0.5f;

    [Header("Debug")]
    public bool enableDebugLogs = true;

    private bool isShowing = false;

    void Start()
    {
        InitializeGameOverPanel();
    }

    private void InitializeGameOverPanel()
    {
        if (gameOverCanvasGroup == null)
        {
            Debug.LogError("GameOverCanvasGroup is not assigned!");
            return;
        }

        // 패널 초기 상태 설정
        gameOverCanvasGroup.alpha = 0f;
        gameOverCanvasGroup.interactable = false;
        gameOverCanvasGroup.blocksRaycasts = false;

        // 통계 텍스트 초기화 - 완전히 보이게 설정 (테스트용)
        InitializeStatsTexts();

        if (enableDebugLogs)
        {
            Debug.Log("GameOverManager initialized");
        }
    }

    private void InitializeStatsTexts()
    {
        // 모든 텍스트를 활성화하고 완전히 불투명하게 설정
        SetupText(finalScoreText, "Final Score: 0", true);
        SetupText(playTimeText, "Play Time: 0sec", true);
        SetupText(highScoreText, "High Score: 0", true);
        SetupText(newRecordText, " New Record! ", false); // 신기록은 처음에 숨김
    }

    private void SetupText(TextMeshProUGUI textComponent, string defaultText, bool visible)
    {
        if (textComponent != null)
        {
            textComponent.text = defaultText;
            textComponent.gameObject.SetActive(visible);

            if (visible)
            {
                // 완전히 불투명하게 설정
                Color color = textComponent.color;
                color.a = 1f;
                textComponent.color = color;
            }

            if (enableDebugLogs)
            {
                Debug.Log($"Setup text: {textComponent.name} = {defaultText}, visible: {visible}");
            }
        }
    }

    public void ShowGameOverPanel()
    {
        if (gameOverCanvasGroup == null || isShowing) return;

        if (enableDebugLogs)
        {
            Debug.Log("=== Showing GameOver Panel ===");
        }

        isShowing = true;

        // 1. 먼저 통계 업데이트
        UpdateGameStats();

        // 2. 패널 페이드 인 (단순하게)
        StartCoroutine(ShowPanelCoroutine());
    }

    private IEnumerator ShowPanelCoroutine()
    {
        // 패널 페이드 인
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime; // unscaledDeltaTime 사용 (timeScale 영향 안받음)
            float alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            gameOverCanvasGroup.alpha = alpha;
            yield return null;
        }

        // 최종 설정
        gameOverCanvasGroup.alpha = 1f;
        gameOverCanvasGroup.interactable = true;
        gameOverCanvasGroup.blocksRaycasts = true;

        isShowing = false;

        if (enableDebugLogs)
        {
            Debug.Log("GameOver panel fully shown");
        }

        // 잠시 대기 후 통계 애니메이션 (선택사항)
        yield return new WaitForSecondsRealtime(statsAnimationDelay);
        StartStatsAnimation();
    }

    private void UpdateGameStats()
    {
        if (GameManager.Instance == null)
        {
            if (enableDebugLogs)
                Debug.LogError("GameManager.Instance is null!");
            return;
        }

        // 통계 텍스트 내용 업데이트
        SafeUpdateText(finalScoreText, "Score: " + GameManager.Instance.score.ToString());
        SafeUpdateText(playTimeText, "Time: " + FormatPlayTime(GameManager.Instance.playTime));

        int currentHighScore = PlayerPrefs.GetInt("HighScore", 0);
        SafeUpdateText(highScoreText, "Best: " + currentHighScore.ToString());

        CheckAndShowNewRecord();

        if (enableDebugLogs)
        {
            Debug.Log($"Stats Updated - Score: {GameManager.Instance.score}, Time: {FormatPlayTime(GameManager.Instance.playTime)}, HighScore: {currentHighScore}");
        }
    }

    private void SafeUpdateText(TextMeshProUGUI textComponent, string content)
    {
        if (textComponent != null && textComponent.gameObject != null)
        {
            try
            {
                textComponent.text = content;

                // 텍스트가 보이도록 확실히 설정
                textComponent.gameObject.SetActive(true);
                Color color = textComponent.color;
                color.a = 1f;
                textComponent.color = color;

                if (enableDebugLogs)
                {
                    Debug.Log($" Updated text: {textComponent.name} = '{content}'");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"Failed to update text: {ex.Message}");
            }
        }
        else
        {
            Debug.LogWarning($"TextMeshPro component is null or destroyed");
        }
    }

    private void CheckAndShowNewRecord()
    {
        if (newRecordText == null || GameManager.Instance == null) return;

        int previousHighScore = PlayerPrefs.GetInt("HighScore", 0);
        bool isNewRecord = GameManager.Instance.score > previousHighScore;

        if (isNewRecord && previousHighScore > 0)
        {
            SafeUpdateText(newRecordText, " NEW RECORD! ");
            newRecordText.gameObject.SetActive(true);

            if (enableDebugLogs)
            {
                Debug.Log(" New Record achieved!");
            }
        }
        else
        {
            newRecordText.gameObject.SetActive(false);

            if (enableDebugLogs)
            {
                Debug.Log("No new record");
            }
        }
    }

    private void StartStatsAnimation()
    {
        if (enableDebugLogs)
        {
            Debug.Log("Starting stats animation (optional effect)");
        }

        // 간단한 스케일 애니메이션으로 변경 (LeanTween 문제 방지)
        StartCoroutine(AnimateStatsCoroutine());
    }

    private IEnumerator AnimateStatsCoroutine()
    {
        // 각 텍스트에 간단한 펄스 효과
        AnimateTextScale(finalScoreText);
        yield return new WaitForSecondsRealtime(0.2f);

        AnimateTextScale(playTimeText);
        yield return new WaitForSecondsRealtime(0.2f);

        AnimateTextScale(highScoreText);
        yield return new WaitForSecondsRealtime(0.2f);

        // 신기록이 있다면 특별 효과
        if (newRecordText != null && newRecordText.gameObject.activeInHierarchy)
        {
            AnimateTextScale(newRecordText);
            StartCoroutine(NewRecordBlinkEffect());
        }
    }

    private void AnimateTextScale(TextMeshProUGUI textComponent)
    {
        if (textComponent != null && textComponent.gameObject != null)
        {
            StartCoroutine(ScalePulse(textComponent.transform));
        }
    }

    private IEnumerator ScalePulse(Transform textTransform)
    {
        Vector3 originalScale = textTransform.localScale;
        Vector3 targetScale = originalScale * 1.1f;

        // 커지기
        float elapsed = 0f;
        float duration = 0.15f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            textTransform.localScale = Vector3.Lerp(originalScale, targetScale, t);
            yield return null;
        }

        // 원래 크기로 돌아가기
        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            textTransform.localScale = Vector3.Lerp(targetScale, originalScale, t);
            yield return null;
        }

        textTransform.localScale = originalScale;
    }

    private IEnumerator NewRecordBlinkEffect()
    {
        if (newRecordText == null) yield break;

        // 5초간 깜빡임
        float duration = 5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            // 페이드 아웃
            Color color = newRecordText.color;
            color.a = 0.3f;
            newRecordText.color = color;

            yield return new WaitForSecondsRealtime(0.5f);

            // 페이드 인
            color.a = 1f;
            newRecordText.color = color;

            yield return new WaitForSecondsRealtime(0.5f);

            elapsed += 1f;
        }

        // 최종적으로 완전히 보이게
        Color finalColor = newRecordText.color;
        finalColor.a = 1f;
        newRecordText.color = finalColor;
    }

    private string FormatPlayTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60f);

        if (minutes > 0)
        {
            return string.Format("{0}:{1:00}", minutes, seconds);
        }
        else
        {
            return string.Format("{0}s", seconds);
        }
    }

    // 디버그용 메서드들
    [ContextMenu("Test Show GameOver Panel")]
    public void TestShowGameOverPanel()
    {
        if (Application.isPlaying)
        {
            ShowGameOverPanel();
        }
    }

    [ContextMenu("Check UI References")]
    public void CheckUIReferences()
    {
        Debug.Log("=== UI Reference Check ===");
        Debug.Log($"gameOverCanvasGroup: {(gameOverCanvasGroup != null ? "" : " NULL")}");
        Debug.Log($"gameOverText: {(gameOverText != null ? "" : " NULL")}");
        Debug.Log($"finalScoreText: {(finalScoreText != null ? "" : " NULL")}");
        Debug.Log($"playTimeText: {(playTimeText != null ? "" : " NULL")}");
        Debug.Log($"highScoreText: {(highScoreText != null ? "" : " NULL")}");
        Debug.Log($"newRecordText: {(newRecordText != null ? "" : " NULL")}");

        // 위치 정보도 확인
        if (finalScoreText != null)
        {
            Debug.Log($"finalScoreText position: {finalScoreText.transform.position}");
            Debug.Log($"finalScoreText active: {finalScoreText.gameObject.activeInHierarchy}");
            Debug.Log($"finalScoreText text: '{finalScoreText.text}'");
        }
    }

    [ContextMenu("Force Update Stats")]
    public void ForceUpdateStats()
    {
        if (Application.isPlaying)
        {
            UpdateGameStats();
            Debug.Log("Stats forcefully updated");
        }
    }

    public void RestartGame()
    {
        if (enableDebugLogs)
        {
            Debug.Log("GameOverManager: Restart button clicked");
        }

        // 모든 코루틴 정지
        StopAllCoroutines();

        Time.timeScale = 1f;
        string currentScene = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentScene);
    }

    public void ReturnToMainMenu()
    {
        if (enableDebugLogs)
        {
            Debug.Log("GameOverManager: Home button clicked");
        }

        // 모든 코루틴 정지
        StopAllCoroutines();

        Time.timeScale = 1f;
        SceneManager.LoadScene("StartScene");
    }
}
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("게임 상태")]
    public bool isGameActive = false;
    public int score = 0;
    public float gameSpeed = 5f;
    public int collectedItems = 0;

    [Header("UI 요소")]
    public TextMeshProUGUI scoreText;
    public GameOverManager gameOverManager;  // 연결된 GameOverManager 참조
    public AudioSource bgm;                  // 배경음악 추가 (Inspector 연결)

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        StartGame();
    }

    public void StartGame()
    {
        isGameActive = true;
        score = 0;
        collectedItems = 0;
        UpdateScoreUI();

        if (gameOverManager != null && gameOverManager.gameOverCanvasGroup != null)
        {
            gameOverManager.gameOverCanvasGroup.alpha = 0;
            gameOverManager.gameOverCanvasGroup.interactable = false;
            gameOverManager.gameOverCanvasGroup.blocksRaycasts = false;
        }

        Time.timeScale = 1f;
        if (bgm != null)
        {
            bgm.pitch = 1f;
            bgm.volume = 1f;
            bgm.Play();
        }
    }

    public void GameOver()
    {
        isGameActive = false;
        gameOverManager?.ShowGameOverPanel();

        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        if (score > highScore)
        {
            PlayerPrefs.SetInt("HighScore", score);
            PlayerPrefs.Save();
        }

        // 게임오버 연출 (TimeScale & BGM 느려짐)
        if (bgm != null)
        {
            StartCoroutine(SlowDownTimeAndMusic());
        }
    }

    IEnumerator SlowDownTimeAndMusic()
    {
        float t = 0f;
        float duration = 3f;
        float startPitch = bgm.pitch;
        float startVolume = 0.5f;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float progress = t / duration;
            float slowTime = Mathf.Lerp(1f, 0.1f, progress);
            float slowPitch = Mathf.Lerp(startPitch, 0.5f, progress);
            float slowVolume = Mathf.Lerp(startVolume, 0f, progress);

            Time.timeScale = slowTime;
            bgm.pitch = slowPitch;
            bgm.volume = slowVolume;

            yield return null;
        }

        Time.timeScale = 0f;
        bgm.Stop();
    }

    public void AddScore(int points)
    {
        if (!isGameActive) return;
        score += points;
        UpdateScoreUI();
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score;
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);  // 씬 재시작
    }

    public void GoToHome()
    {
        SceneManager.LoadScene("StartScene");  // StartScene 이름으로 홈화면 이동
    }
}

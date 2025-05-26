using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("���� ����")]
    public bool isGameActive = false;
    public int score = 0;
    public float gameSpeed = 5f;
    public int collectedItems = 0;

    [Header("UI ���")]
    public TextMeshProUGUI scoreText;
    public GameOverManager gameOverManager;  // ����� GameOverManager ����
    public AudioSource bgm;                  // ������� �߰� (Inspector ����)

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

        // ���ӿ��� ���� (TimeScale & BGM ������)
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
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);  // �� �����
    }

    public void GoToHome()
    {
        SceneManager.LoadScene("StartScene");  // StartScene �̸����� Ȩȭ�� �̵�
    }
}

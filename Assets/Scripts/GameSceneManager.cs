using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameSceneManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI itemCountText;
    public GameObject pausePanel;

    [Header("Button References")]
    public Button pauseButton;
    public Button resumeButton;
    public Button restartButton;
    public Button mainMenuButton;

    [Header("Audio")]
    public AudioSource buttonAudioSource;
    public string backgroundMusicObjectName = "BackgroundMusic";

    // Game State
    private bool isPaused = false;
    private AudioSource backgroundMusicSource;

    void Start()
    {
        InitializeGame();
        SetupUI();
        SetupAudio();
    }

    void Update()
    {
        UpdateUI();
        HandleInput();
    }

    private void InitializeGame()
    {
        // GameManager Ȯ��
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager instance not found!");
            return;
        }

        // ���� ���� (�� ���� ȣ��)
        GameManager.Instance.StartGame();
    }

    private void SetupUI()
    {
        // �г� �ʱ� ����
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        // ��ư ������ ���� (null üũ ����)
        SetupButtonListeners();

        // �ʱ� UI ������Ʈ
        UpdateUI();
    }

    private void SetupButtonListeners()
    {
        if (pauseButton != null)
            pauseButton.onClick.AddListener(TogglePause);
        else
            Debug.LogWarning("Pause button not assigned!");

        if (resumeButton != null)
            resumeButton.onClick.AddListener(TogglePause);
        else
            Debug.LogWarning("Resume button not assigned!");

        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);
        else
            Debug.LogWarning("Restart button not assigned!");

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(GoToMainMenu);
        else
            Debug.LogWarning("Main menu button not assigned!");
    }

    private void SetupAudio()
    {
        // ��� ���� ����
        StartBackgroundMusic();
    }

    private void UpdateUI()
    {
        // ������ Ȱ��ȭ �����̰� �Ͻ������� �ƴ� ���� UI ������Ʈ
        if (GameManager.Instance != null && GameManager.Instance.isGameActive && !isPaused)
        {
            UpdateScoreUI();
            UpdateItemCountUI();
        }
    }

    private void HandleInput()
    {
        // ESC Ű �Ǵ� �ڷ� ���� ��ư ó��
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    void TogglePause()
    {
        if (GameManager.Instance == null || !GameManager.Instance.isGameActive) return;

        isPaused = !isPaused;

        // UI ���� ����
        if (pausePanel != null)
        {
            pausePanel.SetActive(isPaused);
        }

        // �ð� ����
        Time.timeScale = isPaused ? 0f : 1f;

        // ȿ���� ���
        PlayButtonSound();
    }

    void RestartGame()
    {
        // �ð��� ����� ����
        RestoreTimeAndAudio();

        // �� �����
        string currentScene = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentScene);
    }

    void GoToMainMenu()
    {
        // �ð��� ����� ����
        RestoreTimeAndAudio();

        // ���� �޴��� �̵�
        SceneManager.LoadScene("StartScene");
    }

    private void RestoreTimeAndAudio()
    {
        Time.timeScale = 1f;

        if (backgroundMusicSource != null)
        {
            backgroundMusicSource.pitch = 1f;
            backgroundMusicSource.volume = 1f;
        }
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null && GameManager.Instance != null)
        {
            scoreText.text = "����: " + GameManager.Instance.score.ToString();
        }
    }

    private void UpdateItemCountUI()
    {
        if (itemCountText != null && GameManager.Instance != null)
        {
            itemCountText.text = "������: " + GameManager.Instance.collectedItems.ToString();
        }
    }

    private void PlayButtonSound()
    {
        if (buttonAudioSource != null)
        {
            buttonAudioSource.Play();
        }
        else
        {
            // ������Ʈ���� AudioSource ã��
            AudioSource audioSource = GetComponent<AudioSource>();
            if (audioSource != null)
            {
                audioSource.Play();
            }
        }
    }

    private void StartBackgroundMusic()
    {
        // �̸����� ������� ������Ʈ ã��
        GameObject bgmObject = GameObject.Find(backgroundMusicObjectName);
        if (bgmObject != null)
        {
            backgroundMusicSource = bgmObject.GetComponent<AudioSource>();
            if (backgroundMusicSource != null && !backgroundMusicSource.isPlaying)
            {
                backgroundMusicSource.Play();
            }
        }
        else
        {
            Debug.LogWarning($"Background music object '{backgroundMusicObjectName}' not found!");
        }
    }

    // �ܺο��� ���ӿ����� ȣ���� �� ���
    public void TriggerGameOver()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }
        else
        {
            Debug.LogError("Cannot trigger game over - GameManager instance not found!");
        }
    }

    void OnDestroy()
    {
        // ��ư ������ ���� (�޸� ���� ����)
        if (pauseButton != null) pauseButton.onClick.RemoveAllListeners();
        if (resumeButton != null) resumeButton.onClick.RemoveAllListeners();
        if (restartButton != null) restartButton.onClick.RemoveAllListeners();
        if (mainMenuButton != null) mainMenuButton.onClick.RemoveAllListeners();
    }
}
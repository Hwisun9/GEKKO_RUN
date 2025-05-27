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
        // GameManager 확인
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager instance not found!");
            return;
        }

        // 게임 시작 (한 번만 호출)
        GameManager.Instance.StartGame();
    }

    private void SetupUI()
    {
        // 패널 초기 상태
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        // 버튼 리스너 설정 (null 체크 포함)
        SetupButtonListeners();

        // 초기 UI 업데이트
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
        // 배경 음악 설정
        StartBackgroundMusic();
    }

    private void UpdateUI()
    {
        // 게임이 활성화 상태이고 일시정지가 아닐 때만 UI 업데이트
        if (GameManager.Instance != null && GameManager.Instance.isGameActive && !isPaused)
        {
            UpdateScoreUI();
            UpdateItemCountUI();
        }
    }

    private void HandleInput()
    {
        // ESC 키 또는 뒤로 가기 버튼 처리
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    void TogglePause()
    {
        if (GameManager.Instance == null || !GameManager.Instance.isGameActive) return;

        isPaused = !isPaused;

        // UI 상태 변경
        if (pausePanel != null)
        {
            pausePanel.SetActive(isPaused);
        }

        // 시간 제어
        Time.timeScale = isPaused ? 0f : 1f;

        // 효과음 재생
        PlayButtonSound();
    }

    void RestartGame()
    {
        // 시간과 오디오 복원
        RestoreTimeAndAudio();

        // 씬 재시작
        string currentScene = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentScene);
    }

    void GoToMainMenu()
    {
        // 시간과 오디오 복원
        RestoreTimeAndAudio();

        // 메인 메뉴로 이동
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
            scoreText.text = "점수: " + GameManager.Instance.score.ToString();
        }
    }

    private void UpdateItemCountUI()
    {
        if (itemCountText != null && GameManager.Instance != null)
        {
            itemCountText.text = "아이템: " + GameManager.Instance.collectedItems.ToString();
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
            // 컴포넌트에서 AudioSource 찾기
            AudioSource audioSource = GetComponent<AudioSource>();
            if (audioSource != null)
            {
                audioSource.Play();
            }
        }
    }

    private void StartBackgroundMusic()
    {
        // 이름으로 배경음악 오브젝트 찾기
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

    // 외부에서 게임오버를 호출할 때 사용
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
        // 버튼 리스너 해제 (메모리 누수 방지)
        if (pauseButton != null) pauseButton.onClick.RemoveAllListeners();
        if (resumeButton != null) resumeButton.onClick.RemoveAllListeners();
        if (restartButton != null) restartButton.onClick.RemoveAllListeners();
        if (mainMenuButton != null) mainMenuButton.onClick.RemoveAllListeners();
    }
}
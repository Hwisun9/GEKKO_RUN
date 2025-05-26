using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameSceneManager : MonoBehaviour
{
    // 참조
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI itemCountText;
    public GameObject pausePanel;
    public Button pauseButton;
    public Button resumeButton;
    public Button restartButton;
    public Button mainMenuButton;

    // 게임 상태
    private bool isPaused = false;

    // 시작
    void Start()
    {
        // 게임 초기화
        GameManager.Instance.StartGame();

        // 패널 초기 상태
        pausePanel.SetActive(false);

        // 버튼 리스너 설정
        pauseButton.onClick.AddListener(TogglePause);
        resumeButton.onClick.AddListener(TogglePause);
        restartButton.onClick.AddListener(RestartGame);
        mainMenuButton.onClick.AddListener(GoToMainMenu);

        // 배경 음악 재생
        StartBackgroundMusic();

        // UI 초기화
        UpdateScoreUI();
        UpdateItemCountUI();

        //게임 시작
        GameManager.Instance.StartGame();
    }

    // 업데이트
    void Update()
    {
        // 게임이 활성화 상태일 때만 UI 업데이트
        if (GameManager.Instance.isGameActive && !isPaused)
        {
            UpdateScoreUI();
            UpdateItemCountUI();
        }

        // 뒤로 가기 버튼 처리 (안드로이드)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    // 일시정지 토글
    void TogglePause()
    {
        isPaused = !isPaused;
        pausePanel.SetActive(isPaused);

        // 게임 시간 제어
        Time.timeScale = isPaused ? 0f : 1f;

        // 효과음 재생
        PlayButtonSound();
    }

    // 게임 재시작
    void RestartGame()
    {
        // 일시정지 해제
        Time.timeScale = 1f;

        // 현재 씬 다시 로드
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // 메인 메뉴로 이동
    void GoToMainMenu()
    {
        // 일시정지 해제
        Time.timeScale = 1f;

        // 메인 메뉴 씬 로드
        SceneManager.LoadScene("StartScene");
    }

    // 점수 UI 업데이트
    void UpdateScoreUI()
    {
        scoreText.text = "점수: " + GameManager.Instance.score;
    }

    // 아이템 카운트 UI 업데이트
    void UpdateItemCountUI()
    {
        itemCountText.text = "아이템: " + GameManager.Instance.collectedItems;
    }

    // 버튼 사운드 재생
    void PlayButtonSound()
    {
        GetComponent<AudioSource>().Play();
    }

    // 배경 음악 재생
    void StartBackgroundMusic()
    {
        AudioSource bgm = GameObject.Find("BackgroundMusic").GetComponent<AudioSource>();
        if (bgm != null && !bgm.isPlaying)
        {
            bgm.Play();
        }
    }

    // 게임 오버 호출
    public void TriggerGameOver()
    {
        // 게임 오버 상태로 설정
        GameManager.Instance.GameOver();

        // 최고 점수 저장
        int currentHighScore = PlayerPrefs.GetInt("HighScore", 0);
        if (GameManager.Instance.score > currentHighScore)
        {
            PlayerPrefs.SetInt("HighScore", GameManager.Instance.score);
            PlayerPrefs.Save();
        }

        // 지연 후 게임 오버 씬으로 전환
        Invoke("LoadGameOverScene", 1.5f);
    }

    // 게임 오버 씬 로드
    void LoadGameOverScene()
    {
        SceneManager.LoadScene("GameOverScene");
    }
}
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameSceneManager : MonoBehaviour
{
    // ����
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI itemCountText;
    public GameObject pausePanel;
    public Button pauseButton;
    public Button resumeButton;
    public Button restartButton;
    public Button mainMenuButton;

    // ���� ����
    private bool isPaused = false;

    // ����
    void Start()
    {
        // ���� �ʱ�ȭ
        GameManager.Instance.StartGame();

        // �г� �ʱ� ����
        pausePanel.SetActive(false);

        // ��ư ������ ����
        pauseButton.onClick.AddListener(TogglePause);
        resumeButton.onClick.AddListener(TogglePause);
        restartButton.onClick.AddListener(RestartGame);
        mainMenuButton.onClick.AddListener(GoToMainMenu);

        // ��� ���� ���
        StartBackgroundMusic();

        // UI �ʱ�ȭ
        UpdateScoreUI();
        UpdateItemCountUI();

        //���� ����
        GameManager.Instance.StartGame();
    }

    // ������Ʈ
    void Update()
    {
        // ������ Ȱ��ȭ ������ ���� UI ������Ʈ
        if (GameManager.Instance.isGameActive && !isPaused)
        {
            UpdateScoreUI();
            UpdateItemCountUI();
        }

        // �ڷ� ���� ��ư ó�� (�ȵ���̵�)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    // �Ͻ����� ���
    void TogglePause()
    {
        isPaused = !isPaused;
        pausePanel.SetActive(isPaused);

        // ���� �ð� ����
        Time.timeScale = isPaused ? 0f : 1f;

        // ȿ���� ���
        PlayButtonSound();
    }

    // ���� �����
    void RestartGame()
    {
        // �Ͻ����� ����
        Time.timeScale = 1f;

        // ���� �� �ٽ� �ε�
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // ���� �޴��� �̵�
    void GoToMainMenu()
    {
        // �Ͻ����� ����
        Time.timeScale = 1f;

        // ���� �޴� �� �ε�
        SceneManager.LoadScene("StartScene");
    }

    // ���� UI ������Ʈ
    void UpdateScoreUI()
    {
        scoreText.text = "����: " + GameManager.Instance.score;
    }

    // ������ ī��Ʈ UI ������Ʈ
    void UpdateItemCountUI()
    {
        itemCountText.text = "������: " + GameManager.Instance.collectedItems;
    }

    // ��ư ���� ���
    void PlayButtonSound()
    {
        GetComponent<AudioSource>().Play();
    }

    // ��� ���� ���
    void StartBackgroundMusic()
    {
        AudioSource bgm = GameObject.Find("BackgroundMusic").GetComponent<AudioSource>();
        if (bgm != null && !bgm.isPlaying)
        {
            bgm.Play();
        }
    }

    // ���� ���� ȣ��
    public void TriggerGameOver()
    {
        // ���� ���� ���·� ����
        GameManager.Instance.GameOver();

        // �ְ� ���� ����
        int currentHighScore = PlayerPrefs.GetInt("HighScore", 0);
        if (GameManager.Instance.score > currentHighScore)
        {
            PlayerPrefs.SetInt("HighScore", GameManager.Instance.score);
            PlayerPrefs.Save();
        }

        // ���� �� ���� ���� ������ ��ȯ
        Invoke("LoadGameOverScene", 1.5f);
    }

    // ���� ���� �� �ε�
    void LoadGameOverScene()
    {
        SceneManager.LoadScene("GameOverScene");
    }
}
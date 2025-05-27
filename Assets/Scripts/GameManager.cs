// GameManager.cs - 자석 이펙트 오류 수정
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game State")]
    public bool isGameActive = false;
    public int score = 0;
    public float gameSpeed = 5f;
    public int collectedItems = 0;
    public float playTime = 0f;

    [Header("Life System")]
    public int maxLives = 3;
    public int currentLives = 3;
    public TextMeshProUGUI livesText;

    [Header("Skills")]
    public float destroySkillCooldown = 30f;
    public float magnetDuration = 5f;
    public float magnetRange = 3f;
    private float destroySkillTimer = 0f;
    private bool isMagnetActive = false;
    private float magnetTimer = 0f;

    [Header("UI Elements")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI playTimeDisplayText;
    public UnityEngine.UI.Button destroySkillButton;
    public TextMeshProUGUI destroySkillCooldownText;

    [Header("Magnet Effect")]
    public MagnetEffectUI magnetEffectUI;  // MagnetEffectUI 컴포넌트 직접 참조
                                           // 또는 GameObject를 사용하려면: public GameObject magnetEffectObject;

    [Header("Combo UI Elements")]
    public GameObject comboPanel;
    public TextMeshProUGUI comboText;
    public TextMeshProUGUI comboScoreText;

    [Header("Managers")]
    public GameOverManager gameOverManager;

    [Header("Audio")]
    public AudioSource bgm;
    public AudioClip itemCollectSound;
    public AudioClip hitSound;
    public AudioClip skillActivateSound;
    public AudioSource sfxAudioSource;

    [Header("Game Over Settings")]
    public float gameOverSlowDownDuration = 3f;
    public float finalTimeScale = 0.1f;
    public float finalPitch = 0.5f;

    private bool isGameOverInProgress = false;
    private float gameStartTime;
    private Coroutine gameOverCoroutine;
    private Transform playerTransform;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("Multiple GameManager instances detected. Destroying duplicate.");
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        InitializeGame();
    }

    void Update()
    {
        if (isGameActive && !isGameOverInProgress)
        {
            UpdatePlayTime();
            UpdateSkills();
            UpdateMagnetEffect();
        }
    }

    void OnDestroy()
    {
        if (gameOverCoroutine != null)
        {
            StopCoroutine(gameOverCoroutine);
            gameOverCoroutine = null;
        }

        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void InitializeGame()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        if (destroySkillButton != null)
        {
            destroySkillButton.onClick.AddListener(UseDestroySkill);
        }

        StartGame();
    }

    public void StartGame()
    {
        if (gameOverCoroutine != null)
        {
            StopCoroutine(gameOverCoroutine);
            gameOverCoroutine = null;
        }

        isGameActive = true;
        isGameOverInProgress = false;
        score = 0;
        collectedItems = 0;
        playTime = 0f;
        currentLives = maxLives;
        destroySkillTimer = 0f;
        isMagnetActive = false;
        magnetTimer = 0f;
        gameStartTime = Time.realtimeSinceStartup;

        UpdateAllUI();
        HideGameOverPanel();
        ShowComboUI(true);
        ResetTimeAndAudio();

        Debug.Log("Game Started - All systems initialized");
    }

    private void UpdatePlayTime()
    {
        playTime = Time.realtimeSinceStartup - gameStartTime;
        UpdatePlayTimeUI();
    }

    private void UpdateSkills()
    {
        if (destroySkillTimer > 0)
        {
            destroySkillTimer -= Time.deltaTime;
            UpdateDestroySkillUI();
        }
    }

    private void UpdateMagnetEffect()
    {
        if (!isMagnetActive) return;

        magnetTimer -= Time.deltaTime;

        if (playerTransform != null)
        {
            GameObject[] items = GameObject.FindGameObjectsWithTag("Item");
            foreach (GameObject item in items)
            {
                float distance = Vector2.Distance(playerTransform.position, item.transform.position);
                if (distance <= magnetRange)
                {
                    Vector2 direction = (playerTransform.position - item.transform.position).normalized;
                    item.transform.Translate(direction * 8f * Time.deltaTime);
                }
            }
        }

        if (magnetTimer <= 0)
        {
            DeactivateMagnet();
        }
    }

    //  라이프 시스템
    public void TakeDamage()
    {
        if (!isGameActive || isGameOverInProgress) return;

        currentLives--;
        PlaySFX(hitSound);
        UpdateLivesUI();

        Debug.Log($"Lives remaining: {currentLives}");

        if (currentLives <= 0)
        {
            GameOver();
        }
        else
        {
            StartCoroutine(DamageEffect());
        }
    }

    private IEnumerator DamageEffect()
    {
        yield return new WaitForSeconds(0.1f);
        Debug.Log("Damage effect completed");
    }

    //  사운드 시스템
    public void PlayItemCollectSound()
    {
        PlaySFX(itemCollectSound);
    }

    private void PlaySFX(AudioClip clip)
    {
        if (sfxAudioSource != null && clip != null)
        {
            sfxAudioSource.PlayOneShot(clip);
        }
    }

    //  파괴 스킬
    public void UseDestroySkill()
    {
        if (destroySkillTimer > 0 || !isGameActive) return;

        PlaySFX(skillActivateSound);
        destroySkillTimer = destroySkillCooldown;

        GameObject[] obstacles = GameObject.FindGameObjectsWithTag("Obstacle");
        foreach (GameObject obstacle in obstacles)
        {
            CreateDestroyEffect(obstacle.transform.position);
            Destroy(obstacle);
        }

        Debug.Log($"Destroyed {obstacles.Length} obstacles!");
        UpdateDestroySkillUI();
    }

    private void CreateDestroyEffect(Vector3 position)
    {
        GameObject effect = new GameObject("DestroyEffect");
        effect.transform.position = position;
        Destroy(effect, 0.5f);
    }

    //  자석 아이템 - 수정된 버전
    public void ActivateMagnet()
    {
        if (isMagnetActive)
        {
            magnetTimer += magnetDuration;
        }
        else
        {
            isMagnetActive = true;
            magnetTimer = magnetDuration;

            // 자석 이펙트 UI 활성화 - 오류 수정
            if (magnetEffectUI != null)
            {
                magnetEffectUI.ActivateMagnetEffect();
            }
            else
            {
                Debug.LogWarning("MagnetEffectUI component not found!");
            }
        }

        PlaySFX(skillActivateSound);
        Debug.Log("Magnet activated!");
    }

    private void DeactivateMagnet()
    {
        isMagnetActive = false;
        magnetTimer = 0f;

        // 자석 이펙트 UI 비활성화 - 오류 수정
        if (magnetEffectUI != null)
        {
            magnetEffectUI.DeactivateMagnetEffect();
        }

        Debug.Log("Magnet deactivated");
    }

    // UI 업데이트 메서드들
    private void UpdateAllUI()
    {
        UpdateScoreUI();
        UpdatePlayTimeUI();
        UpdateLivesUI();
        UpdateDestroySkillUI();
    }

    private void UpdatePlayTimeUI()
    {
        if (playTimeDisplayText != null)
        {
            try
            {
                playTimeDisplayText.text = "Time: " + FormatTime(playTime);
            }
            catch (MissingReferenceException)
            {
                playTimeDisplayText = null;
            }
        }
    }

    private void UpdateLivesUI()
    {
        if (livesText != null)
        {
            livesText.text = "Lives: " + currentLives.ToString();
        }
    }

    private void UpdateDestroySkillUI()
    {
        if (destroySkillButton != null)
        {
            destroySkillButton.interactable = destroySkillTimer <= 0;
        }

        if (destroySkillCooldownText != null)
        {
            if (destroySkillTimer > 0)
            {
                destroySkillCooldownText.text = Mathf.Ceil(destroySkillTimer).ToString();
                destroySkillCooldownText.gameObject.SetActive(true);
            }
            else
            {
                destroySkillCooldownText.text = "READY";
                destroySkillCooldownText.gameObject.SetActive(false);
            }
        }
    }

    private bool IsUIDestroyed(TextMeshProUGUI uiElement)
    {
        return uiElement == null || uiElement.gameObject == null;
    }

    private string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    private void HideGameOverPanel()
    {
        if (gameOverManager?.gameOverCanvasGroup != null)
        {
            gameOverManager.gameOverCanvasGroup.alpha = 0;
            gameOverManager.gameOverCanvasGroup.interactable = false;
            gameOverManager.gameOverCanvasGroup.blocksRaycasts = false;
        }
    }

    private void ShowComboUI(bool show)
    {
        if (comboPanel != null)
        {
            comboPanel.SetActive(show);
        }

        if (comboText != null)
        {
            comboText.gameObject.SetActive(show);
        }

        if (comboScoreText != null)
        {
            comboScoreText.gameObject.SetActive(show);
        }
    }

    private void ResetTimeAndAudio()
    {
        Time.timeScale = 1f;

        if (bgm != null)
        {
            bgm.pitch = 1f;
            bgm.volume = 1f;
            if (!bgm.isPlaying)
            {
                bgm.Play();
            }
        }
    }

    public void GameOver()
    {
        if (!isGameActive || isGameOverInProgress) return;

        isGameActive = false;
        isGameOverInProgress = true;

        playTime = Time.realtimeSinceStartup - gameStartTime;

        ShowComboUI(false);

        // 자석 효과도 게임오버 시 비활성화
        if (isMagnetActive)
        {
            DeactivateMagnet();
        }

        Debug.Log($"Game Over - Final Score: {score}, Play Time: {FormatTime(playTime)}");

        SaveGameStats();

        if (gameOverManager != null)
        {
            gameOverManager.ShowGameOverPanel();
        }

        if (bgm != null)
        {
            gameOverCoroutine = StartCoroutine(SlowDownTimeAndMusic());
        }
        else
        {
            gameOverCoroutine = StartCoroutine(SlowDownTime());
        }
    }

    private void SaveGameStats()
    {
        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        if (score > highScore)
        {
            PlayerPrefs.SetInt("HighScore", score);
            PlayerPrefs.SetFloat("HighScoreTime", playTime);
        }

        float totalPlayTime = PlayerPrefs.GetFloat("TotalPlayTime", 0f);
        PlayerPrefs.SetFloat("TotalPlayTime", totalPlayTime + playTime);

        int playCount = PlayerPrefs.GetInt("PlayCount", 0);
        PlayerPrefs.SetInt("PlayCount", playCount + 1);

        PlayerPrefs.Save();
    }

    IEnumerator SlowDownTimeAndMusic()
    {
        float t = 0f;
        float startPitch = bgm.pitch;
        float startVolume = bgm.volume;

        while (t < gameOverSlowDownDuration)
        {
            if (!isGameOverInProgress || this == null)
                yield break;

            t += Time.unscaledDeltaTime;
            float progress = t / gameOverSlowDownDuration;
            float easedProgress = EaseOutQuad(progress);

            Time.timeScale = Mathf.Lerp(1f, finalTimeScale, easedProgress);

            if (bgm != null)
            {
                bgm.pitch = Mathf.Lerp(startPitch, finalPitch, easedProgress);
                bgm.volume = Mathf.Lerp(startVolume, 0f, easedProgress);
            }

            yield return null;
        }

        Time.timeScale = 0f;
        if (bgm != null)
        {
            bgm.Stop();
        }

        gameOverCoroutine = null;
    }

    IEnumerator SlowDownTime()
    {
        float t = 0f;

        while (t < gameOverSlowDownDuration)
        {
            if (!isGameOverInProgress || this == null)
                yield break;

            t += Time.unscaledDeltaTime;
            float progress = t / gameOverSlowDownDuration;
            float easedProgress = EaseOutQuad(progress);

            Time.timeScale = Mathf.Lerp(1f, finalTimeScale, easedProgress);

            yield return null;
        }

        Time.timeScale = 0f;
        gameOverCoroutine = null;
    }

    private float EaseOutQuad(float t)
    {
        return 1f - (1f - t) * (1f - t);
    }

    public void AddScore(int points)
    {
        if (!isGameActive || points < 0) return;

        score += points;
        UpdateScoreUI();
    }

    public void AddCollectedItem()
    {
        if (!isGameActive) return;

        collectedItems++;
        PlayItemCollectSound();
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null && !IsUIDestroyed(scoreText))
        {
            try
            {
                scoreText.text = "Score: " + score.ToString();
            }
            catch (MissingReferenceException)
            {
                scoreText = null;
            }
        }
    }

    public void RestartGame()
    {
        Debug.Log("Restarting Game...");

        if (gameOverCoroutine != null)
        {
            StopCoroutine(gameOverCoroutine);
            gameOverCoroutine = null;
        }

        Time.timeScale = 1f;
        isGameActive = false;
        isGameOverInProgress = false;

        if (bgm != null)
        {
            bgm.Stop();
            bgm.pitch = 1f;
            bgm.volume = 1f;
        }

        string currentScene = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentScene);
    }

    public void GoToHome()
    {
        Debug.Log("Going to Home...");

        if (gameOverCoroutine != null)
        {
            StopCoroutine(gameOverCoroutine);
            gameOverCoroutine = null;
        }

        Time.timeScale = 1f;
        isGameActive = false;
        isGameOverInProgress = false;

        if (bgm != null)
        {
            bgm.Stop();
            bgm.pitch = 1f;
            bgm.volume = 1f;
        }
        Time.timeScale = 1f;
        SceneManager.LoadScene("StartScene");
    }

    // 공개 메서드들
    public string GetFormattedPlayTime() => FormatTime(playTime);
    public float GetHighScoreTime() => PlayerPrefs.GetFloat("HighScoreTime", 0f);
    public float GetTotalPlayTime() => PlayerPrefs.GetFloat("TotalPlayTime", 0f);
    public int GetPlayCount() => PlayerPrefs.GetInt("PlayCount", 0);
    public void HideComboUI() => ShowComboUI(false);
    public void ShowComboUIElements() => ShowComboUI(true);
    public bool IsDestroySkillReady() => destroySkillTimer <= 0;
    public bool IsMagnetActive() => isMagnetActive;
    public float GetMagnetTimeRemaining() => isMagnetActive ? magnetTimer : 0f;
}
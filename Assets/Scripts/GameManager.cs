using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
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
    public TextMeshProUGUI livesText; // 텍스트 목숨 표시용 (이제 사용하지 않음)
    public HeartUISystem heartUI; // 새로운 하트 UI 시스템 참조

    [Header("Skills")]
    public float destroySkillCooldown = 30f;
    public float magnetDuration = 5f;
    public float magnetRange = 3f;
    private float destroySkillTimer = 0f;
    private bool isMagnetActive = false;
    private float magnetTimer = 0f;

    [Header("Input Settings")]
    public KeyCode destroySkillKey = KeyCode.Space;
    public bool enableKeyboardInput = true;

    [Header("UI Elements")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI playTimeDisplayText;
    public UnityEngine.UI.Button destroySkillButton;
    public TextMeshProUGUI destroySkillCooldownText;

    [Header("Flash Effect")]
    public Image flashPanel;
    public Color flashColor = Color.white;
    public float flashDuration = 0.05f;

    [Header("Magnet Effect")]
    public MagnetEffectUI magnetEffectUI;

    [Header("Buff Effects")]
    public float shrinkDuration = 8f;
    public float shrinkScale = 0.1f;
    public float hideDuration = 6f;
    public float hideAlpha = 0.2f;

    [Header("Animation Settings")]
    public float shrinkAnimationDuration = 0.5f;
    public float expandAnimationDuration = 0.5f;
    public float warningBlinkDuration = 1f;
    public int warningBlinkCount = 6;

    // 버프 상태 변수들
    private bool isShrinkActive = false;
    private float shrinkTimer = 0f;
    private bool isHideActive = false;
    private float hideTimer = 0f;

    // 플레이어 관련 변수
    private Transform playerTransform;
    private SpriteRenderer playerSpriteRenderer;
    private Collider2D playerCollider;
    private Vector3 originalPlayerScale;
    private Color originalPlayerColor;

    // 애니메이션 변수
    private Coroutine shrinkAnimationCoroutine;
    private Coroutine hideAnimationCoroutine;
    private Coroutine boosterAnimationCoroutine;

    // 버프 UI 관련
    [Header("Buff UI")]
    public GameObject shrinkBuffUI;
    public GameObject hideBuffUI;
    public TextMeshProUGUI shrinkTimerText;
    public TextMeshProUGUI hideTimerText;

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
    public AudioClip magnetSound;
    public AudioClip mushroomSound;
    public AudioClip hidePotionSound;

    [Header("Game Over Settings")]
    public float gameOverSlowDownDuration = 3f;
    public float finalTimeScale = 0.1f;
    public float finalPitch = 0.5f;

    private bool isGameOverInProgress = false;
    private float gameStartTime;
    private Coroutine gameOverCoroutine;


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
        InitializeFlashEffect();
    }

    void Update()
    {
        if (isGameActive && !isGameOverInProgress)
        {
            UpdatePlayTime();
            UpdateSkills();
            UpdateMagnetEffect();
            UpdateShrinkEffect();
            UpdateHideEffect();
            HandleInput();
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
            playerSpriteRenderer = player.GetComponent<SpriteRenderer>();
            playerCollider = player.GetComponent<Collider2D>();
            originalPlayerScale = playerTransform.localScale;
        }

        if (destroySkillButton != null)
        {
            destroySkillButton.onClick.AddListener(UseDestroySkill);
        }

        if (playerSpriteRenderer != null)
        {
            originalPlayerColor = playerSpriteRenderer.color;
        }

        // 하트 UI 시스템 초기화
        InitializeHeartUI();

        StartGame();
    }
    
    // 하트 UI 시스템 초기화
    private void InitializeHeartUI()
    {
        // 하트 UI 시스템이 할당되어 있지 않으면 찾기
        if (heartUI == null)
        {
            heartUI = FindObjectOfType<HeartUISystem>();
            
            if (heartUI == null)
            {
                Debug.LogWarning("HeartUISystem not found! Lives will be displayed as text.");
            }
        }
    }

    // 플래시 효과 초기화
    private void InitializeFlashEffect()
    {
        if (flashPanel == null)
        {
            // 자동으로 플래시 패널 생성
            CreateFlashPanel();
        }
        else
        {
            // 기존 패널이 있으면 초기 설정
            SetupFlashPanel();
        }
    }

    // 플래시 패널 자동 생성
    private void CreateFlashPanel()
    {
        // Canvas 찾기
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning("Canvas not found! Flash effect will not work.");
            return;
        }

        // 플래시 패널 생성
        GameObject flashObject = new GameObject("FlashPanel");
        flashObject.transform.SetParent(canvas.transform, false);

        // Image 컴포넌트 추가
        flashPanel = flashObject.AddComponent<Image>();

        // RectTransform 설정 (전체 화면)
        RectTransform rectTransform = flashObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.anchoredPosition = Vector2.zero;

        SetupFlashPanel();

        Debug.Log("Flash panel created automatically");
    }

    // 플래시 패널 설정
    private void SetupFlashPanel()
    {
        if (flashPanel == null) return;

        // 색상 설정
        flashPanel.color = new Color(flashColor.r, flashColor.g, flashColor.b, 0f);

        // 맨앞 표시되도록 설정
        flashPanel.transform.SetAsLastSibling();

        // Raycast Target 비활성화 (클릭 관련 문제)
        flashPanel.raycastTarget = false;

        Debug.Log("Flash panel setup completed");
    }

    private void HandleInput()
    {
        if (!enableKeyboardInput) return;

        if (Input.GetKeyDown(destroySkillKey))
        {
            Debug.Log("Destroy skill key pressed!");
            UseDestroySkill();
        }
    }

    private void UpdateShrinkEffect()
    {
        if (!isShrinkActive) return;

        shrinkTimer -= Time.deltaTime;

        // UI 업데이트
        if (shrinkTimerText != null)
        {
            shrinkTimerText.text = Mathf.Ceil(shrinkTimer).ToString();
        }

        if (shrinkTimer <= 0)
        {
            DeactivateShrink();
        }
    }

    private void UpdateHideEffect()
    {
        if (!isHideActive) return;

        hideTimer -= Time.deltaTime;

        // UI 업데이트
        if (hideTimerText != null)
        {
            hideTimerText.text = Mathf.Ceil(hideTimer).ToString();
        }

        if (hideTimer <= 0)
        {
            DeactivateHide();
        }
    }

    // 작아지기 효과 활성화
    public void ActivateShrink()
    {
        Debug.Log("Shrink effect activated!");

        if (isShrinkActive)
        {
            // 이미 활성화된 경우 시간 연장
            shrinkTimer += shrinkDuration;
            Debug.Log("Shrink effect extended! New duration: " + shrinkTimer);
        }
        else
        {
            // 새로 활성화
            isShrinkActive = true;
            shrinkTimer = shrinkDuration;

            if (playerTransform != null)
            {
                playerTransform.localScale = originalPlayerScale * shrinkScale;
            }

            // UI 활성화
            if (shrinkBuffUI != null)
            {
                shrinkBuffUI.SetActive(true);
            }

            Debug.Log("Player shrunk to " + shrinkScale + " size for " + shrinkDuration + " seconds");
        }

        PlaySFX(skillActivateSound);

        // 플래시 효과 (푸른색으로)
        //TriggerFlashEffect(Color.cyan, 0.1f);
    }

    // 작아지기 효과 비활성화
    private void DeactivateShrink()
    {
        isShrinkActive = false;
        shrinkTimer = 0f;

        if (playerTransform != null)
        {
            playerTransform.localScale = originalPlayerScale;
        }

        // UI 비활성화
        if (shrinkBuffUI != null)
        {
            shrinkBuffUI.SetActive(false);
        }

        Debug.Log("Shrink effect ended - Player size restored");
    }

    // 투명화 효과 활성화
    public void ActivateHide()
    {
        Debug.Log("Hide effect activated!");

        if (isHideActive)
        {
            // 이미 활성화된 경우 시간 연장
            hideTimer += hideDuration;
            Debug.Log("Hide effect extended! New duration: " + hideTimer);
        }
        else
        {
            // 새로 활성화
            isHideActive = true;
            hideTimer = hideDuration;

            if (playerSpriteRenderer != null)
            {
                Color newColor = originalPlayerColor;
                newColor.a = hideAlpha;
                playerSpriteRenderer.color = newColor;
            }

            // 충돌 비활성화 (장애물과의 충돌만)
            if (playerCollider != null)
            {
                // 레이어 태그를 이용해서 장애물과의 충돌만 비활성화
                // 또는 PlayerController에서 충돌 처리를 구현해야 함
            }

            // UI 활성화
            if (hideBuffUI != null)
            {
                hideBuffUI.SetActive(true);
            }

            Debug.Log("Player hidden for " + hideDuration + " seconds");
        }

        PlaySFX(skillActivateSound);

        // 플래시 효과 (보라색으로)
        //TriggerFlashEffect(Color.magenta, 0.1f);
    }

    // 투명화 효과 비활성화
    private void DeactivateHide()
    {
        isHideActive = false;
        hideTimer = 0f;

        if (playerSpriteRenderer != null)
        {
            playerSpriteRenderer.color = originalPlayerColor;
        }

        // 충돌 활성화
        if (playerCollider != null)
        {
            // 충돌 복구
        }

        // UI 비활성화
        if (hideBuffUI != null)
        {
            hideBuffUI.SetActive(false);
        }

        Debug.Log("Hide effect ended - Player visible again");
    }

    public void StartGame()
    {
        // 진행 중인 게임오버 처리 중단
        StopGameOverProcess();

        // 게임 상태 초기화
        InitializeGameState();

        // 플레이어 능력 초기화
        ResetPlayerAbilities();

        // 버프 효과 초기화
        ResetBuffEffects();

        // 부스터 효과 초기화
        ResetBoosterEffects();

        // UI 및 오디오 설정
        SetupUIAndAudio();

        // 디버그 로그
        LogGameStartInfo();
    }

    private void StopGameOverProcess()
    {
        if (gameOverCoroutine != null)
        {
            StopCoroutine(gameOverCoroutine);
            gameOverCoroutine = null;
        }
    }

    private void InitializeGameState()
    {
        isGameActive = true;
        isGameOverInProgress = false;
        score = 0;
        collectedItems = 0;
        playTime = 0f;
        currentLives = maxLives;
        gameStartTime = Time.realtimeSinceStartup;
    }

    private void ResetPlayerAbilities()
    {
        // 스킬 초기화
        destroySkillTimer = 0f;

        // 자석 효과 초기화
        if (isMagnetActive)
        {
            DeactivateMagnet();
        }
        isMagnetActive = false;
        magnetTimer = 0f;
    }

    private void ResetBuffEffects()
    {
        // 작아지기 효과 초기화
        if (isShrinkActive)
        {
            DeactivateShrink();
        }

        // 투명화 효과 초기화
        if (isHideActive)
        {
            DeactivateHide();
        }
    }

    // 부스터 효과 초기화
    private void ResetBoosterEffects()
    {
        // 부스터 시스템 초기화
        if (BoosterSystem.Instance != null)
        {
            BoosterSystem.Instance.ResetBooster();
        }

        // 콤보 시스템 초기화
        if (ComboSystem.Instance != null)
        {
            ComboSystem.Instance.ResetCombo();
        }
    }

    private void SetupUIAndAudio()
    {
        // UI 업데이트 및 설정
        UpdateAllUI();
        HideGameOverPanel();
        ShowComboUI(true);

        // 타임스케일 및 오디오 설정
        ResetTimeAndAudio();
    }

    private void LogGameStartInfo()
    {
        Debug.Log($"Game Started - Lives: {currentLives}");

        if (enableKeyboardInput)
        {
            Debug.Log($"Press {destroySkillKey} to use destroy skill!");
        }
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
            int itemsAffected = 0;

            foreach (GameObject item in items)
            {
                float distance = Vector2.Distance(playerTransform.position, item.transform.position);
                if (distance <= magnetRange)
                {
                    Vector2 direction = (playerTransform.position - item.transform.position).normalized;
                    item.transform.Translate(direction * 8f * Time.deltaTime);
                    itemsAffected++;
                }
            }

            
            if (itemsAffected > 0 && Time.frameCount % 60 == 0)
            {
                Debug.Log("Magnet pulling " + itemsAffected + " items");
            }
        }

        if (magnetTimer <= 0)
        {
            DeactivateMagnet();
        }
    }

    public void TakeDamage()
    {
        if (!isGameActive || isGameOverInProgress) return;

        Debug.Log("TakeDamage - Before: Lives = " + currentLives);

        currentLives--;
        PlaySFX(hitSound);
        UpdateLivesUI();

        Debug.Log("TakeDamage - After: Lives = " + currentLives);

        if (currentLives <= 0)
        {
            Debug.Log("Game Over triggered");
            GameOver();
        }
        else
        {
            Debug.Log("Lives remaining: " + currentLives);
            StartCoroutine(DamageEffect());
        }
    }

    private IEnumerator DamageEffect()
    {
        yield return new WaitForSeconds(0.5f);
        Debug.Log("Damage effect completed");
    }

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

    // 파괴 스킬 - 플래시 효과 추가
    public void UseDestroySkill()
    {
        if (destroySkillTimer > 0 || !isGameActive)
        {
            if (destroySkillTimer > 0)
            {
                Debug.Log("Destroy skill on cooldown: " + destroySkillTimer.ToString("F1") + "s");
            }
            return;
        }

        Debug.Log("Using destroy skill with flash effect!");

        // 효과음 재생
        PlaySFX(skillActivateSound);

        // 쿨다운 설정
        destroySkillTimer = destroySkillCooldown;

        // 플래시 효과 실행
        StartCoroutine(FlashEffect());

        // 장애물 파괴
        GameObject[] obstacles = GameObject.FindGameObjectsWithTag("Obstacle");
        foreach (GameObject obstacle in obstacles)
        {
            CreateDestroyEffect(obstacle.transform.position);
            Destroy(obstacle);
        }

        Debug.Log("Destroyed " + obstacles.Length + " obstacles!");

        UpdateDestroySkillUI();
    }

    // 플래시 효과 코루틴
    private IEnumerator FlashEffect()
    {
        if (flashPanel == null)
        {
            Debug.LogWarning("Flash panel not found!");
            yield break;
        }

        Debug.Log("Flash effect started");

        // 플래시기능 (서서히 -> 서서히)
        float elapsed = 0f;
        float halfDuration = flashDuration * 0.5f;

        // 페이드 인
        while (elapsed < halfDuration)
        {
            elapsed += Time.unscaledDeltaTime; // unscaledDeltaTime 사용 (timeScale 영향 안받음)
            float alpha = Mathf.Lerp(0f, 1f, elapsed / halfDuration);
            flashPanel.color = new Color(flashColor.r, flashColor.g, flashColor.b, alpha);
            yield return null;
        }

        // 페이드 아웃
        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / halfDuration);
            flashPanel.color = new Color(flashColor.r, flashColor.g, flashColor.b, alpha);
            yield return null;
        }

        // 완전히 투명하게
        flashPanel.color = new Color(flashColor.r, flashColor.g, flashColor.b, 0f);

        Debug.Log("Flash effect completed");
    }

    // 범용적인 플래시 효과 메소드 (다른 기능에서 호출 가능)
    public void TriggerFlashEffect()
    {
        TriggerFlashEffect(flashColor, flashDuration);
    }

    public void TriggerFlashEffect(Color color, float duration)
    {
        StartCoroutine(CustomFlashEffect(color, duration));
    }

    private IEnumerator CustomFlashEffect(Color color, float duration)
    {
        if (flashPanel == null) yield break;

        float elapsed = 0f;
        float halfDuration = duration * 0.5f;

        // 페이드 인
        while (elapsed < halfDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsed / halfDuration);
            flashPanel.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }

        // 페이드 아웃
        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / halfDuration);
            flashPanel.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }

        flashPanel.color = new Color(color.r, color.g, color.b, 0f);
    }

    private void CreateDestroyEffect(Vector3 position)
    {
        GameObject effect = new GameObject("DestroyEffect");
        effect.transform.position = position;
        Destroy(effect, 0.5f);
    }

    public void ActivateMagnet()
    {
        Debug.Log("Magnet activated - Lives: " + currentLives);

        if (isMagnetActive)
        {
            magnetTimer += magnetDuration;
        }
        else
        {
            isMagnetActive = true;
            magnetTimer = magnetDuration;

            if (magnetEffectUI != null)
            {
                magnetEffectUI.ActivateMagnetEffect();
            }
        }

        PlaySFX(skillActivateSound);
    }

    private void DeactivateMagnet()
    {
        isMagnetActive = false;
        magnetTimer = 0f;

        if (magnetEffectUI != null)
        {
            magnetEffectUI.DeactivateMagnetEffect();
        }

        Debug.Log("Magnet deactivated");
    }

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

        if (heartUI != null)
        {
            heartUI.UpdateHeartUI();
        }
 
        else if (livesText != null)
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
                if (enableKeyboardInput)
                {
                    destroySkillCooldownText.text = "READY (" + destroySkillKey + ")";
                }
                else
                {
                    destroySkillCooldownText.text = "READY";
                }
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
            bgm.volume = 0.7f;
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

        if (isMagnetActive)
        {
            DeactivateMagnet();
        }

        Debug.Log("Game Over - Score: " + score + ", Time: " + FormatTime(playTime));

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

        SceneManager.LoadScene("StartScene");
    }

    // 유틸 메소드들
    public string GetFormattedPlayTime() => FormatTime(playTime);
    public float GetHighScoreTime() => PlayerPrefs.GetFloat("HighScoreTime", 0f);
    public float GetTotalPlayTime() => PlayerPrefs.GetFloat("TotalPlayTime", 0f);
    public int GetPlayCount() => PlayerPrefs.GetInt("PlayCount", 0);
    public void HideComboUI() => ShowComboUI(false);
    public void ShowComboUIElements() => ShowComboUI(true);
    public bool IsDestroySkillReady() => destroySkillTimer <= 0;
    public bool IsMagnetActive() => isMagnetActive;
    public float GetMagnetTimeRemaining() => isMagnetActive ? magnetTimer : 0f;
    // 버프 상태 확인 메소드들
    public bool IsShrinkActive() => isShrinkActive;
    public bool IsHideActive() => isHideActive;
    public float GetShrinkTimeRemaining() => isShrinkActive ? shrinkTimer : 0f;
    public float GetHideTimeRemaining() => isHideActive ? hideTimer : 0f;

    public void SetKeyboardInputEnabled(bool enabled)
    {
        enableKeyboardInput = enabled;
    }

    public void ChangeDestroySkillKey(KeyCode newKey)
    {
        destroySkillKey = newKey;
        UpdateDestroySkillUI();
    }
}
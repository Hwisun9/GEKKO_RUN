// GameManager.cs - ȭ�� ��½�� ȿ�� �߰�
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
    public TextMeshProUGUI livesText;

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
    public Image flashPanel; // ��½�� ȿ���� �̹��� �г�
    public Color flashColor = Color.white; // ��½�� ����
    public float flashDuration = 0.05f; // ��½�� ���ӽð�

    [Header("Magnet Effect")]
    public MagnetEffectUI magnetEffectUI;

    [Header("Buff Effects")]
    public float shrinkDuration = 8f; // �۾����� ȿ�� ���ӽð�
    public float shrinkScale = 0.1f; // �۾����� ���� (0.6 = 60% ũ��)
    public float hideDuration = 6f; // ����ȭ ȿ�� ���ӽð�
    public float hideAlpha = 0.2f; // ���� (0.3 = 30% ������)

    [Header("Animation Settings")]
    public float shrinkAnimationDuration = 0.5f; // �۾����� �ִϸ��̼� �ð�
    public float expandAnimationDuration = 0.5f; // Ŀ���� �ִϸ��̼� �ð�
    public float warningBlinkDuration = 1f; // ��� ������ �ð�
    public int warningBlinkCount = 6; // ������ Ƚ��


    // ���� ���� ������
    private bool isShrinkActive = false;
    private float shrinkTimer = 0f;
    private bool isHideActive = false;
    private float hideTimer = 0f;

    // �÷��̾� ���� ����
    private Transform playerTransform;
    private SpriteRenderer playerSpriteRenderer;
    private Collider2D playerCollider;
    private Vector3 originalPlayerScale;
    private Color originalPlayerColor;

    // �ִϸ��̼� ����
    private Coroutine shrinkAnimationCoroutine;
    private Coroutine hideAnimationCoroutine;
    private Coroutine boosterAnimationCoroutine;

    // ���� UI ���� (���û���)
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

        StartGame();
    }

    // ��½�� ȿ�� �ʱ�ȭ
    private void InitializeFlashEffect()
    {
        if (flashPanel == null)
        {
            // �ڵ����� ��½�� �г� ����
            CreateFlashPanel();
        }
        else
        {
            // ���� �г��� ������ �ʱ� ����
            SetupFlashPanel();
        }
    }

    // ��½�� �г� �ڵ� ����
    private void CreateFlashPanel()
    {
        // Canvas ã��
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning("Canvas not found! Flash effect will not work.");
            return;
        }

        // ��½�� �г� ����
        GameObject flashObject = new GameObject("FlashPanel");
        flashObject.transform.SetParent(canvas.transform, false);

        // Image ������Ʈ �߰�
        flashPanel = flashObject.AddComponent<Image>();

        // RectTransform ���� (��ü ȭ��)
        RectTransform rectTransform = flashObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.anchoredPosition = Vector2.zero;

        SetupFlashPanel();

        Debug.Log("Flash panel created automatically");
    }

    // ��½�� �г� ����
    private void SetupFlashPanel()
    {
        if (flashPanel == null) return;

        // ���� ����
        flashPanel.color = new Color(flashColor.r, flashColor.g, flashColor.b, 0f);

        // ���� �տ� ǥ�õǵ��� ����
        flashPanel.transform.SetAsLastSibling();

        // Raycast Target ��Ȱ��ȭ (Ŭ�� ���� ����)
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

        // UI ������Ʈ
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

        // UI ������Ʈ
        if (hideTimerText != null)
        {
            hideTimerText.text = Mathf.Ceil(hideTimer).ToString();
        }

        if (hideTimer <= 0)
        {
            DeactivateHide();
        }
    }

    // �۾����� ȿ�� Ȱ��ȭ
    public void ActivateShrink()
    {
        Debug.Log("Shrink effect activated!");

        if (isShrinkActive)
        {
            // �̹� Ȱ��ȭ�� ��� �ð� ����
            shrinkTimer += shrinkDuration;
            Debug.Log("Shrink effect extended! New duration: " + shrinkTimer);
        }
        else
        {
            // ���� Ȱ��ȭ
            isShrinkActive = true;
            shrinkTimer = shrinkDuration;

            if (playerTransform != null)
            {
                playerTransform.localScale = originalPlayerScale * shrinkScale;
            }

            // UI Ȱ��ȭ
            if (shrinkBuffUI != null)
            {
                shrinkBuffUI.SetActive(true);
            }

            Debug.Log("Player shrunk to " + shrinkScale + " size for " + shrinkDuration + " seconds");
        }

        PlaySFX(skillActivateSound);

        // ��½�� ȿ�� (Ǫ��������)
        //TriggerFlashEffect(Color.cyan, 0.1f);
    }

    // �۾����� ȿ�� ��Ȱ��ȭ
    private void DeactivateShrink()
    {
        isShrinkActive = false;
        shrinkTimer = 0f;

        if (playerTransform != null)
        {
            playerTransform.localScale = originalPlayerScale;
        }

        // UI ��Ȱ��ȭ
        if (shrinkBuffUI != null)
        {
            shrinkBuffUI.SetActive(false);
        }

        Debug.Log("Shrink effect ended - Player size restored");
    }

    // ����ȭ ȿ�� Ȱ��ȭ
    public void ActivateHide()
    {
        Debug.Log("Hide effect activated!");

        if (isHideActive)
        {
            // �̹� Ȱ��ȭ�� ��� �ð� ����
            hideTimer += hideDuration;
            Debug.Log("Hide effect extended! New duration: " + hideTimer);
        }
        else
        {
            // ���� Ȱ��ȭ
            isHideActive = true;
            hideTimer = hideDuration;

            if (playerSpriteRenderer != null)
            {
                Color newColor = originalPlayerColor;
                newColor.a = hideAlpha;
                playerSpriteRenderer.color = newColor;
            }

            // �浹 ��Ȱ��ȭ (��ֹ����� �浹��)
            if (playerCollider != null)
            {
                // ���̾ �±׸� �̿��ؼ� ��ֹ����� �浹�� ��Ȱ��ȭ
                // �Ǵ� PlayerController���� �浹 ó���� �����ؾ� ��
            }

            // UI Ȱ��ȭ
            if (hideBuffUI != null)
            {
                hideBuffUI.SetActive(true);
            }

            Debug.Log("Player hidden for " + hideDuration + " seconds");
        }

        PlaySFX(skillActivateSound);

        // ��½�� ȿ�� (���������)
        //TriggerFlashEffect(Color.magenta, 0.1f);
    }

    // ����ȭ ȿ�� ��Ȱ��ȭ
    private void DeactivateHide()
    {
        isHideActive = false;
        hideTimer = 0f;

        if (playerSpriteRenderer != null)
        {
            playerSpriteRenderer.color = originalPlayerColor;
        }

        // �浹 Ȱ��ȭ
        if (playerCollider != null)
        {
            // �浹 ����
        }

        // UI ��Ȱ��ȭ
        if (hideBuffUI != null)
        {
            hideBuffUI.SetActive(false);
        }

        Debug.Log("Hide effect ended - Player visible again");
    }

    public void StartGame()
    {
        // ���� ���� ���ӿ��� ó�� �ߴ�
        StopGameOverProcess();

        // ���� ���� �ʱ�ȭ
        InitializeGameState();

        // �÷��̾� �ɷ� �ʱ�ȭ
        ResetPlayerAbilities();

        // ���� ȿ�� �ʱ�ȭ
        ResetBuffEffects();

        // �ν��� ȿ�� �ʱ�ȭ
        ResetBoosterEffects();

        // UI �� ����� ����
        SetupUIAndAudio();

        // ����� �α�
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
        // ��ų �ʱ�ȭ
        destroySkillTimer = 0f;

        // ���׳� ȿ�� �ʱ�ȭ
        if (isMagnetActive)
        {
            DeactivateMagnet();
        }
        isMagnetActive = false;
        magnetTimer = 0f;
    }

    private void ResetBuffEffects()
    {
        // �۾����� ȿ�� �ʱ�ȭ
        if (isShrinkActive)
        {
            DeactivateShrink();
        }

        // ����ȭ ȿ�� �ʱ�ȭ
        if (isHideActive)
        {
            DeactivateHide();
        }
    }

    //  �ν��� ȿ�� �ʱ�ȭ
    private void ResetBoosterEffects()
    {
        // �ν��� �ý��� �ʱ�ȭ
        if (BoosterSystem.Instance != null)
        {
            BoosterSystem.Instance.ResetBooster();
        }

        // �޺� �ý��� �ʱ�ȭ
        if (ComboSystem.Instance != null)
        {
            ComboSystem.Instance.ResetCombo();
        }
    }

    private void SetupUIAndAudio()
    {
        // UI ������Ʈ �� ����
        UpdateAllUI();
        HideGameOverPanel();
        ShowComboUI(true);

        // ����� �� �ð� ���� ����
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

            GameObject[] magnetItems = GameObject.FindGameObjectsWithTag("Magnet");
            foreach (GameObject magnetItem in magnetItems)
            {
                float distance = Vector2.Distance(playerTransform.position, magnetItem.transform.position);
                if (distance <= magnetRange)
                {
                    Vector2 direction = (playerTransform.position - magnetItem.transform.position).normalized;
                    magnetItem.transform.Translate(direction * 8f * Time.deltaTime);
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

    // �ı� ��ų - ��½�� ȿ�� �߰�
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

        // ȿ���� ���
        PlaySFX(skillActivateSound);

        // ��ٿ� ����
        destroySkillTimer = destroySkillCooldown;

        // ��½�� ȿ�� ����
        StartCoroutine(FlashEffect());

        // ��ֹ� �ı�
        GameObject[] obstacles = GameObject.FindGameObjectsWithTag("Obstacle");
        foreach (GameObject obstacle in obstacles)
        {
            CreateDestroyEffect(obstacle.transform.position);
            Destroy(obstacle);
        }

        Debug.Log("Destroyed " + obstacles.Length + " obstacles!");

        UpdateDestroySkillUI();
    }

    // ��½�� ȿ�� �ڷ�ƾ
    private IEnumerator FlashEffect()
    {
        if (flashPanel == null)
        {
            Debug.LogWarning("Flash panel not found!");
            yield break;
        }

        Debug.Log("Flash effect started");

        // ��½�̱� ���� (���� -> ������)
        float elapsed = 0f;
        float halfDuration = flashDuration * 0.5f;

        // ���̵� ��
        while (elapsed < halfDuration)
        {
            elapsed += Time.unscaledDeltaTime; // unscaledDeltaTime ��� (timeScale ���� �ȹ���)
            float alpha = Mathf.Lerp(0f, 1f, elapsed / halfDuration);
            flashPanel.color = new Color(flashColor.r, flashColor.g, flashColor.b, alpha);
            yield return null;
        }

        // ���̵� �ƿ�
        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / halfDuration);
            flashPanel.color = new Color(flashColor.r, flashColor.g, flashColor.b, alpha);
            yield return null;
        }

        // ������ �����ϰ�
        flashPanel.color = new Color(flashColor.r, flashColor.g, flashColor.b, 0f);

        Debug.Log("Flash effect completed");
    }

    // �������� ��½�� ȿ�� ���� (�ٸ� �������� ��� ����)
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

        // ���̵� ��
        while (elapsed < halfDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsed / halfDuration);
            flashPanel.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }

        // ���̵� �ƿ�
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

    // ���� �޼����
    public string GetFormattedPlayTime() => FormatTime(playTime);
    public float GetHighScoreTime() => PlayerPrefs.GetFloat("HighScoreTime", 0f);
    public float GetTotalPlayTime() => PlayerPrefs.GetFloat("TotalPlayTime", 0f);
    public int GetPlayCount() => PlayerPrefs.GetInt("PlayCount", 0);
    public void HideComboUI() => ShowComboUI(false);
    public void ShowComboUIElements() => ShowComboUI(true);
    public bool IsDestroySkillReady() => destroySkillTimer <= 0;
    public bool IsMagnetActive() => isMagnetActive;
    public float GetMagnetTimeRemaining() => isMagnetActive ? magnetTimer : 0f;
    // ���� ���� Ȯ�� �޼����
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
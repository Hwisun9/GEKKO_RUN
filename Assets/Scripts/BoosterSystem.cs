using System.Collections;
using UnityEngine;

public class BoosterSystem : MonoBehaviour
{
    [Header("�ν��� ����")]
    public float boosterDuration = 10f; // �ν��� ���� �ð�
    public float speedMultiplier = 3f; // �ӵ� ����

    [Header("�ð� ȿ��")]
    public Color boosterColor = Color.yellow; // �ν��� ����
    public float flashInterval = 0.5f; // ������ ����

    [Header("�����")]
    public AudioClip boosterStartSound;
    public AudioClip boosterEndSound;
    public AudioClip obstacleDestroySound;

    // �ν��� ����
    private bool isBoosterActive = false;
    private float boosterTimer = 0f;
    private Coroutine boosterCoroutine;

    // �ý��� ����
    private BackgroundRepeat[] backgrounds;
    private Spawner spawner;
    private PlayerController playerController;
    private GameManager gameManager;

    // ���� ���� ����
    private float[] originalBackgroundSpeeds;

    public static BoosterSystem Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        InitializeReferences();
    }

    void InitializeReferences()
    {
        // ���� ����
        backgrounds = FindObjectsOfType<BackgroundRepeat>();
        spawner = FindObjectOfType<Spawner>();
        playerController = FindObjectOfType<PlayerController>();
        gameManager = GameManager.Instance;

        // ���� ��� �ӵ� ����
        if (backgrounds != null && backgrounds.Length > 0)
        {
            originalBackgroundSpeeds = new float[backgrounds.Length];
            for (int i = 0; i < backgrounds.Length; i++)
            {
                originalBackgroundSpeeds[i] = backgrounds[i].baseScrollSpeed;
            }
        }

        Debug.Log($" BoosterSystem initialized - Found {backgrounds?.Length ?? 0} backgrounds");
    }

    void Update()
    {
        if (isBoosterActive)
        {
            boosterTimer -= Time.deltaTime;

            if (boosterTimer <= 0f)
            {
                DeactivateBooster();
            }
        }
    }

    // �ν��� Ȱ��ȭ
    public void ActivateBooster()
    {
        if (isBoosterActive)
        {
            // �̹� Ȱ��ȭ�Ǿ� �ִٸ� �ð� ����
            boosterTimer = boosterDuration;
            Debug.Log($" Booster time extended! Remaining: {boosterTimer:F1}s");
            return;
        }

        isBoosterActive = true;
        boosterTimer = boosterDuration;

        Debug.Log($" Booster activated! Duration: {boosterDuration}s");

        // ȿ�� ����
        ApplyBoosterEffects();

        // �ð� ȿ�� ����
        if (boosterCoroutine != null)
        {
            StopCoroutine(boosterCoroutine);
        }
        boosterCoroutine = StartCoroutine(BoosterVisualEffect());

        // ���� ���
        PlaySound(boosterStartSound);

        // �÷��̾� ���� ���� Ȱ��ȭ
        if (playerController != null)
        {
            playerController.SetInvincible(true);
        }
    }

    // �ν��� ��Ȱ��ȭ
    public void DeactivateBooster()
    {
        if (!isBoosterActive) return;

        isBoosterActive = false;
        boosterTimer = 0f;

        Debug.Log(" Booster deactivated!");

        // ȿ�� ����
        ResetBoosterEffects();

        // �ð� ȿ�� �ߴ�
        if (boosterCoroutine != null)
        {
            StopCoroutine(boosterCoroutine);
            boosterCoroutine = null;
        }

        // ���� ���
        PlaySound(boosterEndSound);

        // �÷��̾� ���� ���� ����
        if (playerController != null)
        {
            playerController.SetInvincible(false);
        }
    }

    // �ν��� ȿ�� ����
    void ApplyBoosterEffects()
    {
        // 1. ��� �ӵ� ����
        if (backgrounds != null)
        {
            for (int i = 0; i < backgrounds.Length; i++)
            {
                if (backgrounds[i] != null)
                {
                    backgrounds[i].baseScrollSpeed = originalBackgroundSpeeds[i] * speedMultiplier;
                }
            }
        }

        // 2. ������ �ӵ� ���� (�̹� ������ ������Ʈ��)
        ApplySpeedToExistingObjects();

        Debug.Log($" Booster effects applied - Speed multiplier: {speedMultiplier}x");
    }

    // �ν��� ȿ�� ����
    void ResetBoosterEffects()
    {
        // 1. ��� �ӵ� ����
        if (backgrounds != null && originalBackgroundSpeeds != null)
        {
            for (int i = 0; i < backgrounds.Length && i < originalBackgroundSpeeds.Length; i++)
            {
                if (backgrounds[i] != null)
                {
                    backgrounds[i].baseScrollSpeed = originalBackgroundSpeeds[i];
                }
            }
        }

        // 2. ���� ������Ʈ �ӵ� ����
        ResetSpeedOfExistingObjects();

        Debug.Log(" Booster effects reset");
    }

    // ���� ������Ʈ�鿡 �ӵ� ����
    void ApplySpeedToExistingObjects()
    {
        MovingObject[] movingObjects = FindObjectsOfType<MovingObject>();
        foreach (var movingObj in movingObjects)
        {
            if (movingObj != null)
            {
                movingObj.speed *= speedMultiplier;

                // Rigidbody2D �ӵ��� ����
                Rigidbody2D rb = movingObj.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.linearVelocity *= speedMultiplier;
                }
            }
        }
    }

    // ���� ������Ʈ�� �ӵ� ����
    void ResetSpeedOfExistingObjects()
    {
        MovingObject[] movingObjects = FindObjectsOfType<MovingObject>();
        foreach (var movingObj in movingObjects)
        {
            if (movingObj != null)
            {
                movingObj.speed /= speedMultiplier;

                // Rigidbody2D �ӵ��� ����
                Rigidbody2D rb = movingObj.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.linearVelocity /= speedMultiplier;
                }
            }
        }
    }

    // �ν��� �ð� ȿ��
    IEnumerator BoosterVisualEffect()
    {
        while (isBoosterActive)
        {
            // ȭ�� ��½�� ȿ��
            if (gameManager != null)
            {
                gameManager.TriggerFlashEffect(boosterColor, 0.1f);
            }

            yield return new WaitForSeconds(flashInterval);
        }
    }

    // ��ֹ� �ı� (PlayerController���� ȣ���)
    public void DestroyObstacle(GameObject obstacle)
    {
        if (!isBoosterActive) return;

        Debug.Log($" Obstacle destroyed by booster: {obstacle.name}");

        // �ı� ȿ�� ����
        StartCoroutine(ObstacleDestroyEffect(obstacle));

        // ���� ���
        PlaySound(obstacleDestroySound);
    }

    // ��ֹ� �ı� ȿ��
    IEnumerator ObstacleDestroyEffect(GameObject obstacle)
    {
        if (obstacle == null) yield break;

        Vector3 originalPosition = obstacle.transform.position;
        Vector3 originalScale = obstacle.transform.localScale;

        // ȸ���ϸ鼭 ���ư��� ȿ��
        float duration = 1f;
        float elapsed = 0f;

        // ���� �������� ���ư� ����
        Vector3 flyDirection = new Vector3(
            Random.Range(-1f, 1f),
            Random.Range(0.5f, 1f),
            0
        ).normalized;

        while (elapsed < duration && obstacle != null)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;

            // ȸ��
            obstacle.transform.Rotate(0, 0, 720 * Time.deltaTime);

            // ���ư���
            obstacle.transform.position = originalPosition + flyDirection * progress * 5f;

            // ũ�� ���
            float scale = Mathf.Lerp(1f, 0f, progress);
            obstacle.transform.localScale = originalScale * scale;

            yield return null;
        }

        // ������Ʈ ����
        if (obstacle != null)
        {
            Destroy(obstacle);
        }
    }

    // ���� ���
    void PlaySound(AudioClip clip)
    {
        if (clip != null && gameManager != null && gameManager.sfxAudioSource != null)
        {
            gameManager.sfxAudioSource.PlayOneShot(clip);
        }
    }

    // ���� �޼����
    public bool IsBoosterActive() => isBoosterActive;
    public float GetBoosterTimeRemaining() => isBoosterActive ? boosterTimer : 0f;
    public float GetBoosterTimeRatio() => isBoosterActive ? boosterTimer / boosterDuration : 0f;
    public float GetSpeedMultiplier() => speedMultiplier;

    // ���� ���� �� �ʱ�ȭ
    public void ResetBooster()
    {
        if (isBoosterActive)
        {
            DeactivateBooster();
        }
    }
}
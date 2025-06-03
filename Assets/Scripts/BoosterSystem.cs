using System.Collections;
using UnityEngine;

public class BoosterSystem : MonoBehaviour
{
    [Header("부스터 설정")]
    public float boosterDuration = 10f; // 부스터 지속 시간
    public float speedMultiplier = 3f; // 속도 배율

    [Header("시각 효과")]
    public Color boosterColor = Color.yellow; // 부스터 색상
    public float flashInterval = 0.5f; // 깜빡임 간격

    [Header("오디오")]
    public AudioClip boosterStartSound;
    public AudioClip boosterEndSound;
    public AudioClip obstacleDestroySound;

    // 부스터 상태
    private bool isBoosterActive = false;
    private float boosterTimer = 0f;
    private Coroutine boosterCoroutine;

    // 시스템 참조
    private BackgroundRepeat[] backgrounds;
    private Spawner spawner;
    private PlayerController playerController;
    private GameManager gameManager;

    // 원본 값들 저장
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
        // 참조 설정
        backgrounds = FindObjectsOfType<BackgroundRepeat>();
        spawner = FindObjectOfType<Spawner>();
        playerController = FindObjectOfType<PlayerController>();
        gameManager = GameManager.Instance;

        // 원본 배경 속도 저장
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

    // 부스터 활성화
    public void ActivateBooster()
    {
        if (isBoosterActive)
        {
            // 이미 활성화되어 있다면 시간 연장
            boosterTimer = boosterDuration;
            Debug.Log($" Booster time extended! Remaining: {boosterTimer:F1}s");
            return;
        }

        isBoosterActive = true;
        boosterTimer = boosterDuration;

        Debug.Log($" Booster activated! Duration: {boosterDuration}s");

        // 효과 적용
        ApplyBoosterEffects();

        // 시각 효과 시작
        if (boosterCoroutine != null)
        {
            StopCoroutine(boosterCoroutine);
        }
        boosterCoroutine = StartCoroutine(BoosterVisualEffect());

        // 사운드 재생
        PlaySound(boosterStartSound);

        // 플레이어 무적 상태 활성화
        if (playerController != null)
        {
            playerController.SetInvincible(true);
        }
    }

    // 부스터 비활성화
    public void DeactivateBooster()
    {
        if (!isBoosterActive) return;

        isBoosterActive = false;
        boosterTimer = 0f;

        Debug.Log(" Booster deactivated!");

        // 효과 해제
        ResetBoosterEffects();

        // 시각 효과 중단
        if (boosterCoroutine != null)
        {
            StopCoroutine(boosterCoroutine);
            boosterCoroutine = null;
        }

        // 사운드 재생
        PlaySound(boosterEndSound);

        // 플레이어 무적 상태 해제
        if (playerController != null)
        {
            playerController.SetInvincible(false);
        }
    }

    // 부스터 효과 적용
    void ApplyBoosterEffects()
    {
        // 1. 배경 속도 증가
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

        // 2. 스포너 속도 증가 (이미 생성된 오브젝트들)
        ApplySpeedToExistingObjects();

        Debug.Log($" Booster effects applied - Speed multiplier: {speedMultiplier}x");
    }

    // 부스터 효과 해제
    void ResetBoosterEffects()
    {
        // 1. 배경 속도 복원
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

        // 2. 기존 오브젝트 속도 복원
        ResetSpeedOfExistingObjects();

        Debug.Log(" Booster effects reset");
    }

    // 기존 오브젝트들에 속도 적용
    void ApplySpeedToExistingObjects()
    {
        MovingObject[] movingObjects = FindObjectsOfType<MovingObject>();
        foreach (var movingObj in movingObjects)
        {
            if (movingObj != null)
            {
                movingObj.speed *= speedMultiplier;

                // Rigidbody2D 속도도 조정
                Rigidbody2D rb = movingObj.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.linearVelocity *= speedMultiplier;
                }
            }
        }
    }

    // 기존 오브젝트들 속도 복원
    void ResetSpeedOfExistingObjects()
    {
        MovingObject[] movingObjects = FindObjectsOfType<MovingObject>();
        foreach (var movingObj in movingObjects)
        {
            if (movingObj != null)
            {
                movingObj.speed /= speedMultiplier;

                // Rigidbody2D 속도도 조정
                Rigidbody2D rb = movingObj.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.linearVelocity /= speedMultiplier;
                }
            }
        }
    }

    // 부스터 시각 효과
    IEnumerator BoosterVisualEffect()
    {
        while (isBoosterActive)
        {
            // 화면 번쩍임 효과
            if (gameManager != null)
            {
                gameManager.TriggerFlashEffect(boosterColor, 0.1f);
            }

            yield return new WaitForSeconds(flashInterval);
        }
    }

    // 장애물 파괴 (PlayerController에서 호출됨)
    public void DestroyObstacle(GameObject obstacle)
    {
        if (!isBoosterActive) return;

        Debug.Log($" Obstacle destroyed by booster: {obstacle.name}");

        // 파괴 효과 생성
        StartCoroutine(ObstacleDestroyEffect(obstacle));

        // 사운드 재생
        PlaySound(obstacleDestroySound);
    }

    // 장애물 파괴 효과
    IEnumerator ObstacleDestroyEffect(GameObject obstacle)
    {
        if (obstacle == null) yield break;

        Vector3 originalPosition = obstacle.transform.position;
        Vector3 originalScale = obstacle.transform.localScale;

        // 회전하면서 날아가는 효과
        float duration = 1f;
        float elapsed = 0f;

        // 랜덤 방향으로 날아갈 벡터
        Vector3 flyDirection = new Vector3(
            Random.Range(-1f, 1f),
            Random.Range(0.5f, 1f),
            0
        ).normalized;

        while (elapsed < duration && obstacle != null)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;

            // 회전
            obstacle.transform.Rotate(0, 0, 720 * Time.deltaTime);

            // 날아가기
            obstacle.transform.position = originalPosition + flyDirection * progress * 5f;

            // 크기 축소
            float scale = Mathf.Lerp(1f, 0f, progress);
            obstacle.transform.localScale = originalScale * scale;

            yield return null;
        }

        // 오브젝트 삭제
        if (obstacle != null)
        {
            Destroy(obstacle);
        }
    }

    // 사운드 재생
    void PlaySound(AudioClip clip)
    {
        if (clip != null && gameManager != null && gameManager.sfxAudioSource != null)
        {
            gameManager.sfxAudioSource.PlayOneShot(clip);
        }
    }

    // 공개 메서드들
    public bool IsBoosterActive() => isBoosterActive;
    public float GetBoosterTimeRemaining() => isBoosterActive ? boosterTimer : 0f;
    public float GetBoosterTimeRatio() => isBoosterActive ? boosterTimer / boosterDuration : 0f;
    public float GetSpeedMultiplier() => speedMultiplier;

    // 게임 시작 시 초기화
    public void ResetBooster()
    {
        if (isBoosterActive)
        {
            DeactivateBooster();
        }
    }
}
using System.Collections;
using UnityEngine;

public class BoosterSystem : MonoBehaviour
{
    [Header("부스터 설정")]
    public float boosterDuration = 10f; // 부스터 지속 시간
    public float speedMultiplier = 3f; // 속도 배율

    [Header("시각 효과")]
    //public Color boosterColor = Color.yellow; // 부스터 색상
    //public float flashInterval = 0.5f; // 깜빡임 간격
    
    [Header("장애물 파괴 효과")]
    //public Color obstacleDestroyColor = Color.red; // 장애물 파괴 색상
    //public float obstacleDestroyFlashDuration = 0.2f; // 장애물 파괴 플래시 지속시간
    //public float obstacleScoreMultiplier = 1.5f; // 장애물 파괴 시 점수 배율

    [Header("사운드")]
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

    // 배경 속도 저장
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

        // 배경 원본 속도 저장
        if (backgrounds != null && backgrounds.Length > 0)
        {
            originalBackgroundSpeeds = new float[backgrounds.Length];
            for (int i = 0; i < backgrounds.Length; i++)
            {
                originalBackgroundSpeeds[i] = backgrounds[i].baseScrollSpeed;
            }
        }

        Debug.Log($"BoosterSystem initialized - Found {backgrounds?.Length ?? 0} backgrounds");
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
        // 이미 활성화된 경우 아무 것도 하지 않음 (지속시간 연장하지 않음)
        if (isBoosterActive)
        {
            Debug.Log("Booster already active, ignoring activation request");
            return;
        }

        isBoosterActive = true;
        boosterTimer = boosterDuration;

        Debug.Log($"Booster activated! Duration: {boosterDuration}s");

        // 효과 적용
        ApplyBoosterEffects();

        // 시각 효과 부분 제거 - 기능만 남김
        if (boosterCoroutine != null)
        {
            StopCoroutine(boosterCoroutine);
            boosterCoroutine = null;
        }

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

        Debug.Log("Booster deactivated!");

        // 효과 리셋
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

        // 2. 기존의 속도 변경 (이미 생성된 오브젝트들)
        ApplySpeedToExistingObjects();

        Debug.Log($"Booster effects applied - Speed multiplier: {speedMultiplier}x");
    }

    // 부스터 효과 리셋
    void ResetBoosterEffects()
    {
        // 1. 배경 속도 리셋
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

        // 2. 기존 오브젝트 속도 리셋
        ResetSpeedOfExistingObjects();

        Debug.Log("Booster effects reset");
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

    // 기존 오브젝트의 속도 리셋
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
    //IEnumerator BoosterVisualEffect()
    //{
    //    while (isBoosterActive)
    //    {
    //        // 화면 플래시 효과
   //         if (gameManager != null)
    //        {
     //           gameManager.TriggerFlashEffect(boosterColor, 0.1f);
    //        }
//
   //         yield return new WaitForSeconds(flashInterval);
    //    }
  //  }

    // 장애물 파괴 (PlayerController에서 호출됨)
    public void DestroyObstacle(GameObject obstacle)
    {
        if (!isBoosterActive) return;

        Debug.Log($"Obstacle destroyed by booster: {obstacle.name}");

        // 파괴 효과 실행
        StartCoroutine(ObstacleDestroyEffect(obstacle));
        
        // 장애물 파괴 시 화면 효과
       // if (gameManager != null)
       // {
       //     gameManager.TriggerFlashEffect(obstacleDestroyColor, obstacleDestroyFlashDuration);
       // }

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

        // 랜덤 방향으로 날아갈 방향
        Vector3 flyDirection = new Vector3(
            Random.Range(-1f, 1f),
            Random.Range(0.5f, 1f),
            0
        ).normalized;

        // 추가 파티클 효과 생성 (간단한 별 파티클)
        CreateDestroyParticles(originalPosition);

        while (elapsed < duration && obstacle != null)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;

            // 회전
            obstacle.transform.Rotate(0, 0, 720 * Time.deltaTime);

            // 날아가기
            obstacle.transform.position = originalPosition + flyDirection * progress * 5f;

            // 크기 감소
            float scale = Mathf.Lerp(1f, 0f, progress);
            obstacle.transform.localScale = originalScale * scale;

            yield return null;
        }

        // 오브젝트 제거
        if (obstacle != null)
        {
            Destroy(obstacle);
        }
    }
    
    // 파괴 파티클 생성 (간단한 별 파티클)
    void CreateDestroyParticles(Vector3 position)
    {
        // 유니티 에디터에서 구현 필요 - 현재는 간단한 더미 구현
        // 실제 게임에서는 파티클 시스템을 사용하는 것이 좋음
        Debug.Log("Obstacle destroy particles created at " + position);
    }

    // 사운드 재생
    void PlaySound(AudioClip clip)
    {
        if (clip != null && gameManager != null && gameManager.sfxAudioSource != null)
        {
            gameManager.sfxAudioSource.PlayOneShot(clip);
        }
    }

    // 상태 메서드들
    public bool IsBoosterActive() => isBoosterActive;
    public float GetBoosterTimeRemaining() => isBoosterActive ? boosterTimer : 0f;
    public float GetBoosterTimeRatio() => isBoosterActive ? boosterTimer / boosterDuration : 0f;
    public float GetSpeedMultiplier() => speedMultiplier;

    // 리셋 메서드
    public void ResetBooster()
    {
        if (isBoosterActive)
        {
            DeactivateBooster();
        }
    }
}
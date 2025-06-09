using System.Collections;
using UnityEngine;

public class ShrinkEffect : MonoBehaviour
{
    // 설정값
    public float shrinkDuration = 8f;
    public float shrinkScale = 0.5f;
    public float shrinkAnimationDuration = 0.5f;
    public float expandAnimationDuration = 0.5f;

    // 내부 변수
    private bool isShrinkActive = false;
    private float shrinkTimer = 0f;
    private Vector3 originalScale;
    private Coroutine shrinkAnimationCoroutine;

    // UI 요소
    public GameObject shrinkBuffUI;
    public TMPro.TextMeshProUGUI shrinkTimerText;

    // 오디오
    public AudioClip shrinkSound;
    private AudioSource audioSource;

    void Start()
    {
        originalScale = transform.localScale;
        audioSource = GetComponent<AudioSource>();
        
        // UI 초기화
        if (shrinkBuffUI != null)
        {
            shrinkBuffUI.SetActive(false);
        }
    }

    void Update()
    {
        if (isShrinkActive)
        {
            shrinkTimer -= Time.deltaTime;

            // UI 업데이트
            if (shrinkTimerText != null)
            {
                shrinkTimerText.text = Mathf.Ceil(shrinkTimer).ToString();
            }

            // 효과 종료
            if (shrinkTimer <= 0)
            {
                DeactivateShrink();
            }
        }
    }

    // 효과 활성화
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

            // 단계적으로 서서히 줄어드는 애니메이션 적용
            if (shrinkAnimationCoroutine != null)
            {
                StopCoroutine(shrinkAnimationCoroutine);
            }
            
            // 새 애니메이션 시작 - 서서히 줄어듦
            shrinkAnimationCoroutine = StartCoroutine(ShrinkAnimation(originalScale, originalScale * shrinkScale, shrinkAnimationDuration));

            // UI 활성화
            if (shrinkBuffUI != null)
            {
                shrinkBuffUI.SetActive(true);
            }

            Debug.Log("Player shrinking to " + shrinkScale + " size for " + shrinkDuration + " seconds");
        }

        // 효과음 재생
        if (audioSource != null && shrinkSound != null)
        {
            audioSource.PlayOneShot(shrinkSound);
        }
    }

    // 효과 비활성화
    private void DeactivateShrink()
    {
        isShrinkActive = false;
        shrinkTimer = 0f;

        // 이전 애니메이션이 있다면 중지
        if (shrinkAnimationCoroutine != null)
        {
            StopCoroutine(shrinkAnimationCoroutine);
        }
        
        // 서서히 원래 크기로 돌아가는 애니메이션 시작
        shrinkAnimationCoroutine = StartCoroutine(ShrinkAnimation(transform.localScale, originalScale, expandAnimationDuration));

        // UI 비활성화
        if (shrinkBuffUI != null)
        {
            shrinkBuffUI.SetActive(false);
        }

        Debug.Log("Shrink effect ended - Player size restoring");
    }

    // 크기 변경 애니메이션 코루틴
    private IEnumerator ShrinkAnimation(Vector3 startScale, Vector3 targetScale, float duration)
    {
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            
            // Easing 함수 적용 (Ease-Out Cubic)
            float easedProgress = 1 - Mathf.Pow(1 - progress, 3);
            
            // 크기 변경 애니메이션
            transform.localScale = Vector3.Lerp(startScale, targetScale, easedProgress);
            
            yield return null;
        }
        
        // 최종 크기 설정
        transform.localScale = targetScale;
        shrinkAnimationCoroutine = null;
    }

    // 상태 확인 함수
    public bool IsShrinkActive() => isShrinkActive;
    public float GetRemainingTime() => shrinkTimer;
}

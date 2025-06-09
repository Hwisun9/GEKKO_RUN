using System.Collections;
using UnityEngine;

public class HideEffect : MonoBehaviour
{
    // 설정값
    public float hideDuration = 6f;
    public float hideAlpha = 0.2f;
    public float warningBlinkDuration = 1.5f;
    public int warningBlinkCount = 8;

    // 내부 변수
    private bool isHideActive = false;
    private float hideTimer = 0f;
    private Color originalColor;
    private SpriteRenderer spriteRenderer;
    private Collider2D playerCollider;
    private Coroutine hideAnimationCoroutine;

    // UI 요소
    public GameObject hideBuffUI;
    public TMPro.TextMeshProUGUI hideTimerText;

    // 오디오
    public AudioClip hideSound;
    private AudioSource audioSource;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerCollider = GetComponent<Collider2D>();
        audioSource = GetComponent<AudioSource>();
        
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        
        // UI 초기화
        if (hideBuffUI != null)
        {
            hideBuffUI.SetActive(false);
        }
    }

    void Update()
    {
        if (isHideActive)
        {
            hideTimer -= Time.deltaTime;

            // UI 업데이트
            if (hideTimerText != null)
            {
                hideTimerText.text = Mathf.Ceil(hideTimer).ToString();
            }

            // 효과 종료 경고 깜빡임 (남은 시간이 1.5초 이하일 때)
            if (hideTimer <= warningBlinkDuration && hideAnimationCoroutine == null)
            {
                hideAnimationCoroutine = StartCoroutine(BlinkWarningEffect());
            }

            // 효과 종료
            if (hideTimer <= 0)
            {
                DeactivateHide();
            }
        }
    }

    // 효과 활성화
    public void ActivateHide()
    {
        Debug.Log("Hide effect activated!");

        if (isHideActive)
        {
            // 이미 활성화된 경우 시간 연장
            hideTimer += hideDuration;
            Debug.Log("Hide effect extended! New duration: " + hideTimer);
            
            // 경고 깜빡임 중이면 중지
            if (hideAnimationCoroutine != null)
            {
                StopCoroutine(hideAnimationCoroutine);
                hideAnimationCoroutine = null;
                
                // 투명도 다시 설정
                if (spriteRenderer != null)
                {
                    Color newColor = originalColor;
                    newColor.a = hideAlpha;
                    spriteRenderer.color = newColor;
                }
            }
        }
        else
        {
            // 새로 활성화
            isHideActive = true;
            hideTimer = hideDuration;

            if (spriteRenderer != null)
            {
                Color newColor = originalColor;
                newColor.a = hideAlpha;
                spriteRenderer.color = newColor;
            }

            // UI 활성화
            if (hideBuffUI != null)
            {
                hideBuffUI.SetActive(true);
            }

            Debug.Log("Player hidden for " + hideDuration + " seconds");
        }

        // 효과음 재생
        if (audioSource != null && hideSound != null)
        {
            audioSource.PlayOneShot(hideSound);
        }
    }

    // 효과 비활성화
    private void DeactivateHide()
    {
        isHideActive = false;
        hideTimer = 0f;

        // 경고 깜빡임 중지
        if (hideAnimationCoroutine != null)
        {
            StopCoroutine(hideAnimationCoroutine);
            hideAnimationCoroutine = null;
        }

        // 원래 색상으로 복구
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }

        // UI 비활성화
        if (hideBuffUI != null)
        {
            hideBuffUI.SetActive(false);
        }

        Debug.Log("Hide effect ended - Player visible again");
    }

    // 경고 깜빡임 효과 코루틴
    private IEnumerator BlinkWarningEffect()
    {
        Debug.Log("Warning blink effect started!");
        
        float blinkInterval = warningBlinkDuration / (warningBlinkCount * 2);
        bool isVisible = true;
        
        // 반복 깜빡임
        for (int i = 0; i < warningBlinkCount; i++)
        {
            // 효과가 비활성화되면 중단
            if (!isHideActive)
                break;
                
            // 투명 <-> 약간 더 보이게 토글
            if (spriteRenderer != null)
            {
                Color color = originalColor;
                // 깜빡일 때는 약간 더 보이게
                color.a = isVisible ? hideAlpha : hideAlpha * 2.5f;
                spriteRenderer.color = color;
            }
            
            isVisible = !isVisible;
            yield return new WaitForSeconds(blinkInterval);
        }
        
        // 마지막 상태가 투명이 아니라면 투명하게 설정
        if (isHideActive && spriteRenderer != null && !isVisible)
        {
            Color color = originalColor;
            color.a = hideAlpha;
            spriteRenderer.color = color;
        }
        
        hideAnimationCoroutine = null;
    }

    // 상태 확인 함수
    public bool IsHideActive() => isHideActive;
    public float GetRemainingTime() => hideTimer;
}

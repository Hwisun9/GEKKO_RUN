using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PlayerEffects : MonoBehaviour
{
    // 효과 컴포넌트들
    private ShrinkEffect shrinkEffect;
    private HideEffect hideEffect;
    
    // 참조
    private GameManager gameManager;
    private SpriteRenderer spriteRenderer;
    
    void Awake()
    {
        // 필요한 컴포넌트 추가
        if (GetComponent<ShrinkEffect>() == null)
        {
            shrinkEffect = gameObject.AddComponent<ShrinkEffect>();
        }
        else
        {
            shrinkEffect = GetComponent<ShrinkEffect>();
        }
        
        if (GetComponent<HideEffect>() == null)
        {
            hideEffect = gameObject.AddComponent<HideEffect>();
        }
        else
        {
            hideEffect = GetComponent<HideEffect>();
        }
        
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    
    void Start()
    {
        gameManager = GameManager.Instance;
        
        // UI 요소 연결
        if (gameManager != null)
        {
            // Shrink 효과 설정
            shrinkEffect.shrinkDuration = gameManager.shrinkDuration;
            shrinkEffect.shrinkScale = gameManager.shrinkScale;
            shrinkEffect.shrinkAnimationDuration = gameManager.shrinkAnimationDuration;
            shrinkEffect.expandAnimationDuration = gameManager.expandAnimationDuration;
            shrinkEffect.shrinkBuffUI = gameManager.shrinkBuffUI;
            shrinkEffect.shrinkTimerText = gameManager.shrinkTimerText;
            
            // Hide 효과 설정
            hideEffect.hideDuration = gameManager.hideDuration;
            hideEffect.hideAlpha = gameManager.hideAlpha;
            hideEffect.warningBlinkDuration = gameManager.warningBlinkDuration;
            hideEffect.warningBlinkCount = gameManager.warningBlinkCount;
            hideEffect.hideBuffUI = gameManager.hideBuffUI;
            hideEffect.hideTimerText = gameManager.hideTimerText;
        }
    }
    
    // 외부에서 호출할 메소드들
    public void ActivateShrink()
    {
        shrinkEffect.ActivateShrink();
    }
    
    public void ActivateHide()
    {
        hideEffect.ActivateHide();
    }
    
    // 상태 확인 메소드들
    public bool IsShrinkActive() => shrinkEffect.IsShrinkActive();
    public bool IsHideActive() => hideEffect.IsHideActive();
    public float GetShrinkTimeRemaining() => shrinkEffect.GetRemainingTime();
    public float GetHideTimeRemaining() => hideEffect.GetRemainingTime();
}

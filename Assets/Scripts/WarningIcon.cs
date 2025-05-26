using UnityEngine;

public class WarningIcon : MonoBehaviour
{
    [Header("깜빡임 설정")]
    public float blinkSpeed = 4f;
    public float minAlpha = 0.1f;
    public float maxAlpha = 1f;
    public bool useColorChange = true;
    public Color warningColor = Color.red;
    public Color normalColor = Color.yellow;

    private SpriteRenderer spriteRenderer;
    private float timer = 0f;

    private Vector3 originalScale; // 초기 scale 저장

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("WarningIcon에 SpriteRenderer가 없습니다!");
            return;
        }

        if (useColorChange)
        {
            spriteRenderer.color = warningColor;
        }

        originalScale = transform.localScale; // scale 저장
    }

    void Update()
    {
        if (spriteRenderer == null) return;

        timer += Time.deltaTime;

        // 깜빡임
        float alpha = Mathf.Lerp(minAlpha, maxAlpha,
            (Mathf.Sin(timer * blinkSpeed) + 1f) / 2f);

        Color currentColor = spriteRenderer.color;
        if (useColorChange)
        {
            float colorLerp = (Mathf.Sin(timer * blinkSpeed * 0.7f) + 1f) / 2f;
            currentColor = Color.Lerp(warningColor, normalColor, colorLerp);
        }

        currentColor.a = alpha;
        spriteRenderer.color = currentColor;

        // 초기 scale 기준으로 변화 적용
        float scale = 1f + Mathf.Sin(timer * blinkSpeed * 1.2f) * 0.1f;
        transform.localScale = originalScale * scale;
    }
}

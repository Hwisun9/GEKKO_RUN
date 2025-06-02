using UnityEngine;
using UnityEngine.UI;

public class MagnetEffectUI : MonoBehaviour
{
    [Header("Effect Settings")]
    public GameObject magnetRingPrefab;      // 자석 링 이펙트 프리팹
    public float effectRadius = 1f;          // 이펙트 반경
    public float rotationSpeed = 20f;        // 회전 속도
    public float pulseSpeed = 2f;            // 맥동 속도
    public float minScale = 0.8f;            // 최소 크기
    public float maxScale = 1.0f;            // 최대 크기

    [Header("Visual Effects")]
    public Color magnetColor = Color.cyan;   // 자석 이펙트 색상
    public bool enableParticles = true;      // 파티클 효과 활성화
    public GameObject particleEffectPrefab;  // 파티클 이펙트 프리팹

    private Transform playerTransform;
    private GameObject currentMagnetRing;
    private GameObject currentParticleEffect;
    private float pulseTimer = 0f;
    private bool isActive = false;

    // 컴포넌트 참조
    private Image ringImage;
    private ParticleSystem particles;

    void Start()
    {
        // 플레이어 찾기
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        // 초기 상태는 비활성화
        gameObject.SetActive(false);
    }

    void Update()
    {
        if (!isActive || playerTransform == null) return;

        UpdatePosition();
        UpdateRotation();
        UpdatePulseEffect();
    }

    private void UpdatePosition()
    {
        // 플레이어 위치를 따라가기
        if (currentMagnetRing != null)
        {
            currentMagnetRing.transform.position = playerTransform.position;
        }

        if (currentParticleEffect != null)
        {
            currentParticleEffect.transform.position = playerTransform.position;
        }
    }

    private void UpdateRotation()
    {
        // 자석 링 회전
        if (currentMagnetRing != null)
        {
            currentMagnetRing.transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
        }
    }

    private void UpdatePulseEffect()
    {
        // 맥동 효과
        pulseTimer += Time.deltaTime * pulseSpeed;
        float scaleMultiplier = Mathf.Lerp(minScale, maxScale, (Mathf.Sin(pulseTimer) + 1f) / 2f);

        if (currentMagnetRing != null)
        {
            currentMagnetRing.transform.localScale = Vector3.one * scaleMultiplier;
        }
    }

    public void ActivateMagnetEffect()
    {
        if (playerTransform == null) return;

        isActive = true;
        gameObject.SetActive(true);

        CreateMagnetRing();

        if (enableParticles)
        {
            CreateParticleEffect();
        }

        Debug.Log("Magnet effect UI activated");
    }

    public void DeactivateMagnetEffect()
    {
        isActive = false;

        if (currentMagnetRing != null)
        {
            Destroy(currentMagnetRing);
            currentMagnetRing = null;
        }

        if (currentParticleEffect != null)
        {
            Destroy(currentParticleEffect);
            currentParticleEffect = null;
        }

        gameObject.SetActive(false);
        Debug.Log("Magnet effect UI deactivated");
    }

    private void CreateMagnetRing()
    {
        if (magnetRingPrefab != null)
        {
            // 프리팹으로 생성
            currentMagnetRing = Instantiate(magnetRingPrefab, playerTransform.position, Quaternion.identity);
        }


        // 크기 설정
        if (currentMagnetRing != null)
        {
            currentMagnetRing.transform.localScale = Vector3.one * effectRadius;

            // 색상 설정
            Image ringImg = currentMagnetRing.GetComponent<Image>();
            if (ringImg != null)
            {
                ringImg.color = magnetColor;
            }

            SpriteRenderer spriteRenderer = currentMagnetRing.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.color = magnetColor;
            }
        }
    }

  

    private void CreateParticleEffect()
    {
        if (particleEffectPrefab != null)
        {
            currentParticleEffect = Instantiate(particleEffectPrefab, playerTransform.position, Quaternion.identity);
        }
        else
        {
            CreateDefaultParticleEffect();
        }
    }

    private void CreateDefaultParticleEffect()
    {
        // 기본 파티클 효과 생성
        currentParticleEffect = new GameObject("MagnetParticles");
        currentParticleEffect.transform.position = playerTransform.position;

        ParticleSystem ps = currentParticleEffect.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startColor = magnetColor;
        main.startSize = 0.1f;
        main.startSpeed = 2f;
        main.maxParticles = 50;

        var emission = ps.emission;
        emission.rateOverTime = 20;

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = effectRadius;
    }

    // 외부에서 설정을 변경할 수 있는 메서드들
    public void SetEffectRadius(float radius)
    {
        effectRadius = radius;
        if (currentMagnetRing != null)
        {
            currentMagnetRing.transform.localScale = Vector3.one * radius;
        }
    }

    public void SetMagnetColor(Color color)
    {
        magnetColor = color;

        if (currentMagnetRing != null)
        {
            Image img = currentMagnetRing.GetComponent<Image>();
            if (img != null) img.color = color;

            SpriteRenderer sr = currentMagnetRing.GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = color;

            LineRenderer lr = currentMagnetRing.GetComponent<LineRenderer>();
            
        }
    }

    // 이펙트가 활성화되어 있는지 확인
    public bool IsActive()
    {
        return isActive;
    }
}
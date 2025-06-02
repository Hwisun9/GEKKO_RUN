using UnityEngine;
using UnityEngine.UI;

public class MagnetEffectUI : MonoBehaviour
{
    [Header("Effect Settings")]
    public GameObject magnetRingPrefab;      // �ڼ� �� ����Ʈ ������
    public float effectRadius = 1f;          // ����Ʈ �ݰ�
    public float rotationSpeed = 20f;        // ȸ�� �ӵ�
    public float pulseSpeed = 2f;            // �Ƶ� �ӵ�
    public float minScale = 0.8f;            // �ּ� ũ��
    public float maxScale = 1.0f;            // �ִ� ũ��

    [Header("Visual Effects")]
    public Color magnetColor = Color.cyan;   // �ڼ� ����Ʈ ����
    public bool enableParticles = true;      // ��ƼŬ ȿ�� Ȱ��ȭ
    public GameObject particleEffectPrefab;  // ��ƼŬ ����Ʈ ������

    private Transform playerTransform;
    private GameObject currentMagnetRing;
    private GameObject currentParticleEffect;
    private float pulseTimer = 0f;
    private bool isActive = false;

    // ������Ʈ ����
    private Image ringImage;
    private ParticleSystem particles;

    void Start()
    {
        // �÷��̾� ã��
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        // �ʱ� ���´� ��Ȱ��ȭ
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
        // �÷��̾� ��ġ�� ���󰡱�
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
        // �ڼ� �� ȸ��
        if (currentMagnetRing != null)
        {
            currentMagnetRing.transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
        }
    }

    private void UpdatePulseEffect()
    {
        // �Ƶ� ȿ��
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
            // ���������� ����
            currentMagnetRing = Instantiate(magnetRingPrefab, playerTransform.position, Quaternion.identity);
        }


        // ũ�� ����
        if (currentMagnetRing != null)
        {
            currentMagnetRing.transform.localScale = Vector3.one * effectRadius;

            // ���� ����
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
        // �⺻ ��ƼŬ ȿ�� ����
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

    // �ܺο��� ������ ������ �� �ִ� �޼����
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

    // ����Ʈ�� Ȱ��ȭ�Ǿ� �ִ��� Ȯ��
    public bool IsActive()
    {
        return isActive;
    }
}
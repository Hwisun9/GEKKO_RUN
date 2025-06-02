using UnityEngine;

public class ScoreItem : MonoBehaviour
{
    public int scoreValue = 10;
    public Color effectColor = Color.white;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($" Player collided with: {gameObject.name}, Tag: {gameObject.tag}");

            if (GameManager.Instance != null)
            {
                HandleItemCollection();
            }
            else
            {
                Debug.LogError(" GameManager.Instance is null!");
            }

            // CreateEffect();
            //Destroy(gameObject);
        }
    }

    void HandleItemCollection()
    {
        switch (gameObject.tag)
        {
            case "Item":
                HandleNormalItem();
                break;
            case "Obstacle":
                HandleObstacle();
                break;
            case "Magnet":
                HandleMagnetItem();
                break;
            case "Mushroom":
                HandleMushroomItem();
                break;
            case "Hide":
                HandleHideItem();
                break;
            default:
                Debug.LogWarning($" Unknown item tag: {gameObject.tag}");
                break;
        }
    }

    void HandleNormalItem()
    {
        Debug.Log(" Normal item collected!");

        // �޺� �ý��� ó��
        if (ComboSystem.Instance != null)
        {
            ComboSystem.Instance.OnItemCollected();
        }

        // ���� �� ������ ���� ó��
        GameManager.Instance.AddScore(scoreValue);
        GameManager.Instance.AddCollectedItem(); // ���� ��� ����
    }

    void HandleObstacle()
    {
        Debug.Log(" Obstacle hit!");
        // ��ֹ��� ScoreItem���� ó������ ���� (PlayerController���� ó��)
    }

    void HandleMagnetItem()
    {
        Debug.Log(" Magnet item collected!");

        // �ڼ� ȿ�� Ȱ��ȭ
        GameManager.Instance.ActivateMagnet();

        // �ڼ� �����۵� ���� ����
        GameManager.Instance.AddScore(scoreValue > 0 ? scoreValue : 25);
        GameManager.Instance.AddCollectedItem(); // ���� ���
    }

    void HandleMushroomItem()
    {
        Debug.Log(" Mushroom item collected!");

        // �۾����� ���� Ȱ��ȭ
        GameManager.Instance.ActivateShrink();

        // ���� ����
        GameManager.Instance.AddScore(scoreValue > 0 ? scoreValue : 30);
        GameManager.Instance.AddCollectedItem(); // ���� ���
    }

    void HandleHideItem()
    {
        Debug.Log(" Hide potion collected!");

        // ����ȭ ���� Ȱ��ȭ
        GameManager.Instance.ActivateHide();

        // ���� ���� (���� ���� ����)
        GameManager.Instance.AddScore(scoreValue > 0 ? scoreValue : 50);
        GameManager.Instance.AddCollectedItem(); // ���� ���
    }

    void CreateEffect()
    {
        GameObject effect = new GameObject($"Effect_{gameObject.tag}");
        effect.transform.position = transform.position;

        // ������ Ÿ�Կ� ���� �ٸ� ����Ʈ
        switch (gameObject.tag)
        {
            case "Magnet":
                CreateMagnetEffect(effect);
                break;
            case "Mushroom":
                CreateMushroomEffect(effect);
                break;
            case "Hide":
                CreateHideEffect(effect);
                break;
            default:
                CreateNormalEffect(effect);
                break;
        }

        // ����Ʈ �ڵ� ����
        Destroy(effect, 1.5f);
    }

    void CreateNormalEffect(GameObject effect)
    {
        StartCoroutine(ScaleEffect(effect.transform, Color.white, 1.5f, 0.5f));
    }

    void CreateMagnetEffect(GameObject effect)
    {
        Debug.Log(" Creating magnet collection effect!");
        StartCoroutine(ScaleEffectWithRotation(effect.transform, Color.blue, 2f, 0.8f));
    }

    void CreateMushroomEffect(GameObject effect)
    {
        Debug.Log(" Creating mushroom collection effect!");
        StartCoroutine(ScaleEffect(effect.transform, Color.green, 1.8f, 0.6f));
    }

    void CreateHideEffect(GameObject effect)
    {
        Debug.Log(" Creating hide potion collection effect!");
        StartCoroutine(ScaleEffectWithFade(effect.transform, Color.magenta, 2.2f, 1f));
    }

    // �⺻ ������ ����Ʈ
    System.Collections.IEnumerator ScaleEffect(Transform effectTransform, Color color, float maxScale, float duration)
    {
        Vector3 startScale = Vector3.zero;
        Vector3 endScale = Vector3.one * maxScale;
        float elapsed = 0f;

        // Ȯ��
        while (elapsed < duration * 0.5f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (duration * 0.5f);
            effectTransform.localScale = Vector3.Lerp(startScale, endScale, t);
            yield return null;
        }

        // ���
        elapsed = 0f;
        while (elapsed < duration * 0.5f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (duration * 0.5f);
            effectTransform.localScale = Vector3.Lerp(endScale, Vector3.zero, t);
            yield return null;
        }
    }

    // ȸ���� ���Ե� ������ ����Ʈ (�ڼ���)
    System.Collections.IEnumerator ScaleEffectWithRotation(Transform effectTransform, Color color, float maxScale, float duration)
    {
        Vector3 startScale = Vector3.zero;
        Vector3 endScale = Vector3.one * maxScale;
        float elapsed = 0f;

        // Ȯ�� + ȸ��
        while (elapsed < duration * 0.6f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (duration * 0.6f);
            effectTransform.localScale = Vector3.Lerp(startScale, endScale, t);
            effectTransform.Rotate(0, 0, 720 * Time.deltaTime); // ���� ȸ��
            yield return null;
        }

        // ��� + ȸ��
        elapsed = 0f;
        while (elapsed < duration * 0.4f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (duration * 0.4f);
            effectTransform.localScale = Vector3.Lerp(endScale, Vector3.zero, t);
            effectTransform.Rotate(0, 0, 360 * Time.deltaTime); // ���� ȸ��
            yield return null;
        }
    }

    // ���̵� ȿ���� ���Ե� ������ ����Ʈ (����ȭ ���ǿ�)
    System.Collections.IEnumerator ScaleEffectWithFade(Transform effectTransform, Color color, float maxScale, float duration)
    {
        Vector3 startScale = Vector3.zero;
        Vector3 endScale = Vector3.one * maxScale;
        float elapsed = 0f;

        // Ȯ��
        while (elapsed < duration * 0.4f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (duration * 0.4f);
            effectTransform.localScale = Vector3.Lerp(startScale, endScale, t);
            yield return null;
        }

        // ����
        elapsed = 0f;
        while (elapsed < duration * 0.2f)
        {
            elapsed += Time.deltaTime;
            // ������ ȿ��
            float alpha = Mathf.Sin(elapsed * 10f) * 0.5f + 0.5f;
            yield return null;
        }

        // ���
        elapsed = 0f;
        while (elapsed < duration * 0.4f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (duration * 0.4f);
            effectTransform.localScale = Vector3.Lerp(endScale, Vector3.zero, t);
            yield return null;
        }
    }
}

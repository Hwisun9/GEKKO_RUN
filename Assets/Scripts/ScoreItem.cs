using UnityEngine;

public class ScoreItem : MonoBehaviour
{
    public int scoreValue = 10;
    public Color effectColor = Color.white;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"Player collided with: {gameObject.name}, Tag: {gameObject.tag}");

            if (GameManager.Instance != null)
            {
                HandleItemCollection();
            }
            else
            {
                Debug.LogError("GameManager.Instance is null!");
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
                Debug.LogWarning($"Unknown item tag: {gameObject.tag}");
                break;
        }
    }

    void HandleNormalItem()
    {
        Debug.Log("Normal item collected!");

        // 콤보 시스템 처리는 PlayerController에서만 수행하도록 제거
        
        // 점수 및 아이템 획득 처리
        GameManager.Instance.AddScore(scoreValue);
        GameManager.Instance.AddCollectedItem(); // 아이템 획득 카운트
    }

    void HandleObstacle()
    {
        Debug.Log("Obstacle hit!");
        // 장애물은 ScoreItem에서 처리하지 않음 (PlayerController에서 처리)
    }

    void HandleMagnetItem()
    {
        Debug.Log("Magnet item collected!");

        // 자석 효과 활성화
        GameManager.Instance.ActivateMagnet();

        // 자석 아이템도 점수 추가
        GameManager.Instance.AddScore(scoreValue > 0 ? scoreValue : 25);
        GameManager.Instance.AddCollectedItem(); // 아이템 획득 카운트
    }

    void HandleMushroomItem()
    {
        Debug.Log("Mushroom item collected!");

        // 작아지기 효과 활성화
        GameManager.Instance.ActivateShrink();

        // 점수 추가
        GameManager.Instance.AddScore(scoreValue > 0 ? scoreValue : 30);
        GameManager.Instance.AddCollectedItem(); // 아이템 획득 카운트
    }

    void HandleHideItem()
    {
        Debug.Log("Hide potion collected!");

        // 투명화 효과 활성화
        GameManager.Instance.ActivateHide();

        // 점수 추가 (가장 높은 가치)
        GameManager.Instance.AddScore(scoreValue > 0 ? scoreValue : 50);
        GameManager.Instance.AddCollectedItem(); // 아이템 획득 카운트
    }

    void CreateEffect()
    {
        GameObject effect = new GameObject($"Effect_{gameObject.tag}");
        effect.transform.position = transform.position;

        // 아이템 타입에 따른 다른 이펙트
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

        // 이펙트 자동 삭제
        Destroy(effect, 1.5f);
    }

    void CreateNormalEffect(GameObject effect)
    {
        StartCoroutine(ScaleEffect(effect.transform, Color.white, 1.5f, 0.5f));
    }

    void CreateMagnetEffect(GameObject effect)
    {
        Debug.Log("Creating magnet collection effect!");
        StartCoroutine(ScaleEffectWithRotation(effect.transform, Color.blue, 2f, 0.8f));
    }

    void CreateMushroomEffect(GameObject effect)
    {
        Debug.Log("Creating mushroom collection effect!");
        StartCoroutine(ScaleEffect(effect.transform, Color.green, 1.8f, 0.6f));
    }

    void CreateHideEffect(GameObject effect)
    {
        Debug.Log("Creating hide potion collection effect!");
        StartCoroutine(ScaleEffectWithFade(effect.transform, Color.magenta, 2.2f, 1f));
    }

    // 기본 스케일 이펙트
    System.Collections.IEnumerator ScaleEffect(Transform effectTransform, Color color, float maxScale, float duration)
    {
        Vector3 startScale = Vector3.zero;
        Vector3 endScale = Vector3.one * maxScale;
        float elapsed = 0f;

        // 확장
        while (elapsed < duration * 0.5f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (duration * 0.5f);
            effectTransform.localScale = Vector3.Lerp(startScale, endScale, t);
            yield return null;
        }

        // 축소
        elapsed = 0f;
        while (elapsed < duration * 0.5f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (duration * 0.5f);
            effectTransform.localScale = Vector3.Lerp(endScale, Vector3.zero, t);
            yield return null;
        }
    }

    // 회전이 포함된 스케일 이펙트 (자석용)
    System.Collections.IEnumerator ScaleEffectWithRotation(Transform effectTransform, Color color, float maxScale, float duration)
    {
        Vector3 startScale = Vector3.zero;
        Vector3 endScale = Vector3.one * maxScale;
        float elapsed = 0f;

        // 확장 + 회전
        while (elapsed < duration * 0.6f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (duration * 0.6f);
            effectTransform.localScale = Vector3.Lerp(startScale, endScale, t);
            effectTransform.Rotate(0, 0, 720 * Time.deltaTime); // 빠른 회전
            yield return null;
        }

        // 축소 + 회전
        elapsed = 0f;
        while (elapsed < duration * 0.4f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (duration * 0.4f);
            effectTransform.localScale = Vector3.Lerp(endScale, Vector3.zero, t);
            effectTransform.Rotate(0, 0, 360 * Time.deltaTime); // 느린 회전
            yield return null;
        }
    }

    // 페이드 효과가 포함된 스케일 이펙트 (투명화 포션용)
    System.Collections.IEnumerator ScaleEffectWithFade(Transform effectTransform, Color color, float maxScale, float duration)
    {
        Vector3 startScale = Vector3.zero;
        Vector3 endScale = Vector3.one * maxScale;
        float elapsed = 0f;

        // 확장
        while (elapsed < duration * 0.4f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (duration * 0.4f);
            effectTransform.localScale = Vector3.Lerp(startScale, endScale, t);
            yield return null;
        }

        // 깜빡임
        elapsed = 0f;
        while (elapsed < duration * 0.2f)
        {
            elapsed += Time.deltaTime;
            // 깜빡임 효과
            float alpha = Mathf.Sin(elapsed * 10f) * 0.5f + 0.5f;
            yield return null;
        }

        // 축소
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
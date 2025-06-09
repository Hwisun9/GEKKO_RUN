using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeartUISystem : MonoBehaviour
{
    [Header("Heart Icons")]
    public GameObject heartIconPrefab;    // 채워진 하트 프리팹
    public GameObject emptyHeartIconPrefab;  // 빈 하트 프리팹
    
    [Header("Settings")]
    public int maxHearts = 3;             // 최대 하트 개수
    private int currentHearts;            // 현재 하트 개수
    
    // 하트 이미지들의 목록
    private List<GameObject> heartObjects = new List<GameObject>();
    
    // GameManager 참조
    private GameManager gameManager;
    
    void Start()
    {
        gameManager = GameManager.Instance;
        
        // GameManager에서 최대 목숨과 현재 목숨 가져오기
        if (gameManager != null)
        {
            maxHearts = gameManager.maxLives;
            currentHearts = gameManager.currentLives;
        }
        
        // 초기 하트 UI 생성
        InitializeHearts();
        
        // 목숨 UI 업데이트
        UpdateHeartUI();
    }
    
    // 하트 UI 초기화
    void InitializeHearts()
    {
        // 기존 하트 오브젝트 제거
        foreach (GameObject heart in heartObjects)
        {
            if (heart != null)
                Destroy(heart);
        }
        heartObjects.Clear();
        
        // 최대 하트 수만큼 하트 생성
        for (int i = 0; i < maxHearts; i++)
        {
            GameObject heartObject = Instantiate(heartIconPrefab, transform);
            heartObjects.Add(heartObject);
        }
        
        Debug.Log($"Heart UI initialized with {maxHearts} hearts");
    }
    
    // 하트 UI 업데이트
    public void UpdateHeartUI()
    {
        // GameManager에서 현재 목숨 가져오기
        if (gameManager != null)
        {
            currentHearts = gameManager.currentLives;
        }
        
        Debug.Log($"Updating Heart UI: {currentHearts}/{maxHearts} hearts");
        
        // 모든 하트 상태 업데이트
        for (int i = 0; i < heartObjects.Count; i++)
        {
            if (heartObjects[i] != null)
            {
                // i+1번째가 현재 하트 수보다 크면 빈 하트로 교체
                if (i >= currentHearts)
                {
                    ReplaceHeartWithEmpty(i);
                }
                // 그렇지 않으면 채워진 하트로 교체
                else
                {
                    ReplaceHeartWithFilled(i);
                }
            }
        }
    }
    
    // 특정 인덱스의 하트를 빈 하트로 교체
    void ReplaceHeartWithEmpty(int index)
    {
        if (index < 0 || index >= heartObjects.Count)
            return;
            
        // 현재 하트가 이미 빈 하트면 무시
        if (heartObjects[index].name.Contains("Empty"))
            return;
            
        // 현재 하트 제거
        GameObject oldHeart = heartObjects[index];
        
        // 새 빈 하트 생성 및 같은 위치에 배치
        GameObject newEmptyHeart = Instantiate(emptyHeartIconPrefab, transform);
        newEmptyHeart.transform.SetSiblingIndex(index); // 같은 위치 유지
        
        // 목록 업데이트
        heartObjects[index] = newEmptyHeart;
        
        // 이전 하트 제거
        Destroy(oldHeart);
    }
    
    // 특정 인덱스의 하트를 채워진 하트로 교체
    void ReplaceHeartWithFilled(int index)
    {
        if (index < 0 || index >= heartObjects.Count)
            return;
            
        // 현재 하트가 이미 채워진 하트면 무시
        if (!heartObjects[index].name.Contains("Empty"))
            return;
            
        // 현재 하트 제거
        GameObject oldHeart = heartObjects[index];
        
        // 새 채워진 하트 생성 및 같은 위치에 배치
        GameObject newFilledHeart = Instantiate(heartIconPrefab, transform);
        newFilledHeart.transform.SetSiblingIndex(index); // 같은 위치 유지
        
        // 목록 업데이트
        heartObjects[index] = newFilledHeart;
        
        // 이전 하트 제거
        Destroy(oldHeart);
    }
    
    // 하트 개수 설정 (외부에서 호출 가능)
    public void SetHearts(int count)
    {
        currentHearts = Mathf.Clamp(count, 0, maxHearts);
        UpdateHeartUI();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// 객체 풀링 시스템을 위한 클래스
public class ObjectPool : MonoBehaviour
{
    [Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int size;
    }

    public List<Pool> pools;
    private Dictionary<string, Queue<GameObject>> poolDictionary;

    // 싱글톤 접근을 위한 인스턴스
    public static ObjectPool Instance;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        InitializePools();
    }

    public void InitializePools()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach (Pool pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();

            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }

            poolDictionary.Add(pool.tag, objectPool);
        }
    }

    // 타입과 태그를 기반으로 풀을 추가하는 메서드
    public void AddPrefabToPool(GameObject prefab, string tag, int initialSize)
    {
        // 이미 존재하는 태그인지 확인
        if (poolDictionary != null && poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"Pool with tag {tag} already exists.");
            return;
        }

        // 풀 추가
        Pool newPool = new Pool { tag = tag, prefab = prefab, size = initialSize };
        
        // pools 목록이 초기화되지 않았다면 초기화
        if (pools == null) pools = new List<Pool>();
        pools.Add(newPool);
        
        // poolDictionary가 초기화되지 않았다면 초기화
        if (poolDictionary == null) poolDictionary = new Dictionary<string, Queue<GameObject>>();
        
        // 객체 생성 및 풀 저장
        Queue<GameObject> objectPool = new Queue<GameObject>();

        for (int i = 0; i < initialSize; i++)
        {
            GameObject obj = Instantiate(prefab);
            obj.SetActive(false);
            objectPool.Enqueue(obj);
        }

        poolDictionary.Add(tag, objectPool);
    }

    // 이미 풀에 있는 프리팹을 확인하는 메서드
    public bool HasPrefabInPool(GameObject prefab, out string existingTag)
    {
        existingTag = null;

        if (pools == null) return false;

        foreach (Pool pool in pools)
        {
            if (pool.prefab == prefab)
            {
                existingTag = pool.tag;
                return true;
            }
        }

        return false;
    }

    // 태그로 객체 가져오기
    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"Pool with tag {tag} doesn't exist.");
            return null;
        }

        // 풀에서 객체 꺼내기
        Queue<GameObject> objectPool = poolDictionary[tag];

        // 풀이 비어있으면 새로 추가
        if (objectPool.Count == 0)
        {
            // 객체 풀이 비어있을 경우 새로 추가
            foreach (Pool pool in pools)
            {
                if (pool.tag == tag)
                {
                    GameObject newObj = Instantiate(pool.prefab);
                    newObj.SetActive(true);
                    newObj.transform.position = position;
                    newObj.transform.rotation = rotation;
                    return newObj;
                }
            }
        }

        // 풀에서 객체 가져오기
        GameObject objectToSpawn = objectPool.Dequeue();

        // 객체가 삭제되었다면 새로 생성
        if (objectToSpawn == null)
        {
            foreach (Pool pool in pools)
            {
                if (pool.tag == tag)
                {
                    objectToSpawn = Instantiate(pool.prefab);
                    break;
                }
            }
        }

        // 객체 활성화 및 위치 설정
        objectToSpawn.SetActive(true);
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;

        // 새로운 객체를 큐에 다시 추가 (순환)
        poolDictionary[tag].Enqueue(objectToSpawn);

        return objectToSpawn;
    }

    // 객체 풀로 반환
    public void ReturnToPool(string tag, GameObject objectToReturn)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"Pool with tag {tag} doesn't exist.");
            return;
        }

        // 객체 비활성화 후 풀에 반환
        objectToReturn.SetActive(false);
        poolDictionary[tag].Enqueue(objectToReturn);
    }

    // 필요한 경우 풀 크기 조정
    public void ResizePool(string tag, int newSize)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"Pool with tag {tag} doesn't exist.");
            return;
        }

        // 현재 풀 크기 확인
        int currentSize = poolDictionary[tag].Count;

        // 원하는 프리팹 찾기
        GameObject prefab = null;
        foreach (Pool pool in pools)
        {
            if (pool.tag == tag)
            {
                prefab = pool.prefab;
                pool.size = newSize; // 풀 크기 업데이트
                break;
            }
        }

        if (prefab == null)
        {
            Debug.LogError($"Prefab for pool {tag} not found.");
            return;
        }

        // 크기를 증가시키는 경우
        if (newSize > currentSize)
        {
            int numToAdd = newSize - currentSize;
            for (int i = 0; i < numToAdd; i++)
            {
                GameObject obj = Instantiate(prefab);
                obj.SetActive(false);
                poolDictionary[tag].Enqueue(obj);
            }
        }
        // 크기를 감소시키는 경우는 자연스럽게 처리됨 (현재 크기 유지)
    }
}
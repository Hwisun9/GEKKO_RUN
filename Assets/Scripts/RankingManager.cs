using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class RankingManager : MonoBehaviour
{
    [System.Serializable]
    public class ScoreEntry
    {
        public string playerName;
        public int score;
        public float playTime;

        public ScoreEntry(string name, int score, float time)
        {
            this.playerName = name;
            this.score = score;
            this.playTime = time;
        }
    }
    
    [System.Serializable]
    public class ScoreEntryList
    {
        public List<ScoreEntry> entries = new List<ScoreEntry>();
    }

    [Header("Ranking Panel")]
    public GameObject rankingPanel;

    [Header("UI Elements")]
    public Transform scoreEntriesContainer;
    public GameObject scoreEntryPrefab;
    public TextMeshProUGUI noScoresText;

    [Header("Settings")]
    public int maxRankEntries = 10;

    // PlayerPrefs 키
    private const string HIGH_SCORES_KEY = "HighScores";

    // 랭킹 데이터
    private List<ScoreEntry> highScores = new List<ScoreEntry>();

    private void Start()
    {
        InitializeRanking();
    }

    private void InitializeRanking()
    {
        // 랭킹 패널 초기 상태 설정 - 반드시 비활성화로 시작
        if (rankingPanel != null)
        {
            rankingPanel.SetActive(false);
        }

        // 저장된 랭킹 데이터 로드
        LoadHighScores();
    }

    // 랭킹 패널 열기
    public void OpenRankingPanel()
    {
        if (rankingPanel != null)
        {
            rankingPanel.SetActive(true);
            DisplayHighScores();
        }
    }

    // 랭킹 패널 닫기
    public void CloseRankingPanel()
    {
        if (rankingPanel != null)
        {
            rankingPanel.SetActive(false);
        }
    }

    // 랭킹 패널 토글
    public void ToggleRankingPanel()
    {
        if (rankingPanel != null)
        {
            bool newState = !rankingPanel.activeSelf;
            rankingPanel.SetActive(newState);
            
            if (newState) // 패널이 활성화되는 경우만 디스플레이 업데이트
            {
                DisplayHighScores();
            }
        }
    }

    // 하이스코어 데이터 로드
    private void LoadHighScores()
    {
        highScores.Clear();

        string json = PlayerPrefs.GetString(HIGH_SCORES_KEY, "");
        
        if (!string.IsNullOrEmpty(json))
        {
            // JSON 문자열을 ScoreEntryList로 변환
            try
            {
                ScoreEntryList loadedScores = JsonUtility.FromJson<ScoreEntryList>(json);
                if (loadedScores != null && loadedScores.entries != null)
                {
                    highScores = new List<ScoreEntry>(loadedScores.entries);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error loading high scores: " + e.Message);
                highScores = new List<ScoreEntry>();
            }
        }

        // 기존 저장 방식 대응 (단일 최고 점수만 저장한 경우)
        if (highScores.Count == 0)
        {
            int highScore = PlayerPrefs.GetInt("HighScore", 0);
            float highScoreTime = PlayerPrefs.GetFloat("HighScoreTime", 0f);
            
            if (highScore > 0)
            {
                highScores.Add(new ScoreEntry("Player", highScore, highScoreTime));
                SaveHighScores();
            }
        }
    }

    // 하이스코어 데이터 저장
    private void SaveHighScores()
    {
        // 최대 개수로 제한
        if (highScores.Count > maxRankEntries)
        {
            highScores = highScores.Take(maxRankEntries).ToList();
        }

        // ScoreEntry 리스트를 ScoreEntryList로 포장하여 JSON 문자열로 변환
        ScoreEntryList scoreEntryList = new ScoreEntryList();
        scoreEntryList.entries = highScores;
        string json = JsonUtility.ToJson(scoreEntryList);
        
        PlayerPrefs.SetString(HIGH_SCORES_KEY, json);
        PlayerPrefs.Save();
    }

    // 새로운 점수 추가
    public void AddScore(string playerName, int score, float playTime)
    {
        // 이름이 비어있으면 기본값 설정
        if (string.IsNullOrEmpty(playerName))
        {
            playerName = "Player";
        }

        // 새로운 점수 추가
        highScores.Add(new ScoreEntry(playerName, score, playTime));
        
        // 점수 내림차순으로 정렬
        highScores = highScores.OrderByDescending(s => s.score).ToList();
        
        // 저장
        SaveHighScores();
    }

    // 하이스코어 표시
    private void DisplayHighScores()
    {
        // 기존 항목 삭제
        if (scoreEntriesContainer != null)
        {
            foreach (Transform child in scoreEntriesContainer)
            {
                Destroy(child.gameObject);
            }
        }
        else
        {
            Debug.LogError("Score entries container is not set!");
            return;
        }

        // 점수가 없는 경우
        if (highScores.Count == 0)
        {
            if (noScoresText != null)
            {
                noScoresText.gameObject.SetActive(true);
            }
            return;
        }

        // noScoresText 숨기기
        if (noScoresText != null)
        {
            noScoresText.gameObject.SetActive(false);
        }

        // 상위 점수 표시
        int displayCount = Mathf.Min(highScores.Count, maxRankEntries);
        
        for (int i = 0; i < displayCount; i++)
        {
            ScoreEntry entry = highScores[i];
            
            if (scoreEntryPrefab == null)
            {
                Debug.LogError("Score entry prefab is not set!");
                return;
            }
            
            // 엔트리 생성
            GameObject entryObject = Instantiate(scoreEntryPrefab, scoreEntriesContainer);
            
            // 랭킹 정보 설정
            TextMeshProUGUI rankText = entryObject.transform.Find("RankText")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI nameText = entryObject.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI scoreText = entryObject.transform.Find("ScoreText")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI timeText = entryObject.transform.Find("TimeText")?.GetComponent<TextMeshProUGUI>();
            
            if (rankText != null) rankText.text = (i + 1).ToString();
            if (nameText != null) nameText.text = entry.playerName;
            if (scoreText != null) scoreText.text = entry.score.ToString();
            
            if (timeText != null)
            {
                int minutes = Mathf.FloorToInt(entry.playTime / 60f);
                int seconds = Mathf.FloorToInt(entry.playTime % 60f);
                timeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
            }
        }
    }

    // 랭킹 리셋 (디버그용)
    public void ResetRanking()
    {
        highScores.Clear();
        SaveHighScores();
        DisplayHighScores();
    }

    // 현재 최고 점수 가져오기
    public int GetHighestScore()
    {
        if (highScores.Count > 0)
        {
            return highScores[0].score;
        }
        return 0;
    }
}
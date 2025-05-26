using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI highScoreText;

    void Update()
    {
        if (GameManager.Instance.isGameActive)
        {
            scoreText.text = "점수: " + GameManager.Instance.score;
        }
    }

    // 최고 점수 업데이트
    public void UpdateHighScore()
    {
        int currentHighScore = PlayerPrefs.GetInt("HighScore", 0);

        if (GameManager.Instance.score > currentHighScore)
        {
            PlayerPrefs.SetInt("HighScore", GameManager.Instance.score);
            highScoreText.text = "최고 점수: " + GameManager.Instance.score;
        }
        else
        {
            highScoreText.text = "최고 점수: " + currentHighScore;
        }
    }
}
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
            scoreText.text = "����: " + GameManager.Instance.score;
        }
    }

    // �ְ� ���� ������Ʈ
    public void UpdateHighScore()
    {
        int currentHighScore = PlayerPrefs.GetInt("HighScore", 0);

        if (GameManager.Instance.score > currentHighScore)
        {
            PlayerPrefs.SetInt("HighScore", GameManager.Instance.score);
            highScoreText.text = "�ְ� ����: " + GameManager.Instance.score;
        }
        else
        {
            highScoreText.text = "�ְ� ����: " + currentHighScore;
        }
    }
}
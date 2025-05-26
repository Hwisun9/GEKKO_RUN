using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOverManager : MonoBehaviour
{
    public CanvasGroup gameOverCanvasGroup; // GameOverPanel의 CanvasGroup
    public float fadeDuration = 1f;         // 페이드 인 지속시간
    public TextMeshProUGUI gameOverText;    // 게임오버 텍스트

    void Start()
    {
        // 게임오버 패널 비활성화 상태 (알파값 0)
        if (gameOverCanvasGroup != null)
        {
            gameOverCanvasGroup.alpha = 0;
            gameOverCanvasGroup.interactable = false;
            gameOverCanvasGroup.blocksRaycasts = false;
        }
    }

    public void ShowGameOverPanel()
    {
        if (gameOverCanvasGroup != null)
        {
            LeanTween.alphaCanvas(gameOverCanvasGroup, 1f, fadeDuration)
                .setEase(LeanTweenType.easeOutQuad)
                .setOnComplete(() => {
                    gameOverCanvasGroup.interactable = true;     // 상호작용 가능
                    gameOverCanvasGroup.blocksRaycasts = true;   // 클릭 차단 해제
                });
        }
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("StartScene"); // StartScene 이름에 맞게 수정
    }
}

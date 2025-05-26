using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOverManager : MonoBehaviour
{
    public CanvasGroup gameOverCanvasGroup; // GameOverPanel�� CanvasGroup
    public float fadeDuration = 1f;         // ���̵� �� ���ӽð�
    public TextMeshProUGUI gameOverText;    // ���ӿ��� �ؽ�Ʈ

    void Start()
    {
        // ���ӿ��� �г� ��Ȱ��ȭ ���� (���İ� 0)
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
                    gameOverCanvasGroup.interactable = true;     // ��ȣ�ۿ� ����
                    gameOverCanvasGroup.blocksRaycasts = true;   // Ŭ�� ���� ����
                });
        }
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("StartScene"); // StartScene �̸��� �°� ����
    }
}

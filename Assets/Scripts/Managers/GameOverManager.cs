using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening; // Add this!

public class GameOverManager : MonoBehaviour
{
    public GameObject gameOverPanel;
    private CanvasGroup canvasGroup;

    void Start()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);

            // Ensure a CanvasGroup exists for fading
            canvasGroup = gameOverPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = gameOverPanel.AddComponent<CanvasGroup>();
        }
    }

    public void TriggerGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            canvasGroup.alpha = 0f;
            canvasGroup.DOFade(1f, 0.5f).SetUpdate(true).SetEase(Ease.OutQuad);

            Time.timeScale = 0f; // Pause the game
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}

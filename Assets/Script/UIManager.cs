using UnityEngine;
using TMPro;
using System.Collections;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject startScreen; // TAP TO START paneli
    [SerializeField] private GameObject gameOverScreen; // Oyun bitti paneli
    [SerializeField] private TextMeshProUGUI scoreText; // Anlık skor
    [SerializeField] private TextMeshProUGUI highScoreText; // Ana ekrandaki en yüksek skor
    [SerializeField] private TextMeshProUGUI gameOverScoreText; // Oyun bitti ekranındaki skor
    [SerializeField] private TextMeshProUGUI gameOverHighScoreText; // Oyun bitti ekranındaki en yüksek skor
    [SerializeField] private float transitionDuration = 0.3f; // Animasyon süresi (daha kısa ve yumuşak)

    private PlayerController player;
    private int highScore;
    private bool isTransitioning = false; // Geçiş animasyonu sırasında çakışmayı önlemek için

    void Start()
    {
        player = FindObjectOfType<PlayerController>();
        highScore = PlayerPrefs.GetInt("HighScore", 0);

        // Başlangıçta tüm UI elemanlarını kapat
        if (startScreen != null) startScreen.SetActive(false);
        if (gameOverScreen != null) gameOverScreen.SetActive(false);
        if (scoreText != null) scoreText.gameObject.SetActive(false);

        // Sadece StartScreen'i aç
        ShowStartScreen();

        // Hata kontrolü
        if (startScreen == null) Debug.LogError("StartScreen atanmamış!");
        if (gameOverScreen == null) Debug.LogError("GameOverScreen atanmamış!");
        if (scoreText == null) Debug.LogError("ScoreText atanmamış!");
        if (highScoreText == null) Debug.LogError("HighScoreText atanmamış!");
        if (gameOverScoreText == null) Debug.LogError("GameOverScoreText atanmamış!");
        if (gameOverHighScoreText == null) Debug.LogError("GameOverHighScoreText atanmamış!");
    }

    public void ShowStartScreen()
    {
        StartCoroutine(ShowStartScreenCoroutine());
    }

    private IEnumerator ShowStartScreenCoroutine()
    {
        // Önceki ekranın kapanmasını bekle
        while (isTransitioning)
        {
            yield return null;
        }

        isTransitioning = true;
        StartCoroutine(TransitionToScreen(gameOverScreen, false));
        StartCoroutine(TransitionToScreen(scoreText.gameObject, false));
        yield return StartCoroutine(TransitionToScreen(startScreen, true));
        isTransitioning = false;
    }

    public void ShowGameScreen()
    {
        StartCoroutine(ShowGameScreenCoroutine());
    }

    private IEnumerator ShowGameScreenCoroutine()
    {
        while (isTransitioning)
        {
            yield return null;
        }

        isTransitioning = true;
        StartCoroutine(TransitionToScreen(startScreen, false));
        StartCoroutine(TransitionToScreen(gameOverScreen, false));
        yield return StartCoroutine(TransitionToScreen(scoreText.gameObject, true));
        isTransitioning = false;
    }

    public void ShowGameOverScreen()
    {
        StartCoroutine(ShowGameOverScreenCoroutine());
    }

    private IEnumerator ShowGameOverScreenCoroutine()
    {
        while (isTransitioning)
        {
            yield return null;
        }

        isTransitioning = true;
        StartCoroutine(TransitionToScreen(startScreen, false));
        StartCoroutine(TransitionToScreen(scoreText.gameObject, false));
        yield return StartCoroutine(TransitionToScreen(gameOverScreen, true));
        isTransitioning = false;
    }

    public void UpdateScore(int score)
    {
        if (scoreText != null) scoreText.text = " " + score;
        if (gameOverScoreText != null) gameOverScoreText.text = "Score: " + score;
    }

    public void UpdateHighScore(int score)
    {
        highScore = score;
        if (highScoreText != null) highScoreText.text = "High Score: " + highScore;
        if (gameOverHighScoreText != null) gameOverHighScoreText.text = "High Score: " + highScore;
    }

    private IEnumerator TransitionToScreen(GameObject uiElement, bool fadeIn)
    {
        if (uiElement == null)
        {
            Debug.LogWarning("UI elemanı null, geçiş yapılamıyor!");
            yield break;
        }

        CanvasGroup canvasGroup = uiElement.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            Debug.LogWarning($"UI elemanı ({uiElement.name}) üzerinde CanvasGroup bileşeni yok, doğrudan aç/kapat yapılıyor.");
            uiElement.SetActive(fadeIn);
            yield break;
        }

        // Eğer fadeIn değilse ve zaten kapalıysa işlemi atla
        if (!fadeIn && !uiElement.activeSelf)
        {
            Debug.Log($"UI elemanı ({uiElement.name}) zaten kapalı, geçiş atlandı.");
            yield break;
        }

        // Eğer fadeIn ise ve zaten açıksa işlemi atla
        if (fadeIn && uiElement.activeSelf && canvasGroup.alpha == 1f)
        {
            Debug.Log($"UI elemanı ({uiElement.name}) zaten açık, geçiş atlandı.");
            yield break;
        }

        uiElement.SetActive(true);
        float startAlpha = fadeIn ? 0f : 1f;
        float endAlpha = fadeIn ? 1f : 0f;
        float elapsedTime = 0f;

        canvasGroup.alpha = startAlpha;

        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / transitionDuration;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, t);
            yield return null;
        }

        canvasGroup.alpha = endAlpha;
        if (!fadeIn)
        {
            uiElement.SetActive(false);
            Debug.Log($"UI elemanı ({uiElement.name}) kapatıldı.");
        }
    }
}
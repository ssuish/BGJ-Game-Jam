using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject endScreen;
    [SerializeField] private TextMeshProUGUI gameOverText;
    [SerializeField] private Button restartButton;

    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 0.5f;

    [Header("End State Text")]
    [SerializeField] private string winMessage = "You Win!";

    public bool IsGameActive { get; private set; }

    private CanvasGroup endScreenCanvasGroup;
    private Coroutine fadeCoroutine;
    private bool isBound;
    private bool hasTriggeredEndState;
    private string gameOverMessage;

    private void Awake()
    {
        InitializeEndScreen();
    }

    private void Start()
    {
        IsGameActive = true;
        TryBindSafetyManager();
    }

    private void OnEnable()
    {
        TryBindSafetyManager();
    }

    private void OnDisable()
    {
        UnbindSafetyManager();
    }

    public void GameOver()
    {
        ShowEndScreen(gameOverMessage);
    }

    public void WinGame()
    {
        ShowEndScreen(winMessage);
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void ShowEndScreen(string endMessage)
    {
        if (hasTriggeredEndState)
        {
            return;
        }

        hasTriggeredEndState = true;
        IsGameActive = false;

        if (endScreen == null)
        {
            return;
        }

        if (gameOverText != null)
        {
            gameOverText.text = endMessage;
        }

        endScreen.SetActive(true);
        gameOverText?.gameObject.SetActive(true);
        restartButton?.gameObject.SetActive(true);

        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        fadeCoroutine = StartCoroutine(FadeInEndScreen());
    }

    private void TryBindSafetyManager()
    {
        if (isBound || SafetyMeterManager.Instance == null)
        {
            return;
        }

        SafetyMeterManager.Instance.OnSafetyMeterValueChanged += OnSafetyMeterValueChanged;
        isBound = true;
    }

    private void UnbindSafetyManager()
    {
        if (!isBound)
        {
            return;
        }

        if (SafetyMeterManager.Instance != null)
        {
            SafetyMeterManager.Instance.OnSafetyMeterValueChanged -= OnSafetyMeterValueChanged;
        }

        isBound = false;
    }

    private void InitializeEndScreen()
    {
        gameOverMessage = gameOverText != null ? gameOverText.text : string.Empty;

        if (endScreen == null)
        {
            return;
        }

        endScreenCanvasGroup = endScreen.GetComponent<CanvasGroup>();

        if (endScreenCanvasGroup == null)
        {
            endScreenCanvasGroup = endScreen.AddComponent<CanvasGroup>();
        }

        endScreenCanvasGroup.alpha = 0f;
        endScreen.SetActive(false);
        gameOverText?.gameObject.SetActive(false);
        restartButton?.gameObject.SetActive(false);
    }

    private void OnSafetyMeterValueChanged(float safetyMeterValue)
    {
        if (safetyMeterValue > 0f)
        {
            return;
        }

        GameOver();
    }

    private IEnumerator FadeInEndScreen()
    {
        if (endScreenCanvasGroup == null)
        {
            yield break;
        }

        float elapsed = 0f;
        endScreenCanvasGroup.alpha = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            endScreenCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            yield return null;
        }

        endScreenCanvasGroup.alpha = 1f;
        fadeCoroutine = null;
    }
}

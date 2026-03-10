using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CheckInPromptUI : MonoBehaviour
{
    private const string PromptMessage = "Synchronized Breath: Hold Space + Enter";

    [Header("Panel")]
    [SerializeField] private GameObject promptPanel;
    [SerializeField] private TMP_Text promptText;

    [Header("Progress")]
    [SerializeField] private Image sparkProgressBar;
    [SerializeField] private Image bulkProgressBar;

    [Header("Feedback")]
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private string successMessage = "Perfect Sync!";
    [SerializeField] private string failureMessage = "Sync Broken. Try Again.";
    [SerializeField] private Color successColor = new(0.3f, 1f, 0.5f);
    [SerializeField] private Color failureColor = new(1f, 0.35f, 0.35f);
    [SerializeField] private float feedbackDuration = 1.2f;

    private float feedbackTimer;
    private bool isPromptVisible;

    private void Awake()
    {
        SetPromptText();
        SetPromptVisible(false);
        SetProgress(0f, 0f);
        HideFeedback();
    }

    private void Update()
    {
        UpdateFeedbackTimer();
    }

    public void ShowPrompt()
    {
        SetPromptVisible(true);
    }

    public void HidePrompt()
    {
        SetPromptVisible(false);
    }

    public void SetPromptVisible(bool visible)
    {
        isPromptVisible = visible;

        if (promptPanel != null)
        {
            promptPanel.SetActive(visible);
        }

        if (!visible)
        {
            SetProgress(0f, 0f);
            HideFeedback();
        }
    }

    public void SetProgress(float sparkNormalized, float bulkNormalized)
    {
        if (sparkProgressBar != null)
        {
            sparkProgressBar.fillAmount = Mathf.Clamp01(sparkNormalized);
        }

        if (bulkProgressBar != null)
        {
            bulkProgressBar.fillAmount = Mathf.Clamp01(bulkNormalized);
        }
    }

    public void ShowSuccessFeedback()
    {
        ShowFeedback(successMessage, successColor);
    }

    public void ShowFailureFeedback()
    {
        ShowFeedback(failureMessage, failureColor);
    }

    private void SetPromptText()
    {
        if (promptText != null)
        {
            promptText.text = PromptMessage;
        }
    }

    private void ShowFeedback(string message, Color color)
    {
        if (feedbackText == null)
        {
            return;
        }

        feedbackText.gameObject.SetActive(true);
        feedbackText.text = message;
        feedbackText.color = color;
        feedbackTimer = feedbackDuration;
    }

    private void HideFeedback()
    {
        if (feedbackText != null)
        {
            feedbackText.gameObject.SetActive(false);
        }

        feedbackTimer = 0f;
    }

    private void UpdateFeedbackTimer()
    {
        if (feedbackText == null || !feedbackText.gameObject.activeSelf)
        {
            return;
        }

        feedbackTimer -= Time.deltaTime;

        if (feedbackTimer > 0f)
        {
            return;
        }

        HideFeedback();
    }
}

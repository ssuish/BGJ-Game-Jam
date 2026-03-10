using UnityEngine;
using UnityEngine.UI;

public class SafetyMeterUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image fillImage;

    [Header("Color Configuration")]
    [SerializeField] private Color safeColor = Color.green;
    [SerializeField] private Color warningColor = Color.yellow;
    [SerializeField] private Color dangerColor = Color.red;
    [SerializeField] private float warningThreshold = 0.5f;

    private SafetyMeterManager manager;

    // TODO: Add showPrompt() and hidePrompt() on specific game events or tutorial.

    private void OnEnable()
    {
        TryBindManager();
    }

    private void OnDisable()
    {
        if (manager != null)
        {
            manager.OnSafetyMeterPercentChanged -= UpdateMeterVisual;
        }
    }

    private void Start()
    {
        if (SafetyMeterManager.Instance != null)
        {
            UpdateMeterVisual(SafetyMeterManager.Instance.SafetyMeterPercent);
        }
    }

    private void Update()
    {
        if (manager == null)
        {
            TryBindManager();
        }
    }

    private void TryBindManager()
    {
        if (manager != null) return;
        if (SafetyMeterManager.Instance == null) return;

        manager = SafetyMeterManager.Instance;
        manager.OnSafetyMeterPercentChanged += UpdateMeterVisual;
        UpdateMeterVisual(manager.SafetyMeterPercent);
    }

    private void UpdateMeterVisual(float percent)
    {
        if (fillImage == null)
        {
            return;
        }

        fillImage.fillAmount = percent;
        fillImage.color = CalculateMeterColor(percent);
    }

    private Color CalculateMeterColor(float percent)
    {
        if (percent > warningThreshold)
        {
            float t = (percent - warningThreshold) / (1f - warningThreshold);
            return Color.Lerp(warningColor, safeColor, t);
        }
        else
        {
            float t = percent / warningThreshold;
            return Color.Lerp(dangerColor, warningColor, t);
        }
    }
}

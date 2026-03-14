using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Light2D))]
public class SparkCheckInLightController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SafetyMeterManager safetyMeter;
    [SerializeField] private Light2D sparkLight;

    [Header("Blink Configuration")]
    [SerializeField] private float blinkThresholdPercent = 0.5f;
    [SerializeField] private float minBlinkIntensity = 0.5f;
    [SerializeField] private float maxBlinkIntensity = 2f;
    [SerializeField] private float blinkSpeed = 8f;

    public bool IsBlinking { get; private set; }

    private float defaultIntensity;
    private bool isBoundToSafetySignals;

    private void Awake()
    {
        if (sparkLight == null)
        {
            sparkLight = GetComponent<Light2D>();
        }

        if (sparkLight != null)
        {
            defaultIntensity = sparkLight.intensity;
        }
    }

    private void OnEnable()
    {
        TryBindSafetyMeter();
        ApplyBlinkState();
    }

    private void OnDisable()
    {
        UnbindSafetyMeter();
        IsBlinking = false;
        ApplyBlinkState();
    }

    private void Update()
    {
        if (!isBoundToSafetySignals)
        {
            if (safetyMeter == null)
            {
                safetyMeter = SafetyMeterManager.Instance;
            }

            TryBindSafetyMeter();
        }

        ApplyBlinkState();
    }

    private void TryBindSafetyMeter()
    {
        if (isBoundToSafetySignals || safetyMeter == null)
        {
            return;
        }

        safetyMeter.OnSafetyMeterPercentChanged += HandleSafetyMeterPercentChanged;
        isBoundToSafetySignals = true;
        HandleSafetyMeterPercentChanged(safetyMeter.SafetyMeterPercent);
    }

    private void UnbindSafetyMeter()
    {
        if (!isBoundToSafetySignals || safetyMeter == null)
        {
            return;
        }

        safetyMeter.OnSafetyMeterPercentChanged -= HandleSafetyMeterPercentChanged;
        isBoundToSafetySignals = false;
    }

    private void HandleSafetyMeterPercentChanged(float safetyMeterPercent)
    {
        IsBlinking = safetyMeterPercent < blinkThresholdPercent;
    }

    private void ApplyBlinkState()
    {
        if (sparkLight == null)
        {
            return;
        }

        if (!IsBlinking)
        {
            sparkLight.intensity = defaultIntensity;
            return;
        }

        float pulse = Mathf.PingPong(Time.time * blinkSpeed, 1f);
        sparkLight.intensity = Mathf.Lerp(minBlinkIntensity, maxBlinkIntensity, pulse);
    }
}
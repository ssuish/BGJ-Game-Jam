using UnityEngine;
using UnityEngine.UI;

public class VisualFeedbackManager : MonoBehaviour
{
    public static VisualFeedbackManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private SafetyMeterManager safetyMeterManager;
    [SerializeField] private Image anxietyOverlayImage;
    [SerializeField] private Sprite anxietyOverlaySprite;

    [Header("Anxiety Vignette")]
    [SerializeField] private float minVignetteIntensity = 0f;
    [SerializeField] private float maxVignetteIntensity = 1f;

    public float CurrentVignetteIntensity { get; private set; }

    private bool isBoundToSafetySignals;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        //DontDestroyOnLoad(gameObject);
        InitializeAnxietyOverlay();
    }

    private void OnEnable()
    {
        TryBindSafetyManager();
    }

    private void OnDisable()
    {
        UnbindSafetyManager();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Update()
    {
        if (!isBoundToSafetySignals)
        {
            if (safetyMeterManager == null)
            {
                safetyMeterManager = SafetyMeterManager.Instance;
            }

            TryBindSafetyManager();
        }

        InitializeAnxietyOverlay();
    }

    private void TryBindSafetyManager()
    {
        if (isBoundToSafetySignals)
        {
            return;
        }

        if (safetyMeterManager == null)
        {
            safetyMeterManager = SafetyMeterManager.Instance;
        }

        if (safetyMeterManager == null)
        {
            return;
        }

        safetyMeterManager.OnSafetyMeterPercentChanged += HandleSafetyMeterPercentChanged;
        isBoundToSafetySignals = true;
        HandleSafetyMeterPercentChanged(safetyMeterManager.SafetyMeterPercent);
    }

    private void UnbindSafetyManager()
    {
        if (!isBoundToSafetySignals || safetyMeterManager == null)
        {
            return;
        }

        safetyMeterManager.OnSafetyMeterPercentChanged -= HandleSafetyMeterPercentChanged;
        isBoundToSafetySignals = false;
    }

    private void InitializeAnxietyOverlay()
    {
        if (anxietyOverlayImage == null)
        {
            return;
        }

        if (anxietyOverlaySprite != null)
        {
            anxietyOverlayImage.sprite = anxietyOverlaySprite;
        }
    }

    private void HandleSafetyMeterPercentChanged(float safetyMeterPercent)
    {
        float clampedPercent = Mathf.Clamp01(safetyMeterPercent);
        float inversePercent = 1f - clampedPercent;
        CurrentVignetteIntensity = Mathf.Lerp(minVignetteIntensity, maxVignetteIntensity, inversePercent);
        ApplyVignetteIntensity(CurrentVignetteIntensity);
    }

    private void ApplyVignetteIntensity(float intensity)
    {
        if (anxietyOverlayImage == null)
        {
            return;
        }

        Color overlayColor = anxietyOverlayImage.color;
        overlayColor.a = intensity;
        anxietyOverlayImage.color = overlayColor;
    }
}

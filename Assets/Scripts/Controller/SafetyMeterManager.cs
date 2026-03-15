using System;
using UnityEngine;

public class SafetyMeterManager : MonoBehaviour
{
    public static SafetyMeterManager Instance { get; private set; }

    [Header("Player References")]
    [SerializeField] private Transform spark;
    [SerializeField] private Transform bulk;

    [Header("Safety Meter")]
    [SerializeField] private float maxSafetyMeter = 100f;
    [SerializeField] private float safeDistanceThreshold = 5f;
    [SerializeField] private float huddleDistanceThreshold = 1f;
    [SerializeField] private float drainRatePerSecond = 10f;
    [SerializeField] private float restoreRatePerSecond = 5f;
    [SerializeField] private float huddleRestoreRatePerSecond = 15f;

    public float SafetyMeterValue { get; private set; } = 100f;
    public float SafetyMeterPercent => maxSafetyMeter <= 0f ? 0f : SafetyMeterValue / maxSafetyMeter;
    public bool IsHuddling { get; private set; }
    public float DistanceBetweenPlayers { get; private set; }
    public float DrainRateMultiplier { get; private set; } = 1f;

    public event Action<float> OnSafetyMeterValueChanged;
    public event Action<float> OnSafetyMeterPercentChanged;
    public event Action<bool> OnHuddlingStateChanged;
    public event Action<float> OnDistanceBetweenPlayersChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        //DontDestroyOnLoad(gameObject);
        SafetyMeterValue = Mathf.Clamp(SafetyMeterValue, 0f, maxSafetyMeter);
    }

    private void Update()
    {
        if (spark == null || bulk == null)
        {
            return;
        }

        UpdateDistance();
        UpdateHuddleState();
        UpdateSafetyMeter();
    }

    public void SetPlayers(Transform sparkTransform, Transform bulkTransform)
    {
        spark = sparkTransform;
        bulk = bulkTransform;
    }

    public void SetDrainRateMultiplier(float multiplier)
    {
        DrainRateMultiplier = Mathf.Max(0f, multiplier);
    }

    public void AddSafety(float amount)
    {
        if (Mathf.Approximately(amount, 0f))
        {
            return;
        }

        float previousValue = SafetyMeterValue;
        SafetyMeterValue = Mathf.Clamp(SafetyMeterValue + amount, 0f, maxSafetyMeter);

        if (!Mathf.Approximately(previousValue, SafetyMeterValue))
        {
            OnSafetyMeterValueChanged?.Invoke(SafetyMeterValue);
            OnSafetyMeterPercentChanged?.Invoke(SafetyMeterPercent);
        }
    }

    private void UpdateDistance()
    {
        float previousDistance = DistanceBetweenPlayers;
        DistanceBetweenPlayers = Vector3.Distance(spark.position, bulk.position);

        if (!Mathf.Approximately(previousDistance, DistanceBetweenPlayers))
        {
            OnDistanceBetweenPlayersChanged?.Invoke(DistanceBetweenPlayers);
        }
    }

    private void UpdateHuddleState()
    {
        bool previousHuddlingState = IsHuddling;
        IsHuddling = DistanceBetweenPlayers < huddleDistanceThreshold;

        if (previousHuddlingState != IsHuddling)
        {
            OnHuddlingStateChanged?.Invoke(IsHuddling);
        }
    }

    private void UpdateSafetyMeter()
    {
        float previousValue = SafetyMeterValue;

        if (DistanceBetweenPlayers > safeDistanceThreshold)
        {
            SafetyMeterValue -= drainRatePerSecond * DrainRateMultiplier * Time.deltaTime;
        }
        else
        {
            float restorationRate = IsHuddling ? huddleRestoreRatePerSecond : restoreRatePerSecond;
            SafetyMeterValue += restorationRate * Time.deltaTime;
        }

        SafetyMeterValue = Mathf.Clamp(SafetyMeterValue, 0f, maxSafetyMeter);

        if (!Mathf.Approximately(previousValue, SafetyMeterValue))
        {
            OnSafetyMeterValueChanged?.Invoke(SafetyMeterValue);
            OnSafetyMeterPercentChanged?.Invoke(SafetyMeterPercent);
        }
    }
}

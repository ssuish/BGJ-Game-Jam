using UnityEngine;
using UnityEngine.Events;

public class SyncBreathManager : MonoBehaviour
{
    public static SyncBreathManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private CheckInManager checkInManager;
    [SerializeField] private SafetyMeterManager safetyMeterManager;

    [Header("Sync Breath Timing")]
    [SerializeField] private float checkInIntervalSeconds = 60f;
    [SerializeField] private float holdTimeoutSeconds = 10f;

    [Header("Outcomes")]
    [SerializeField] private float successSafetyBonus = 20f;
    [SerializeField] private float failureDrainMultiplier = 1.5f;

    [Header("Success Triggers")]
    [SerializeField] private UnityEvent onSyncSuccess;
    [SerializeField] private UnityEvent onClearStaticEffects;
    [SerializeField] private UnityEvent onSuccessParticleBurst;
    [SerializeField] private UnityEvent onSuccessSound;

    [Header("Failure Triggers")]
    [SerializeField] private UnityEvent onSyncFailure;
    [SerializeField] private UnityEvent onFailureSound;

    public bool IsCycleActive { get; private set; }
    public float TimeUntilNextPrompt => Mathf.Max(0f, Mathf.Max(checkInIntervalSeconds, 0.01f) - intervalTimer);

    private float intervalTimer;
    private float activeAttemptTimer;
    private bool areEventsBound;
    private bool hasLoggedMissingManagers;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        ResolveReferences();
        safetyMeterManager?.SetDrainRateMultiplier(1f);
    }

    private void OnEnable()
    {
        ResolveReferences();
        BindEvents();
    }

    private void OnDisable()
    {
        UnbindEvents();
    }

    private void Update()
    {
        ResolveReferences();

        if (checkInManager == null || safetyMeterManager == null)
        {
            if (!hasLoggedMissingManagers)
            {
                Debug.LogWarning("SyncBreathManager requires CheckInManager and SafetyMeterManager references.", this);
                hasLoggedMissingManagers = true;
            }

            return;
        }

        hasLoggedMissingManagers = false;

        if (IsCycleActive)
        {
            UpdateActiveCycle();
            return;
        }

        UpdateIntervalTimer();
    }

    private void ResolveReferences()
    {
        if (checkInManager == null)
        {
            checkInManager = FindFirstObjectByType<CheckInManager>();
        }

        if (safetyMeterManager == null)
        {
            safetyMeterManager = SafetyMeterManager.Instance;
        }

        if (!areEventsBound)
        {
            BindEvents();
        }
    }

    private void BindEvents()
    {
        if (areEventsBound || checkInManager == null)
        {
            return;
        }

        checkInManager.OnCheckInSucceeded += HandleCheckInSucceeded;
        checkInManager.OnCheckInFailed += HandleCheckInFailed;
        areEventsBound = true;
    }

    private void UnbindEvents()
    {
        if (!areEventsBound || checkInManager == null)
        {
            return;
        }

        checkInManager.OnCheckInSucceeded -= HandleCheckInSucceeded;
        checkInManager.OnCheckInFailed -= HandleCheckInFailed;
        areEventsBound = false;
    }

    private void UpdateIntervalTimer()
    {
        intervalTimer += Time.deltaTime;
        float clampedInterval = Mathf.Max(checkInIntervalSeconds, 0.01f);

        if (intervalTimer < clampedInterval)
        {
            return;
        }

        TryStartCheckInCycle();
    }

    private void TryStartCheckInCycle()
    {
        intervalTimer = 0f;
        activeAttemptTimer = 0f;
        IsCycleActive = true;
        checkInManager.StartCheckInAttempt();

        if (!checkInManager.IsActive)
        {
            IsCycleActive = false;
        }
    }

    private void UpdateActiveCycle()
    {
        if (!checkInManager.IsActive)
        {
            ResetCycleState();
            return;
        }

        activeAttemptTimer += Time.deltaTime;
        float clampedTimeout = Mathf.Max(holdTimeoutSeconds, 0.01f);

        if (activeAttemptTimer >= clampedTimeout)
        {
            checkInManager.FailCheckInAttempt();
        }
    }

    private void HandleCheckInSucceeded()
    {
        if (!IsCycleActive)
        {
            return;
        }

        safetyMeterManager.SetDrainRateMultiplier(1f);
        safetyMeterManager.AddSafety(successSafetyBonus);

        onSyncSuccess?.Invoke();
        onClearStaticEffects?.Invoke();
        onSuccessParticleBurst?.Invoke();
        onSuccessSound?.Invoke();

        ResetCycleState();
    }

    private void HandleCheckInFailed()
    {
        if (!IsCycleActive)
        {
            return;
        }

        safetyMeterManager.SetDrainRateMultiplier(Mathf.Max(failureDrainMultiplier, 1f));

        onSyncFailure?.Invoke();
        onFailureSound?.Invoke();

        ResetCycleState();
    }

    private void ResetCycleState()
    {
        IsCycleActive = false;
        activeAttemptTimer = 0f;
        intervalTimer = 0f;
    }
}

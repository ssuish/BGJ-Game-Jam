using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class CheckInManager : MonoBehaviour
{
    private const string CheckInActionName = "Check In";

    public enum CheckInFlowState
    {
        NotEligible,
        Eligible,
        Active,
        Success,
        Fail
    }

    [Header("References")]
    [SerializeField] private SafetyMeterManager safetyMeterManager;
    [SerializeField] private CheckInPromptUI checkInPromptUI;
    [SerializeField] private PlayerInput sparkPlayerInput;
    [SerializeField] private PlayerInput bulkPlayerInput;

    [Header("Eligibility")]
    [SerializeField] private bool requireTutorialPhase = true;
    [SerializeField] private bool autoActivateWhenEligible = true;
    [SerializeField] private bool requireHuddleForEligibility = true;
    [SerializeField] private float maxEligibleDistance = 1.75f;

    [Header("Check-In Input")]
    [SerializeField] private float requiredHoldDuration = 1.5f;

    [Header("Feedback")]
    [SerializeField] private float feedbackDisplayDuration = 1.2f;

    private Coroutine promptHideCoroutine;

    public CheckInFlowState CurrentState { get; private set; } = CheckInFlowState.NotEligible;
    public bool IsEligible { get; private set; }
    public bool IsActive => CurrentState == CheckInFlowState.Active;

    public event Action<CheckInFlowState> OnCheckInStateChanged;
    public event Action<bool> OnEligibilityChanged;
    public event Action<bool> OnPromptVisibilityChanged;
    public event Action OnCheckInSucceeded;
    public event Action OnCheckInFailed;

    private bool tutorialPhaseActive;
    private bool encounterActive;
    private bool isHuddling;
    private float distanceBetweenPlayers = float.MaxValue;
    private bool isBoundToSafetySignals;
    private InputAction sparkCheckInAction;
    private InputAction bulkCheckInAction;
    private float sparkHoldDuration;
    private float bulkHoldDuration;
    private bool wasBothHolding;

    private void Awake()
    {
        if (safetyMeterManager == null)
        {
            safetyMeterManager = SafetyMeterManager.Instance;
        }

        TryBindSafetySignals();
        CacheInputActions();
        UpdateEligibility();
        UpdatePromptVisibility();
    }

    private void OnEnable()
    {
        TryBindSafetySignals();
        CacheInputActions();
        sparkCheckInAction?.Enable();
        bulkCheckInAction?.Enable();
    }

    private void OnDisable()
    {
        sparkCheckInAction?.Disable();
        bulkCheckInAction?.Disable();
        UnbindSafetySignals();
    }

    private void Update()
    {
        if (!isBoundToSafetySignals)
        {
            if (safetyMeterManager == null)
            {
                safetyMeterManager = SafetyMeterManager.Instance;
            }

            TryBindSafetySignals();
        }

        if (sparkCheckInAction == null || bulkCheckInAction == null)
        {
            CacheInputActions();
        }

        if (CurrentState == CheckInFlowState.Active)
        {
            UpdateActiveAttempt();
        }
    }

    public void SetTutorialPhaseActive(bool isActive)
    {
        tutorialPhaseActive = isActive;
        UpdateEligibility();
    }

    public void SetEncounterActive(bool isActive)
    {
        encounterActive = isActive;
        UpdateEligibility();
    }

    public void StartCheckInAttempt()
    {
        if (!IsEligible)
        {
            return;
        }

        SetState(CheckInFlowState.Active);
        ResetHoldTracking();
        UpdatePromptVisibility();
    }

    public void CompleteCheckInAttempt()
    {
        if (!IsActive)
        {
            return;
        }

        SetState(CheckInFlowState.Success);
        checkInPromptUI?.ShowSuccessFeedback();
        ResetHoldTracking();
        OnCheckInSucceeded?.Invoke();
        StartPromptHideDelay();
    }

    public void FailCheckInAttempt()
    {
        if (!IsActive)
        {
            return;
        }

        SetState(CheckInFlowState.Fail);
        checkInPromptUI?.ShowFailureFeedback();
        ResetHoldTracking();
        OnCheckInFailed?.Invoke();
        StartPromptHideDelay();
    }

    private void StartPromptHideDelay()
    {
        if (promptHideCoroutine != null)
        {
            StopCoroutine(promptHideCoroutine);
        }

        promptHideCoroutine = StartCoroutine(HidePromptAfterDelay());
    }

    private System.Collections.IEnumerator HidePromptAfterDelay()
    {
        yield return new WaitForSeconds(feedbackDisplayDuration);
        UpdatePromptVisibility();
        promptHideCoroutine = null;
    }

    public void CancelCheckInAttempt()
    {
        if (!IsActive)
        {
            return;
        }

        SetState(IsEligible ? CheckInFlowState.Eligible : CheckInFlowState.NotEligible);
        ResetHoldTracking();
        UpdatePromptVisibility();
    }

    public void RetryCheckInAttempt()
    {
        if (!IsEligible)
        {
            return;
        }

        SetState(CheckInFlowState.Eligible);

        if (autoActivateWhenEligible)
        {
            StartCheckInAttempt();
            return;
        }

        UpdatePromptVisibility();
    }

    private void TryBindSafetySignals()
    {
        if (isBoundToSafetySignals || safetyMeterManager == null)
        {
            return;
        }

        safetyMeterManager.OnHuddlingStateChanged += HandleHuddlingStateChanged;
        safetyMeterManager.OnDistanceBetweenPlayersChanged += HandleDistanceChanged;

        isHuddling = safetyMeterManager.IsHuddling;
        distanceBetweenPlayers = safetyMeterManager.DistanceBetweenPlayers;
        isBoundToSafetySignals = true;

        UpdateEligibility();
    }

    private void UnbindSafetySignals()
    {
        if (!isBoundToSafetySignals || safetyMeterManager == null)
        {
            return;
        }

        safetyMeterManager.OnHuddlingStateChanged -= HandleHuddlingStateChanged;
        safetyMeterManager.OnDistanceBetweenPlayersChanged -= HandleDistanceChanged;
        isBoundToSafetySignals = false;
    }

    private void HandleHuddlingStateChanged(bool isNowHuddling)
    {
        isHuddling = isNowHuddling;
        UpdateEligibility();
    }

    private void HandleDistanceChanged(float distance)
    {
        distanceBetweenPlayers = distance;
        UpdateEligibility();
    }

    private void UpdateEligibility()
    {
        bool tutorialGatePassed = !requireTutorialPhase || tutorialPhaseActive;
        bool encounterGatePassed = encounterActive;
        bool huddleGatePassed = !requireHuddleForEligibility || isHuddling;
        bool distanceGatePassed = distanceBetweenPlayers <= maxEligibleDistance;
        bool canBeEligible = tutorialGatePassed && encounterGatePassed && huddleGatePassed && distanceGatePassed;

        if (IsEligible != canBeEligible)
        {
            IsEligible = canBeEligible;
            OnEligibilityChanged?.Invoke(IsEligible);
        }

        if (!IsEligible)
        {
            if (CurrentState == CheckInFlowState.Active)
            {
                SetState(CheckInFlowState.Fail);
                checkInPromptUI?.ShowFailureFeedback();
                ResetHoldTracking();
                OnCheckInFailed?.Invoke();
            }
            else
            {
                SetState(CheckInFlowState.NotEligible);
            }

            UpdatePromptVisibility();
            return;
        }

        if (CurrentState == CheckInFlowState.NotEligible)
        {
            SetState(CheckInFlowState.Eligible);
        }

        if (CurrentState == CheckInFlowState.Eligible && autoActivateWhenEligible)
        {
            SetState(CheckInFlowState.Active);
        }

        UpdatePromptVisibility();
    }

    private void UpdatePromptVisibility()
    {
        if (checkInPromptUI == null)
        {
            OnPromptVisibilityChanged?.Invoke(false);
            return;
        }

        bool isActive = CurrentState == CheckInFlowState.Active;
        checkInPromptUI.SetPromptState(isActive);

        bool shouldShowPrompt = CurrentState == CheckInFlowState.Eligible || CurrentState == CheckInFlowState.Active;
        checkInPromptUI.SetPromptVisible(shouldShowPrompt);

        if (shouldShowPrompt)
        {
            UpdateProgressUI();
        }

        OnPromptVisibilityChanged?.Invoke(shouldShowPrompt);
    }

    private void SetState(CheckInFlowState nextState)
    {
        if (CurrentState == nextState)
        {
            return;
        }

        CurrentState = nextState;
        OnCheckInStateChanged?.Invoke(CurrentState);
    }

    private void CacheInputActions()
    {
        sparkCheckInAction = sparkPlayerInput != null ? sparkPlayerInput.actions[CheckInActionName] : null;
        bulkCheckInAction = bulkPlayerInput != null ? bulkPlayerInput.actions[CheckInActionName] : null;
    }

    private void UpdateActiveAttempt()
    {
        if (sparkCheckInAction == null || bulkCheckInAction == null)
        {
            return;
        }

        bool sparkHolding = sparkCheckInAction.IsPressed();
        bool bulkHolding = bulkCheckInAction.IsPressed();
        bool bothHolding = sparkHolding && bulkHolding;

        sparkHoldDuration = sparkHolding ? sparkHoldDuration + Time.deltaTime : 0f;
        bulkHoldDuration = bulkHolding ? bulkHoldDuration + Time.deltaTime : 0f;

        UpdateProgressUI();

        float clampedRequiredDuration = Mathf.Max(requiredHoldDuration, 0.01f);
        bool successReached = sparkHoldDuration >= clampedRequiredDuration && bulkHoldDuration >= clampedRequiredDuration;

        if (bothHolding && successReached)
        {
            CompleteCheckInAttempt();
            return;
        }

        if (!bothHolding && wasBothHolding)
        {
            FailCheckInAttempt();
            return;
        }

        wasBothHolding = bothHolding;
    }

    private void UpdateProgressUI()
    {
        float clampedRequiredDuration = Mathf.Max(requiredHoldDuration, 0.01f);
        float sparkProgress = Mathf.Clamp01(sparkHoldDuration / clampedRequiredDuration);
        float bulkProgress = Mathf.Clamp01(bulkHoldDuration / clampedRequiredDuration);
        checkInPromptUI?.SetProgress(sparkProgress, bulkProgress);
    }

    private void ResetHoldTracking()
    {
        sparkHoldDuration = 0f;
        bulkHoldDuration = 0f;
        wasBothHolding = false;
        UpdateProgressUI();
    }
}

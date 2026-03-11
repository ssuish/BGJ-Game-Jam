using UnityEngine;

public class ToggleEligiblityGates : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CheckInManager checkInManager;

    [Header("Debug UI")]
    [SerializeField] private bool showDebugWindow = true;
    [SerializeField] private Rect windowRect = new(20f, 20f, 360f, 280f);

    [Header("Initial Gate Values")]
    [SerializeField] private bool tutorialActiveOnStart = true;
    [SerializeField] private bool encounterActiveOnStart = true;

    private bool tutorialGateActive;
    private bool encounterGateActive;

    private void Awake()
    {
        if (checkInManager == null)
        {
            checkInManager = FindFirstObjectByType<CheckInManager>();
        }

        tutorialGateActive = tutorialActiveOnStart;
        encounterGateActive = encounterActiveOnStart;
    }

    private void Start()
    {
        ApplyGateValues();
    }

    private void OnGUI()
    {
        if (!showDebugWindow)
        {
            return;
        }

        windowRect = GUI.Window(GetInstanceID(), windowRect, DrawWindow, "Check-In Debug");
    }

    public void SetDebugWindowVisible(bool visible)
    {
        showDebugWindow = visible;
    }

    public void SetTutorialGate(bool isActive)
    {
        tutorialGateActive = isActive;
        checkInManager?.SetTutorialPhaseActive(tutorialGateActive);
    }

    public void SetEncounterGate(bool isActive)
    {
        encounterGateActive = isActive;
        checkInManager?.SetEncounterActive(encounterGateActive);
    }

    private void DrawWindow(int windowId)
    {
        GUILayout.BeginVertical();

        if (checkInManager == null)
        {
            GUILayout.Label("CheckInManager not found");

            if (GUILayout.Button("Find Manager"))
            {
                checkInManager = FindFirstObjectByType<CheckInManager>();
                ApplyGateValues();
            }

            GUILayout.EndVertical();
            GUI.DragWindow(new Rect(0f, 0f, 10000f, 22f));
            return;
        }

        GUILayout.Label($"State: {checkInManager.CurrentState}");
        GUILayout.Label($"Eligible: {checkInManager.IsEligible}");
        GUILayout.Label($"Active: {checkInManager.IsActive}");

        GUILayout.Space(8f);

        bool newTutorialGateActive = GUILayout.Toggle(tutorialGateActive, "Tutorial Gate Active");
        if (newTutorialGateActive != tutorialGateActive)
        {
            SetTutorialGate(newTutorialGateActive);
        }

        bool newEncounterGateActive = GUILayout.Toggle(encounterGateActive, "Encounter Gate Active");
        if (newEncounterGateActive != encounterGateActive)
        {
            SetEncounterGate(newEncounterGateActive);
        }

        GUILayout.Space(8f);

        if (GUILayout.Button("Start Attempt"))
        {
            checkInManager.StartCheckInAttempt();
        }

        if (GUILayout.Button("Force Success"))
        {
            checkInManager.CompleteCheckInAttempt();
        }

        if (GUILayout.Button("Force Fail"))
        {
            checkInManager.FailCheckInAttempt();
        }

        if (GUILayout.Button("Retry"))
        {
            checkInManager.RetryCheckInAttempt();
        }

        GUILayout.EndVertical();
        GUI.DragWindow(new Rect(0f, 0f, 10000f, 22f));
    }

    private void ApplyGateValues()
    {
        checkInManager?.SetTutorialPhaseActive(tutorialGateActive);
        checkInManager?.SetEncounterActive(encounterGateActive);
    }
}

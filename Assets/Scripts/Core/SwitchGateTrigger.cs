using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchGateTrigger : MonoBehaviour
{
    [Header("Gate References")]
    [SerializeField] private List<GameObject> gates = new();

    [Header("Gate Behavior")]
    [SerializeField] private bool gateActiveWhenOpen;
    [SerializeField] private bool isTimed;
    [SerializeField] private float openDurationSeconds = 3f;

    [Header("Activator Tags")]
    [SerializeField] private bool allowSparkTag = true;
    [SerializeField] private bool allowBulkTag = true;

    public bool IsGateOpen { get; private set; }

    private readonly HashSet<Collider2D> activeActivators = new();
    private Coroutine closeTimerCoroutine;

    private void OnDisable()
    {
        if (closeTimerCoroutine != null)
        {
            StopCoroutine(closeTimerCoroutine);
            closeTimerCoroutine = null;
        }

        activeActivators.Clear();
        SetGatesOpen(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        TryActivateSwitch(collision);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        TryActivateSwitch(collision);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!activeActivators.Remove(collision))
        {
            return;
        }

        if (isTimed)
        {
            return;
        }

        if (activeActivators.Count == 0)
        {
            SetGatesOpen(false);
        }
    }

    private void TryActivateSwitch(Collider2D collision)
    {
        if (!IsValidActivator(collision))
        {
            return;
        }

        if (!activeActivators.Add(collision))
        {
            return;
        }

        SetGatesOpen(true);

        if (!isTimed)
        {
            return;
        }

        StartCloseTimer();
    }

    private bool IsValidActivator(Collider2D collision)
    {
        if (allowSparkTag && collision.CompareTag("Spark"))
        {
            return true;
        }

        if (allowBulkTag && collision.CompareTag("Bulk"))
        {
            return true;
        }

        return false;
    }

    private void StartCloseTimer()
    {
        if (closeTimerCoroutine != null)
        {
            StopCoroutine(closeTimerCoroutine);
        }

        closeTimerCoroutine = StartCoroutine(CloseAfterDelay());
    }

    private IEnumerator CloseAfterDelay()
    {
        float clampedDuration = Mathf.Max(0f, openDurationSeconds);

        if (clampedDuration > 0f)
        {
            yield return new WaitForSeconds(clampedDuration);
        }

        SetGatesOpen(false);
        closeTimerCoroutine = null;
    }

    private void SetGatesOpen(bool isOpen)
    {
        if (IsGateOpen == isOpen)
        {
            return;
        }

        IsGateOpen = isOpen;

        for (int index = 0; index < gates.Count; index++)
        {
            GameObject gate = gates[index];

            if (gate == null)
            {
                continue;
            }

            bool shouldBeActive = isOpen ? gateActiveWhenOpen : !gateActiveWhenOpen;
            gate.SetActive(shouldBeActive);
        }
    }
}

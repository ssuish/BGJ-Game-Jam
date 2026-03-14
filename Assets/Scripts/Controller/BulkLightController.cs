using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class BulkLightController : MonoBehaviour
{
    [Header("Light Configuration")]
    [SerializeField] private Light2D bulkLight;
    [SerializeField] private float huddleIntensity = 4f;
    [SerializeField] private float transitionDuration = 0.5f;

    private float defaultIntensity;
    private Coroutine intensityCoroutine;
    private bool isBound;

    private void Awake()
    {
        if (bulkLight != null)
        {
            defaultIntensity = bulkLight.intensity;
        }
    }

    private void Start()
    {
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

    private void TryBindSafetyManager()
    {
        if (isBound || SafetyMeterManager.Instance == null)
        {
            return;
        }

        SafetyMeterManager.Instance.OnHuddlingStateChanged += OnHuddlingStateChanged;
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
            SafetyMeterManager.Instance.OnHuddlingStateChanged -= OnHuddlingStateChanged;
        }

        isBound = false;
    }

    private void OnHuddlingStateChanged(bool isHuddling)
    {
        float targetIntensity = isHuddling ? huddleIntensity : defaultIntensity;

        if (intensityCoroutine != null)
        {
            StopCoroutine(intensityCoroutine);
        }

        intensityCoroutine = StartCoroutine(TransitionIntensity(targetIntensity));
    }

    private IEnumerator TransitionIntensity(float targetIntensity)
    {
        if (bulkLight == null)
        {
            yield break;
        }

        float startIntensity = bulkLight.intensity;
        float elapsed = 0f;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            bulkLight.intensity = Mathf.Lerp(startIntensity, targetIntensity, elapsed / transitionDuration);
            yield return null;
        }

        bulkLight.intensity = targetIntensity;
        intensityCoroutine = null;
    }
}

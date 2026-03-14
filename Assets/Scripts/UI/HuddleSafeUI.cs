using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HuddleSafeUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject safePanel;
    [SerializeField] private TMP_Text safeText;
    [SerializeField] private Image heartIcon;

    [Header("Configuration")]
    [SerializeField] private string safeMessage = "Safe";
    [SerializeField] private float fadeDuration = 0.3f;

    private CanvasGroup canvasGroup;
    private Coroutine fadeCoroutine;
    private bool isBound;

    private void Awake()
    {
        InitializePanel();
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

    private void InitializePanel()
    {
        if (safePanel == null)
        {
            return;
        }

        canvasGroup = safePanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = safePanel.AddComponent<CanvasGroup>();
        }

        if (safeText != null)
        {
            safeText.text = safeMessage;
        }

        SetPanelAlpha(0f);
        safePanel.SetActive(false);
    }

    private void OnHuddlingStateChanged(bool isHuddling)
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        fadeCoroutine = StartCoroutine(FadePanel(isHuddling));
    }

    private IEnumerator FadePanel(bool visible)
    {
        if (safePanel == null)
        {
            yield break;
        }

        if (visible)
        {
            safePanel.SetActive(true);
        }

        float startAlpha = canvasGroup != null ? canvasGroup.alpha : 0f;
        float targetAlpha = visible ? 1f : 0f;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            SetPanelAlpha(Mathf.Lerp(startAlpha, targetAlpha, elapsed / fadeDuration));
            yield return null;
        }

        SetPanelAlpha(targetAlpha);

        if (!visible)
        {
            safePanel.SetActive(false);
        }

        fadeCoroutine = null;
    }

    private void SetPanelAlpha(float alpha)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = alpha;
        }
    }
}

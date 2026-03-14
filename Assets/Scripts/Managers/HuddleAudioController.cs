using System.Collections;
using UnityEngine;

public class HuddleAudioController : MonoBehaviour
{
    [Header("Audio Configuration")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip huddleAmbientClip;
    [SerializeField] private float huddleVolume = 0.7f;
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float fadeOutDuration = 0.5f;

    private Coroutine fadeCoroutine;
    private bool isBound;

    private void Start()
    {
        TryBindSafetyManager();
        InitializeAudioSource();
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

    private void InitializeAudioSource()
    {
        if (audioSource == null)
        {
            return;
        }

        audioSource.volume = 0f;
        audioSource.loop = true;
    }

    private void OnHuddlingStateChanged(bool isHuddling)
    {
        if (isHuddling)
        {
            PlayHuddleAmbient();
        }
        else
        {
            StopHuddleAmbient();
        }
    }

    private void PlayHuddleAmbient()
    {
        if (audioSource == null || huddleAmbientClip == null)
        {
            return;
        }

        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        if (!audioSource.isPlaying)
        {
            audioSource.clip = huddleAmbientClip;
            audioSource.volume = 0f;
            audioSource.Play();
        }

        fadeCoroutine = StartCoroutine(FadeVolume(huddleVolume, fadeInDuration));
    }

    private void StopHuddleAmbient()
    {
        if (audioSource == null)
        {
            return;
        }

        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        fadeCoroutine = StartCoroutine(FadeOutAndStop());
    }

    private IEnumerator FadeVolume(float targetVolume, float duration)
    {
        float startVolume = audioSource.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, targetVolume, elapsed / duration);
            yield return null;
        }

        audioSource.volume = targetVolume;
        fadeCoroutine = null;
    }

    private IEnumerator FadeOutAndStop()
    {
        yield return FadeVolume(0f, fadeOutDuration);
        audioSource.Stop();
    }
}

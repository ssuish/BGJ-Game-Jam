using System.Collections.Generic;
using UnityEngine;

public class DangerMech : MonoBehaviour
{
    [Header("Danger Effect")]
    [SerializeField] private float speedMultiplier = 0.5f;
    [SerializeField] private float drainMultiplierWhenSeparated = 4f;

    private readonly HashSet<BaseCreatureController> affectedCreatures = new();

    private void OnDisable()
    {
        ClearAllEffects();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!collision.CompareTag("Spark") && !collision.CompareTag("Bulk"))
        {
            return;
        }

        BaseCreatureController creature = collision.GetComponent<BaseCreatureController>();

        if (creature == null)
        {
            return;
        }

        affectedCreatures.Add(creature);
        creature.SetSpeedMultiplier(speedMultiplier);
        UpdateDrainEffect();
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Spark") && !collision.CompareTag("Bulk"))
        {
            return;
        }

        BaseCreatureController creature = collision.GetComponent<BaseCreatureController>();

        if (creature == null)
        {
            return;
        }

        RemoveCreatureEffect(creature);
        UpdateDrainEffect();
    }

    private void RemoveCreatureEffect(BaseCreatureController creature)
    {
        if (!affectedCreatures.Remove(creature))
        {
            return;
        }

        creature.SetSpeedMultiplier(1f);
    }

    private void UpdateDrainEffect()
    {
        SafetyMeterManager safetyMeterManager = SafetyMeterManager.Instance;

        if (safetyMeterManager == null)
        {
            return;
        }

        if (affectedCreatures.Count > 0 && !safetyMeterManager.IsHuddling)
        {
            safetyMeterManager.SetDrainRateMultiplier(drainMultiplierWhenSeparated);
            return;
        }

        safetyMeterManager.SetDrainRateMultiplier(1f);
    }

    private void ClearAllEffects()
    {
        foreach (BaseCreatureController creature in affectedCreatures)
        {
            if (creature == null)
            {
                continue;
            }

            creature.SetSpeedMultiplier(1f);
        }

        affectedCreatures.Clear();

        SafetyMeterManager safetyMeterManager = SafetyMeterManager.Instance;

        if (safetyMeterManager == null)
        {
            return;
        }

        safetyMeterManager.SetDrainRateMultiplier(1f);
    }
}

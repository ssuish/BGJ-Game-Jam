using System.Collections.Generic;
using UnityEngine;

public class WinGoalTrigger : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameManager gameManager;

    [Header("Activator Tags")]
    [SerializeField] private bool allowSparkTag = true;
    [SerializeField] private bool allowBulkTag = true;

    private readonly HashSet<Collider2D> activePlayers = new();

    private void Start()
    {
        ResolveGameManager();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        TryTrackPlayer(collision);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        TryTrackPlayer(collision);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        activePlayers.Remove(collision);
    }

    private void TryTrackPlayer(Collider2D collision)
    {
        if (!IsValidPlayer(collision))
        {
            return;
        }

        activePlayers.Add(collision);
        TryTriggerWin();
    }

    private bool IsValidPlayer(Collider2D collision)
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

    private void TryTriggerWin()
    {
        ResolveGameManager();

        if (gameManager == null || !gameManager.IsGameActive)
        {
            return;
        }

        if (activePlayers.Count == 0)
        {
            return;
        }

        SafetyMeterManager safetyMeterManager = SafetyMeterManager.Instance;

        if (safetyMeterManager == null || !safetyMeterManager.IsHuddling)
        {
            return;
        }

        gameManager.WinGame();
    }

    private void ResolveGameManager()
    {
        if (gameManager == null)
        {
            gameManager = FindFirstObjectByType<GameManager>();
        }
    }
}

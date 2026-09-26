using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class TurnController : MonoBehaviour
{
    public static TurnController Singleton;

    // INFO: Actions
    public static UnityAction OnTurnEnd;

    // INFO: Turn tracker
    public Dictionary<GameObject, bool> turnTracker { get; set; } = new();

    private void Awake()
    {
        #region Singleton
        if (Singleton == null)
        {
            Singleton = this;

        }
        else
        {
            Destroy(gameObject);

        }

        #endregion

    }

    [ContextMenu("End Turn")]
    public void EndTurn()
    {
        GameObject currentPlayer = GetCurrentPlayerGO();
        GameObject nextPlayer = GetNextPlayer();

        turnTracker[currentPlayer] = false;
        turnTracker[nextPlayer] = true;

        // Debug.Log($"Turn ended. Next player: {nextPlayer}");
        OnTurnEnd?.Invoke();

    }

    #region Helper
    public void Initialise(GameObject[] players)
    {
        foreach (GameObject playerGO in players)
        {
            turnTracker.Add(playerGO, false);

        }

        // GUARD: Prevent nulls
        if (turnTracker.Count <= 0)
        {
            Debug.LogError($"Ensure the player is spawned and has the \"Player\" tag!");
            return;

        }

        GameObject firstPlayer = turnTracker.Keys.FirstOrDefault();
        turnTracker[firstPlayer] = true;

    }

    public GameObject GetNextPlayer()
    {
        List<GameObject> keys = turnTracker.Keys.ToList();
        GameObject currentPlayer = GetCurrentPlayerGO();
        int currentIndex = keys.IndexOf(currentPlayer);

        // Wrap around to the first player if we're at the end
        int nextIndex = (currentIndex + 1) % keys.Count;

        return keys[nextIndex];
    }

    #endregion

    #region Utility
    public GameObject GetCurrentPlayerGO() => turnTracker.First(p => p.Value == true).Key;

    #endregion






}

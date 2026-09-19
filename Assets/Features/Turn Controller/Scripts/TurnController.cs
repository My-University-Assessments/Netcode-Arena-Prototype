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
    [SerializeField, DictionaryDisplay(keyLabel = "Player GO", valueLabel = "Their Turn")] private Dictionary<GameObject, bool> _playerTurnTracker = new();

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
        GameObject currentPlayer = GetCurrentPlayer();
        GameObject nextPlayer = GetNextPlayer();

        _playerTurnTracker[currentPlayer] = false;
        _playerTurnTracker[nextPlayer] = true;

        // Debug.Log($"Turn ended. Next player: {nextPlayer}");
        OnTurnEnd?.Invoke();

    }

    #region Helper
    public void Initialise(GameObject[] players)
    {
        foreach (GameObject playerGO in players)
        {
            _playerTurnTracker.Add(playerGO, false);

        }

        // GUARD: Prevent nulls
        if (_playerTurnTracker.Count <= 0)
        {
            Debug.LogError($"Ensure the player is spawned and has the \"Player\" tag!");
            return;

        }

        GameObject firstPlayer = _playerTurnTracker.Keys.FirstOrDefault();
        _playerTurnTracker[firstPlayer] = true;

    }

    private GameObject GetNextPlayer()
    {
        List<GameObject> keys = _playerTurnTracker.Keys.ToList();
        GameObject currentPlayer = GetCurrentPlayer();
        int currentIndex = keys.IndexOf(currentPlayer);

        // Wrap around to the first player if we're at the end
        int nextIndex = (currentIndex + 1) % keys.Count;

        return keys[nextIndex];
    }

    #endregion

    #region Utility
    public GameObject GetCurrentPlayer() => _playerTurnTracker.First(p => p.Value == true).Key;

    #endregion






}

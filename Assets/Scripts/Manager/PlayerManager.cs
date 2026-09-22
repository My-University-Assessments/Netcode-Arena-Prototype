using System;
using System.Collections.Generic;
using ArenaPrototype.Util;
using TeamBuilder.Agents.Data;
using TeamBuilder.Agents.Interface;
using TeamBuilder.Agents.Manager;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(NetworkObject))]
public class PlayerManager : NetworkBehaviour
{
    public static PlayerManager Singleton;
    private GameNetworkManager _gameManager => GameNetworkManager.Singleton;

    [SerializeField] private GameObject _playerPrefab;
    [SerializeField, DictionaryDisplay(keyLabel = "Team", valueLabel = "Spawn Position")] private Dictionary<ulong, List<Vector2Int>> _spawnPositions = new();

    public static UnityAction<RpcParams> OnTileClicked;

    [SerializeField] private Dictionary<ulong, NetworkObject> players = new();

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

    #region Events
    private void OnEnable()
    {
        OnTileClicked += AskToMoveRPC;
    }

    private void OnDisable()
    {
        OnTileClicked -= AskToMoveRPC;

    }
    #endregion

    #region Spawn Players
    public bool SpawnPlayers()
    {
        if (_playerPrefab == null)
        {
            Debug.LogError($"Player prefab is null!");
            return false;

        }

        Debug.Log($"<color={LogColours.Unity}>[PLAYER MANAGER]</color> <color={LogColours.Host}>[HOST]</color> Spawning {NetworkManager.ConnectedClientsIds.Count} players");

        foreach (ulong clientId in NetworkManager.ConnectedClientsIds)
        {
            ulong currentClientId = clientId;
            GameObject instance = Instantiate(_playerPrefab);

            NetworkObject netObj = instance.GetComponent<NetworkObject>();
            players.Add(currentClientId, netObj);
            netObj.SpawnAsPlayerObject(currentClientId, true);

        }


        return true;
    }

    #endregion

    #region Handle Player Inputs
    [Rpc(SendTo.Server)]
    private void AskToMoveRPC(RpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;

        // GUARD: Ensure its their turn
        if (players[clientId].gameObject != _gameManager.turnController.GetCurrentPlayer())
        {
            TellPlayerMoveRejectedRPC(RpcTarget.Single(rpcParams.Receive.SenderClientId, RpcTargetUse.Temp));
            return;

        }

        // INFO: Player's Turn \/ \/ \/ \/

    }

    [Rpc(SendTo.SpecifiedInParams)]
    private void TellPlayerMoveRejectedRPC(RpcParams rpcParams)
    {
        Debug.LogWarning($"It is not your turn, move rejected!");

    }

    #endregion

    #region Spawn Player Team
    // [Rpc(SendTo.ClientsAndHost)]
    public void SpawnTeamsRPC()
    {
        foreach (ulong clientId in players.Keys)
        {
            TeamManager playerTeamManager = players[clientId].GetComponent<NetworkPlayerController>().teamManager;
            for (int i = 0; i < playerTeamManager.agentDataList.Count; i++)
            {
                playerTeamManager.SpawnTeam(_spawnPositions[clientId]);
                List<GameObject> playerTeam = playerTeamManager.spawnedAgents;

                foreach (GameObject agent in playerTeam)
                {
                    agent.GetComponent<NetworkObject>().Spawn();

                }
            }
        }
    }
    #endregion

}
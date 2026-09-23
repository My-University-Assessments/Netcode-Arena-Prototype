using System;
using System.Collections.Generic;
using ArenaPrototype.Util;
using TeamBuilder.Agents.Interface;
using TeamBuilder.Agents.Manager;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(NetworkObject))]
public class PlayerManager : NetworkBehaviour
{
    public static PlayerManager Singleton;
    private GameNetworkManager _gameManager => GameNetworkManager.Singleton;

    [SerializeField] private GameObject _playerPrefab;
    [SerializeField, DictionaryDisplay(keyLabel = "Team", valueLabel = "Spawn Position")] private Dictionary<ulong, List<Vector2Int>> _spawnPositions = new();

    // public static UnityAction<RpcParams> OnTileClicked;

    [SerializeField] private Dictionary<ulong, NetworkObject> players = new();
    [SerializeField] private Dictionary<ulong, NetworkObject> playerAgents = new();

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
    public void AskToMoveRPC(Vector3 position, RpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        NetworkPlayerController currentPlayer = _gameManager.turnController.GetCurrentPlayer().GetComponent<NetworkPlayerController>();


        // GUARD: Ensure its their turn
        if (players[clientId].gameObject != currentPlayer.gameObject)
        {
            TellPlayerMoveRejectedRPC(RpcTarget.Single(rpcParams.Receive.SenderClientId, RpcTargetUse.Temp));
            return;

        }

        // INFO: Player's Turn \/ \/ \/ \/
        Debug.Log($"Your turn!");
        currentPlayer.teamManager.spawnedAgents[0].transform.position = position;

    }

    [Rpc(SendTo.SpecifiedInParams)]
    private void TellPlayerMoveRejectedRPC(RpcParams rpcParams)
    {
        Debug.LogWarning($"It is not your turn, move rejected!");

    }

    #endregion

    #region Spawn Player Team
    public void SpawnTeams()
    {
        foreach (ulong clientId in players.Keys)
        {
            TeamManager playerTeamManager = players[clientId].GetComponent<NetworkPlayerController>().teamManager;
            for (int i = 0; i < playerTeamManager.teamList.Count; i++)
            {
                playerTeamManager.SpawnTeam(_spawnPositions[clientId]);
                List<GameObject> playerTeam = playerTeamManager.spawnedAgents;

                List<NetworkObjectReference> agentRefs = new();

                foreach (GameObject agent in playerTeam)
                {
                    NetworkObject networkObject = agent.GetComponent<NetworkObject>();
                    networkObject.SpawnWithOwnership(clientId);
                    agentRefs.Add(networkObject);

                }
                agentRefs.Clear();

            }
        }
    }

    #endregion



}
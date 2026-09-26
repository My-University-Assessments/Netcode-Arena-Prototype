using System;
using System.Collections.Generic;
using ArenaPrototype.Util;
using HealthSystem;
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

    public Dictionary<ulong, NetworkObject> players { get; private set; } = new();

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

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsServer) gameObject.SetActive(false);

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
            TellPlayerBoardSizeRPC(new Vector2Int(_gameManager.networkGridGenerator.boardSize.x * 2, (int)(_gameManager.networkGridGenerator.boardSize.y * 1.5f)), RpcTarget.Single(clientId, RpcTargetUse.Temp));

        }


        return true;
    }

    [Rpc(SendTo.SpecifiedInParams)]
    private void TellPlayerBoardSizeRPC(Vector2Int boardSize, RpcParams rpcParams = default)
    {
        NetworkManager.LocalClient.PlayerObject.GetComponentInChildren<CameraController>().boardBounds = boardSize;
    }

    #endregion

    #region Handle Player Inputs

    #region Moving
    [Rpc(SendTo.Server)]
    public void AskToMoveRPC(NetworkObjectReference agentNetRef, Vector3 position, RpcParams rpcParams = default)
    {
        if (!agentNetRef.TryGet(out NetworkObject agentNet)) return;

        ulong clientId = rpcParams.Receive.SenderClientId;
        NetworkPlayerController currentPlayer = _gameManager.networkTurnController.turnController.GetCurrentPlayerGO().GetComponent<NetworkPlayerController>();
        if (!ValidateMove(clientId)) return;

        // INFO: Player's Turn \/ \/ \/ \/
        for (int i = 0; i < currentPlayer.teamManager.spawnedAgents.Count; i++)
        {
            GameObject currentAgentGO = currentPlayer.teamManager.spawnedAgents[i].gameObject;
            if (currentAgentGO != agentNet.gameObject) continue;

            currentAgentGO.transform.position = new Vector3(position.x, 1f, position.z);

        }

        _gameManager.networkTurnController.EndTurn();

    }

    #endregion

    #region Attacking
    [Rpc(SendTo.Server)]
    public void AskToAttackRPC(NetworkObjectReference attackerNetRef, NetworkObjectReference targetNetRef, RpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        if (!ValidateMove(clientId)) return;

        if (!attackerNetRef.TryGet(out NetworkObject attacker)) return;
        if (!targetNetRef.TryGet(out NetworkObject target)) return;

        if (!target.TryGetComponent(out IDamageable targetDamageable) || !attacker.TryGetComponent(out IDamageable attackerDamageable))
        {
            Debug.LogError($"Failed to get attacker and or target IDamageable component!");
            return;

        }

        targetDamageable.Damage(attackerDamageable.damage);
        if (targetDamageable.currentHealth <= 0) TellAgentDieRPC(targetNetRef);
        attacker.gameObject.transform.position = target.transform.position;

        if (IsGameOver())
        {
            Debug.Log($"Game is over!"); // TODO: Link to game manager
            return;

        }

        _gameManager.networkTurnController.EndTurn();

    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TellAgentDieRPC(NetworkObjectReference networkObjectReference, RpcParams rpcParams = default)
    {
        if (!networkObjectReference.TryGet(out NetworkObject target)) return;
        if (NetworkManager.LocalClientId == rpcParams.Receive.SenderClientId)
            players[(ulong)target.GetComponent<IAgent>().team].GetComponent<TeamManager>().spawnedAgents.Remove(target.gameObject);

        Destroy(target.gameObject);

    }

    #endregion

    [Rpc(SendTo.SpecifiedInParams)]
    private void TellPlayerMoveRejectedRPC(RpcParams rpcParams = default)
    {
        Debug.LogWarning($"It is not your turn, move rejected!");

    }


    #endregion

    #region Spawn Player Team
    public void InitialiseTeamManager()
    {
        foreach (ulong clientId in players.Keys)
        {
            TeamManager playerTeamManager = players[clientId].GetComponent<NetworkPlayerController>().teamManager;
            playerTeamManager.SpawnTeam(_spawnPositions[clientId]);
            List<GameObject> playerTeam = playerTeamManager.spawnedAgents;
            List<NetworkObjectReference> agentRefs = new();

            for (int i = 0; i < playerTeamManager.teamList.Count; i++)
            {
                NetworkObject networkObject = playerTeam[i].GetComponent<NetworkObject>();
                networkObject.SpawnWithOwnership(clientId);
                agentRefs.Add(networkObject);

            }

            bool teamManagerEnabled = _gameManager.networkTurnController.turnController.GetCurrentPlayerGO().GetComponent<TeamManager>() == playerTeamManager;
            SetTeamIdRPC(agentRefs.ToArray(), teamManagerEnabled, RpcTarget.Single(clientId, RpcTargetUse.Temp));

        }

    }

    [Rpc(SendTo.SpecifiedInParams)]
    private void SetTeamIdRPC(NetworkObjectReference[] spawnedAgents, bool teamManagerEnabled = false, RpcParams rpcParams = default)
    {
        TeamManager teamManager = NetworkManager.ConnectedClients[NetworkManager.LocalClientId].PlayerObject.GetComponent<NetworkPlayerController>().teamManager;
        teamManager.yourTeam = (int)NetworkManager.LocalClientId;

        foreach (NetworkObjectReference agent in spawnedAgents)
        {
            if (!agent.TryGet(out NetworkObject netObj)) return;
            netObj.GetComponent<IAgent>().team = teamManager.yourTeam;

        }

        teamManager.enabled = teamManagerEnabled;

    }

    #endregion

    #region Helper
    private bool ValidateMove(ulong clientId)
    {
        NetworkPlayerController currentPlayer = _gameManager.networkTurnController.turnController.GetCurrentPlayerGO().GetComponent<NetworkPlayerController>();

        // GUARD: Ensure its their turn
        if (players[clientId].gameObject != currentPlayer.gameObject)
        {
            TellPlayerMoveRejectedRPC(RpcTarget.Single(clientId, RpcTargetUse.Temp));
            return false;

        }

        return true;
    }

    private bool IsGameOver()
    {
        foreach (NetworkClient client in NetworkManager.ConnectedClients.Values)
        {
            if (client.PlayerObject.GetComponent<TeamManager>().spawnedAgents.Count <= 0)
                return true;

        }

        return false;
    }

    #endregion



}
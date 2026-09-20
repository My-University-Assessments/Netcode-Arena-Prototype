using System;
using System.Collections.Generic;
using ArenaPrototype.Util;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(NetworkObject))]
public class PlayerManager : NetworkBehaviour
{
    public static PlayerManager Singleton;
    private GameNetworkManager _gameManager => GameNetworkManager.Singleton;

    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private List<Transform> _spawnPositions;

    public static UnityAction<RpcParams> OnTileClicked;

    private Dictionary<ulong, NetworkObject> players = new();

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

        for (int i = 0; i < NetworkManager.ConnectedClientsIds.Count; i++)
        {
            ulong currentClientId = NetworkManager.ConnectedClientsIds[i];
            GameObject instance = Instantiate(_playerPrefab);

            int nextIndex = (i + 1) % _spawnPositions.Count;
            instance.transform.position = _spawnPositions[nextIndex].position;
            instance.name = $"Client: {currentClientId}";

            NetworkObject netObj = instance.GetComponent<NetworkObject>();
            netObj.SpawnAsPlayerObject(currentClientId, true);
            players.Add(currentClientId, netObj);

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

}
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ArenaPrototype.Feature.GridSystem;
using NUnit.Framework.Internal;
using Unity.Netcode;
using UnityEditor.MPE;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

[RequireComponent(typeof(NetworkObject))]
public class GameNetworkManager : NetworkBehaviour
{
    private GridGenerator _gridGenerator => GridGenerator.Singleton;
    private TurnController _turnController => TurnController.Singleton;
    private PlayerManager _playerManager => PlayerManager.Singleton;

    public override async void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (EnsureServerOnly())
            await HandleStartGame();

    }

    private async Task HandleStartGame()
    {
        if (_playerManager == null) return;
        if (_gridGenerator == null) return;
        if (_turnController == null) return;


        if (!await _playerManager.SpawnPlayers())
        {
            Debug.LogError($"Players failed to spawn, cannot continue!");
            return;

        }

        HandleGenerateGrid();

    }

    #region Grid
    private void HandleGenerateGrid()
    {
        _gridGenerator.CreateGrid(60, 40);

        foreach (GameObject tile in _gridGenerator.gridTiles.Values)
        {
            NetworkObject networkObject = tile.GetComponent<NetworkObject>();
            networkObject.Spawn();
            FixNameRPC(networkObject, tile.name);

        }

    }

    [Rpc(SendTo.NotServer)]
    private void FixNameRPC(NetworkObjectReference networkObjectReference, string tileName)
    {
        if (!networkObjectReference.TryGet(out NetworkObject networkObject)) return;
        networkObject.name = tileName;

    }

    #endregion

    #region Utility
    private bool EnsureServerOnly()
    {
        if (!IsServer)
        {
            enabled = false;
            return false;
        }

        return true;

    }

    #endregion

}

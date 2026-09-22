using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ArenaPrototype.Feature.GridSystem;
using NUnit.Framework.Internal;
using TeamBuilder.Agents.Manager;
using Unity.Netcode;
using UnityEditor.MPE;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

[RequireComponent(typeof(NetworkObject))]
public class GameNetworkManager : NetworkBehaviour
{
    public static GameNetworkManager Singleton;

    public GridGenerator gridGenerator => GridGenerator.Singleton;
    public TurnController turnController => TurnController.Singleton;
    public PlayerManager playerManager => PlayerManager.Singleton;

    private void Awake()
    {
        if (NetworkManager == null || !NetworkManager.IsListening) transform.root.gameObject.SetActive(false);
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
        turnController.enabled = false;

    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!EnsureServerOnly()) return;

        turnController.enabled = true;
        Invoke(nameof(HandleStartGame), .1f);

    }

    private void HandleStartGame()
    {
        if (playerManager == null) return;
        if (gridGenerator == null) return;
        if (turnController == null) return;


        if (!playerManager.SpawnPlayers())
        {
            Debug.LogError($"Players failed to spawn, cannot continue!");
            return;

        }

        if (!HandleGenerateGrid())
        {
            Debug.LogError($"Failed to spawn grid!");
            return;

        }

        InitialiseTurnController();
        InitialiseTeamManager();

    }

    #region Grid
    private bool HandleGenerateGrid()
    {
        gridGenerator.CreateGrid(60, 40);

        foreach (GameObject tile in gridGenerator.gridTiles.Values)
        {
            NetworkObject networkObject = tile.GetComponent<NetworkObject>();
            networkObject.Spawn();
            FixNameRPC(networkObject, tile.name);

        }

        return true;

    }

    [Rpc(SendTo.NotServer)]
    private void FixNameRPC(NetworkObjectReference networkObjectReference, string tileName)
    {
        if (!networkObjectReference.TryGet(out NetworkObject networkObject)) return;
        networkObject.name = tileName;

    }

    #endregion

    #region Turn Controller
    private void InitialiseTurnController()
    {
        GameObject[] playersGO = new GameObject[NetworkManager.ConnectedClients.Count];

        for (int i = 0; i < NetworkManager.Singleton.ConnectedClients.Count; i++)
            playersGO[i] = NetworkManager.Singleton.ConnectedClients[(ulong)i].PlayerObject.gameObject;

        if (playersGO.Length <= 0)
        {
            Debug.LogError($"No players provided!");
            return;

        }

        turnController.Initialise(playersGO);

    }
    #endregion

    #region Team Manager
    private void InitialiseTeamManager()
    {
        playerManager.SpawnTeamsRPC();
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

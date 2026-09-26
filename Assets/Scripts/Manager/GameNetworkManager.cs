using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        if (!IsServer)
        {
            if (playerManager != null) playerManager.enabled = false;
            if (turnController != null) turnController.enabled = false;
            // if (gridGenerator != null) gridGenerator.enabled = false;

            enabled = false;
            return;

        }

        Invoke(nameof(HandleStartGame), .1f);

    }

    private async void HandleStartGame()
    {
        // GUARD: Prevent nulls!
        if (playerManager == null) { Debug.LogError("PlayerManager singleton not found!"); return; }
        if (gridGenerator == null) { Debug.LogError("GridGenerator singleton not found!"); return; }
        if (turnController == null) { Debug.LogError("TurnController singleton not found!"); return; }

        if (!await SpawnNetworkGridAsync())
        {
            Debug.LogError($"Failed to spawn grid!");
            return;

        }

        if (!playerManager.SpawnPlayers())
        {
            Debug.LogError($"Players failed to spawn, cannot continue!");
            return;

        }

        InitialiseTurnController();
        InitialiseTeamManager();

    }

    #region Grid
    [Rpc(SendTo.NotServer)]
    private void FixNameRPC(NetworkObjectReference networkObjectReference, string tileName, Vector3 tilePosition)
    {
        if (!networkObjectReference.TryGet(out NetworkObject networkObject)) return;
        networkObject.transform.position = tilePosition;
        networkObject.name = tileName;
    }

    private async Awaitable<bool> SpawnNetworkGridAsync(int batchSize = 10)
    {
        gridGenerator.CreateGrid(new Vector2Int(40, 60), GridLayout.CellLayout.Hexagon, new Vector3(1.02f, 1.02f));
        if (NetworkManager.ConnectedClients.Count <= 1) return true;

        List<KeyValuePair<Vector3Int, GameObject>> tilesList = new List<KeyValuePair<Vector3Int, GameObject>>(gridGenerator.gridTiles);

        if (tilesList.Count <= 0) return false;
        batchSize = Mathf.Clamp(batchSize, 1, tilesList.Count);

        for (int i = 0; i < tilesList.Count; i += batchSize)
        {
            int end = Mathf.Min(i + batchSize, tilesList.Count);

            for (int j = i; j < end; j++)
            {
                NetworkObject networkObject = tilesList[j].Value.GetComponent<NetworkObject>();
                networkObject.Spawn();
                FixNameRPC(networkObject, networkObject.name, networkObject.transform.position);
                PassGridTilesRPC(tilesList[j].Key, networkObject);
            }

            await Awaitable.WaitForSecondsAsync(.01f);
        }

        return true;
    }

    [Rpc(SendTo.NotAuthority)]
    private void PassGridTilesRPC(Vector3Int position, NetworkObjectReference gridTile)
    {
        gridGenerator.gridTiles.Add(position, gridTile);
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
        playerManager.SpawnTeams();
    }

    #endregion


}

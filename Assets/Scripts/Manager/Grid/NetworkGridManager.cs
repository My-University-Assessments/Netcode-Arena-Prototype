using System.Collections.Generic;
using ArenaPrototype.Feature.GridSystem;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
[RequireComponent(typeof(GridGenerator))]
public class NetworkGridManager : NetworkBehaviour
{
    public static NetworkGridManager Singleton;
    private GridGenerator _gridGenerator => GridGenerator.Singleton;

    [field: SerializeField] public Vector2Int boardSize { get; private set; }

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

        if (!IsServer) enabled = false;

    }

    public async Awaitable<bool> SpawnNetworkGridAsync(int batchSize = 10)
    {
        if (boardSize == Vector2.zero)
        {
            Debug.LogError($"Board size is 0!");
            return false;

        }

        _gridGenerator.CreateGrid(boardSize, GridLayout.CellLayout.Hexagon, new Vector3(1.02f, 1.02f));
        if (NetworkManager.ConnectedClients.Count <= 1) return true;

        List<KeyValuePair<Vector3Int, GameObject>> tilesList = new List<KeyValuePair<Vector3Int, GameObject>>(_gridGenerator.gridTiles);

        if (tilesList.Count <= 0)
        {
            Debug.LogError($"Failed to spawn grid!");
            return false;

        }

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

    [Rpc(SendTo.NotServer)]
    private void FixNameRPC(NetworkObjectReference networkObjectReference, string tileName, Vector3 tilePosition)
    {
        if (!networkObjectReference.TryGet(out NetworkObject networkObject)) return;
        networkObject.transform.position = tilePosition;
        networkObject.name = tileName;
    }



    [Rpc(SendTo.NotAuthority)]
    private void PassGridTilesRPC(Vector3Int position, NetworkObjectReference gridTile)
    {
        _gridGenerator.gridTiles.Add(position, gridTile);

    }

    #region Utility
    public Dictionary<Vector3Int, GameObject> GetGridTiles() => _gridGenerator.gridTiles;

    #endregion

}
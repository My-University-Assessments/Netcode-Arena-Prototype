using ArenaPrototype.Feature.GridSystem.Interface;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public class NetworkPlayerController : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsOwner)
        {
            enabled = false;
            return;

        }

        GridTile.OnTileClicked += ClickedTile;

    }

    #region Events
    private void OnEnable()
    {

    }

    private void OnDisable()
    {
        GridTile.OnTileClicked -= ClickedTile;

    }
    #endregion

    private void ClickedTile(IGridTile tileClicked)
    {
        Debug.Log($"Clicked Tile: {tileClicked.gameObject.name}");
        PlayerManager.OnTileClicked?.Invoke(default);

    }

    private void TellServerTileClickedRPC()
    {

    }

}
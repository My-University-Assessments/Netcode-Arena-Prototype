using ArenaPrototype.Feature.GridSystem.Interface;
using TeamBuilder.Agents.Base;
using TeamBuilder.Agents.Manager;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public class NetworkPlayerController : NetworkBehaviour
{

    [field: SerializeField] public TeamManager teamManager { get; private set; }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsOwner)
        {
            teamManager.enabled = false;
            enabled = false;
            return;

        }

        Agent.OnAgentMove += Test;
        // GridTile.OnTileClicked += ClickedTile;

    }

    #region Events
    private void OnEnable()
    {

    }

    private void OnDisable()
    {
        Agent.OnAgentMove -= Test;
        // GridTile.OnTileClicked -= ClickedTile;

    }
    #endregion

    private void Test(GameObject gameObject)
    {
        Debug.Log($"Moving: {gameObject}");
    }

    private void ClickedTile(IGridTile tileClicked)
    {
        Debug.Log($"Clicked Tile: {tileClicked.gameObject.name}");
        PlayerManager.OnTileClicked?.Invoke(default);

    }

    private void TellServerTileClickedRPC()
    {

    }

}
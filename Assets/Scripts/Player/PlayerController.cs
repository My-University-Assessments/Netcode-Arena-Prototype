using ArenaPrototype.Feature.GridSystem.Interface;
using TeamBuilder.Agents.Base;
using TeamBuilder.Agents.Manager;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public class NetworkPlayerController : NetworkBehaviour
{

    [field: SerializeField] public TeamManager teamManager { get; private set; }

    private void Awake()
    {
        teamManager.enabled = false;

    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsOwner)
        {
            Camera camera = GetComponentInChildren<Camera>();
            camera.enabled = false;
            camera.GetComponent<AudioListener>().enabled = false;

            enabled = false;
            return;

        }
        teamManager.enabled = true;
        teamManager.yourTeam = (int)NetworkManager.LocalClientId;

        Agent.OnAgentMove += Test;

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

    private void Test(Vector3 position)
    {
        GameNetworkManager.Singleton.playerManager.AskToMoveRPC(position);

    }

    private void ClickedTile(IGridTile tileClicked)
    {
        Debug.Log($"Clicked Tile: {tileClicked.gameObject.name}");
        // PlayerManager.OnTileClicked?.Invoke(default);

    }

    private void TellServerTileClickedRPC()
    {

    }

}
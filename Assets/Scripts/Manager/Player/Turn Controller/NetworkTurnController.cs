using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using TeamBuilder.Agents.Manager;
using System.Linq;

[RequireComponent(typeof(NetworkObject))]
[RequireComponent(typeof(TurnController))]
public class NetworkTurnController : NetworkBehaviour
{
    public static NetworkTurnController Singleton;
    private GameNetworkManager _gameNetworkManager => GameNetworkManager.Singleton;
    public TurnController turnController => TurnController.Singleton;

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

    public void EndTurn()
    {
        ToggleTeamManagerRPC(false, RpcTarget.Single(GetCurrentPlayerId(), RpcTargetUse.Temp));
        turnController.EndTurn();
        ToggleTeamManagerRPC(true, RpcTarget.Single(GetCurrentPlayerId(), RpcTargetUse.Temp));


    }

    [Rpc(SendTo.SpecifiedInParams)]
    private void ToggleTeamManagerRPC(bool status = true, RpcParams rpcParams = default)
    {
        NetworkManager.LocalClient.PlayerObject.GetComponent<TeamManager>().enabled = status;

    }

    public void InitialiseTurnController()
    {
        List<GameObject> playerObjects = new();
        foreach (var playerObject in _gameNetworkManager.playerManager.players.Values)
        {
            GameObject value = playerObject.gameObject;
            playerObjects.Add(value);

        }


        turnController.Initialise(playerObjects.ToArray());

    }


    #region Utility
    private ulong GetCurrentPlayerId() => turnController.GetCurrentPlayerGO().GetComponent<NetworkObject>().OwnerClientId;

    #endregion

}
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


    #region Events
    private void OnEnable()
    {
        Agent.OnAgentMove += Move;
        Agent.OnAgentAttack += Attack;

    }

    private void OnDisable()
    {
        Agent.OnAgentMove -= Move;
        Agent.OnAgentAttack -= Attack;


    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsOwner)
        {
            // Camera camera = 
            GetComponentInChildren<Camera>().gameObject.SetActive(false);
            GetComponentInChildren<CameraController>().enabled = false;
            Agent.OnAgentMove -= Move;
            Agent.OnAgentAttack -= Attack;

            enabled = false;
            return;

        }

        teamManager.yourTeam = (int)NetworkManager.LocalClientId;
        teamManager.enabled = true;


    }

    #endregion

    private void Move(GameObject agentNetRef, Vector3 position)
    {
        GameNetworkManager.Singleton.playerManager.AskToMoveRPC(agentNetRef.GetComponent<NetworkObject>(), position);

    }

    private void Attack(GameObject attacker, GameObject target)
    {
        GameNetworkManager.Singleton.playerManager.AskToAttackRPC(attacker.GetComponent<NetworkObject>(), target.GetComponent<NetworkObject>());

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
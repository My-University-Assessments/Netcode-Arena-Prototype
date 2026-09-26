using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public class GameNetworkManager : NetworkBehaviour
{
    public static GameNetworkManager Singleton;

    #region Singleton Components
    public NetworkGridManager networkGridGenerator => NetworkGridManager.Singleton;
    public NetworkTurnController networkTurnController => NetworkTurnController.Singleton;
    public PlayerManager playerManager => PlayerManager.Singleton;

    #endregion



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

    private bool FindRequiredComponents() => playerManager != null && networkTurnController != null && networkGridGenerator != null;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!FindRequiredComponents())
        {
            Debug.LogError($"Failed to find all required components!");
            enabled = false;
            return;

        }

        if (!IsServer)
        {
            enabled = false;
            return;

        }

        Invoke(nameof(HandleStartGame), .1f);

    }

    private async void HandleStartGame()
    {
        // GUARD: Prevent nulls!
        if (playerManager == null) { Debug.LogError("PlayerManager singleton not found!"); return; }
        if (networkGridGenerator == null) { Debug.LogError("GridGenerator singleton not found!"); return; }
        if (networkTurnController == null) { Debug.LogError("TurnController singleton not found!"); return; }

        if (!await networkGridGenerator.SpawnNetworkGridAsync())
        {

            return;

        }

        if (!playerManager.SpawnPlayers())
        {
            Debug.LogError($"Players failed to spawn, cannot continue!");
            return;

        }

        networkTurnController.InitialiseTurnController();
        playerManager.InitialiseTeamManager();

    }

}

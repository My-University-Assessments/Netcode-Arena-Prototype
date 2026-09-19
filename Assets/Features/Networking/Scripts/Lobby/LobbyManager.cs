using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using ArenaPrototype.Util;
using System;
using Unity.Netcode.Transports.UTP;
using Unity.Netcode;
using System.Threading.Tasks;
using UnityEngine.Events;

public class LobbyManager : MonoBehaviour
{

    public static UnityAction OnLobbyCreated;
    public static UnityAction OnLobbyJoined;

    private static IRelayService relayService => RelayService.Instance;

    private async void Start()
    {
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();

    }

    #region Create Lobby
    public static async Awaitable<bool> CreateLobbyAsync(int maxMembers)
    {
        try
        {
            Allocation allocation = await relayService.CreateAllocationAsync(maxMembers);
            string joinCode = await relayService.GetJoinCodeAsync(allocation.AllocationId);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(AllocationUtils.ToRelayServerData(allocation, "dtls"));

            Debug.Log($"<color={LogColours.Lobby}>[LOBBY]</color> Lobby created!. Region: {allocation.Region} | Join Code: {joinCode}");
            OnLobbyCreated?.Invoke();

            return true;

        }
        catch (RelayServiceException ex)
        {
            Debug.LogError(ex);
            return false;
        }
    }

    #endregion

    #region Join Lobby
    public static async Awaitable<bool> JoinLobbyAsync(string joinCode)
    {
        if (string.IsNullOrEmpty(joinCode)) { Debug.LogWarning($"Join code is null!"); return false; }

        try
        {
            JoinAllocation joinAllocation = await relayService.JoinAllocationAsync(joinCode);
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(AllocationUtils.ToRelayServerData(joinAllocation, "dtls"));
            Debug.Log($"<color={LogColours.Lobby}>[LOBBY]</color> Joined lobby!. Region: {joinAllocation.Region} | Join Code: {joinCode}");
            OnLobbyJoined?.Invoke();

            return true;

        }
        catch (RelayServiceException ex)
        {
            Debug.LogError(ex);
            return false;

        }
    }

    #endregion

}

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

public class LobbyManager : MonoBehaviour
{

    public static Action OnLobbyCreated;
    private static IRelayService relayService => RelayService.Instance;

    private async void Start()
    {
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();

    }

    public static async Task CreateLobbyAsync(int maxMembers)
    {
        try
        {
            IRelayService relayService = RelayService.Instance;
            Allocation allocation = await relayService.CreateAllocationAsync(maxMembers);
            string joinCode = await relayService.GetJoinCodeAsync(allocation.AllocationId);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(AllocationUtils.ToRelayServerData(allocation, "dtls"));

            Debug.Log($"<color={LogColours.Lobby}>[LOBBY]</color> Lobby created!. Region: {allocation.Region} | Join Code: {joinCode}");
            OnLobbyCreated?.Invoke();

        }
        catch (RelayServiceException ex)
        {
            Debug.LogError(ex.Message);

        }
    }

    public static async Task JoinLobbyAsync(string joinCode)
    {
        try
        {
            if (string.IsNullOrEmpty(joinCode)) { Debug.LogWarning($"Join code is null!"); return; }
            JoinAllocation joinAllocation = await relayService.JoinAllocationAsync(joinCode);
            if (joinAllocation.AllocationId == null) { Debug.LogError($"Allocation Id is null!"); return; }

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(AllocationUtils.ToRelayServerData(joinAllocation, "dtls"));

            Debug.Log($"<color={LogColours.Lobby}>[LOBBY]</color> Joined lobby!. Region: {joinAllocation.Region} | Join Code: {joinCode}");


        }
        catch (RelayServiceException ex)
        {
            Debug.LogError(ex.Message);

        }
    }
}

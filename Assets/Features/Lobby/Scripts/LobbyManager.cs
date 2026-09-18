using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using ArenaPrototype.Util;
using System;

public class LobbyManager : MonoBehaviour
{

    public static Action OnLobbyCreated;
    private static IRelayService relayService => RelayService.Instance;

    private async void Start()
    {
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();

    }

    public static async void CreateLobbyAsync(int maxMembers)
    {
        try
        {
            IRelayService relayService = RelayService.Instance;
            Allocation allocation = await relayService.CreateAllocationAsync(maxMembers);
            string joinCode = await relayService.GetJoinCodeAsync(allocation.AllocationId);

            Debug.Log($"<color={LogColours.Lobby}>[LOBBY]</color> Lobby created. Region: {allocation.Region} | Join Code: {joinCode}");
            OnLobbyCreated?.Invoke();

        }
        catch (RelayServiceException ex)
        {
            Debug.LogError(ex.Message);

        }
    }

    public static async void JoinLobbyAsync(string joinCode)
    {
        try
        {
            await relayService.JoinAllocationAsync(joinCode);
            Debug.Log($"<color={LogColours.Lobby}>[LOBBY]</color> Joined lobby {joinCode}.");


        }
        catch (RelayServiceException ex)
        {
            Debug.LogError(ex.Message);

        }
    }
}

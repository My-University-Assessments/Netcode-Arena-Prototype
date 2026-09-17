using ArenaPrototype.Util;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public class UnityNetworkHelper : NetworkBehaviour
{

    #region Events
    private void OnEnable()
    {
        NetworkEventManager.OnRequestStartUnityHost += StartUnityHost;
        NetworkEventManager.OnRequestStopUnityHost += StopUnityHost;

        NetworkEventManager.OnRequestStartUnityClient += StartUnityClient;
        NetworkEventManager.OnRequestStopUnityClient += StopUnityClient;

    }

    private void OnDisable()
    {
        NetworkEventManager.OnRequestStartUnityHost -= StartUnityHost;
        NetworkEventManager.OnRequestStopUnityHost -= StopUnityHost;

        NetworkEventManager.OnRequestStartUnityClient -= StartUnityClient;
        NetworkEventManager.OnRequestStopUnityClient -= StopUnityClient;

        if (NetworkManager == null) return;
        NetworkManager.OnServerStarted -= OnServerStarted;
        NetworkManager.OnServerStopped -= OnServerStopped;

        NetworkManager.OnClientStarted -= OnClientStarted;
        NetworkManager.OnClientStopped -= OnClientStopped;

    }
    #endregion


    #region Host
    private void StartUnityHost()
    {
        if (!FindNetworkManager()) return;

        try
        {
            NetworkManager.OnServerStarted += OnServerStarted;
            NetworkManager.OnServerStopped += OnServerStopped;
            if (!NetworkManager.StartHost()) return;


        }
        catch (System.Exception ex)
        {
            Debug.LogError($"{ex.Message}");

        }
    }

    private void StopUnityHost()
    {
        StopUnityClient();

    }

    #region Unity
    protected virtual void OnServerStarted()
    {
        Debug.Log($"{CheckPrivilege()} Unity Host has started!");
        NetworkEventManager.OnUnityHostStarted?.Invoke();
    }

    protected virtual void OnServerStopped(bool wasHost)
    {
        NetworkManager.OnServerStarted -= OnServerStarted;
        NetworkManager.OnServerStopped -= OnServerStopped;

        Debug.Log($"{CheckPrivilege()} Unity Host has stopped!");
        NetworkEventManager.OnUnityHostStopped?.Invoke();

    }

    #endregion

    #endregion

    #region Client
    private void StartUnityClient()
    {
        if (!FindNetworkManager()) return;

        try
        {
            NetworkManager.OnClientStarted += OnClientStarted;
            NetworkManager.OnClientStopped += OnClientStopped;

            if (!NetworkManager.StartClient()) return;

        }
        catch (System.Exception ex)
        {
            Debug.LogError($"{ex.Message}");

        }

    }

    private void StopUnityClient()
    {
        if (!FindNetworkManager()) return;

        try
        {
            NetworkManager.Shutdown();

        }
        catch (System.Exception ex)
        {
            Debug.LogError($"{ex.Message}");

        }

    }

    #region Unity
    protected virtual void OnClientStarted()
    {
        Debug.Log($"{CheckPrivilege()} Unity Client has started!");
        NetworkEventManager.OnUnityClientStarted?.Invoke();

    }

    protected virtual void OnClientStopped(bool wasHost)
    {
        NetworkManager.OnClientStarted -= OnClientStarted;
        NetworkManager.OnClientStopped -= OnClientStopped;

        Debug.Log($"{CheckPrivilege()} Unity Client has stopped!");
        NetworkEventManager.OnUnityClientStopped?.Invoke();

    }

    #endregion

    #endregion

    #region Utility
    public static string CheckPrivilege()
    {
        if (!NetworkManager.Singleton.IsListening) return $"<color={LogColours.Unity}>[UNITY]</color>";

        switch (NetworkManager.Singleton.IsHost)
        {
            case true:
                return $"<color={LogColours.Unity}>[UNITY]</color> <color={LogColours.Host}>[HOST]</color>";
            case false:
                return $"<color={LogColours.Unity}>[UNITY]</color> <color={LogColours.Client}>[CLIENT]</color>";
        }

    }

    private bool FindNetworkManager()
    {
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError($"Network Manager is null!");
            return false;

        }

        return true;

    }
    #endregion
}

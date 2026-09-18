using ArenaPrototype.Util;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public class UnityNetworkHelper : MonoBehaviour
{

    #region Events
    private void OnEnable()
    {
        // INFO: Host
        NetworkEventManager.OnRequestStartUnityHost += StartUnityHost;
        NetworkEventManager.OnRequestStopUnityHost += StopUnityHost;

        // INFO: Client
        NetworkEventManager.OnRequestStartUnityClient += StartUnityClient;
        NetworkEventManager.OnRequestStopUnityClient += StopUnityClient;

    }

    private void OnDisable()
    {
        // INFO: Host
        NetworkEventManager.OnRequestStartUnityHost -= StartUnityHost;
        NetworkEventManager.OnRequestStopUnityHost -= StopUnityHost;

        // INFO: Client
        NetworkEventManager.OnRequestStartUnityClient -= StartUnityClient;
        NetworkEventManager.OnRequestStopUnityClient -= StopUnityClient;

        if (NetworkManager.Singleton == null) return;
        NetworkManager.Singleton.OnServerStarted -= OnServerStarted;
        NetworkManager.Singleton.OnServerStopped -= OnServerStopped;

        NetworkManager.Singleton.OnClientStarted -= OnClientStarted;
        NetworkManager.Singleton.OnClientStopped -= OnClientStopped;

    }
    #endregion

    #region Host
    private void StartUnityHost()
    {
        if (NetworkManager.Singleton == null) return;

        try
        {
            NetworkManager.Singleton.OnServerStarted += OnServerStarted;
            NetworkManager.Singleton.OnServerStopped += OnServerStopped;

            NetworkManager.Singleton.StartHost();

            NetworkManager.Singleton.SceneManager.ActiveSceneSynchronizationEnabled = true;
            NetworkManager.Singleton.SceneManager.PostSynchronizationSceneUnloading = true;
            NetworkManager.Singleton.SceneManager.SetClientSynchronizationMode(UnityEngine.SceneManagement.LoadSceneMode.Additive);



        }
        catch (System.Exception ex)
        {
            Debug.LogError($"{ex.Message}");

        }
    }

    private void StopUnityHost()
    {
        if (NetworkManager.Singleton == null) return;
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
        NetworkManager.Singleton.OnServerStarted -= OnServerStarted;
        NetworkManager.Singleton.OnServerStopped -= OnServerStopped;

        Debug.Log($"{CheckPrivilege()} Unity Host has stopped!");
        NetworkEventManager.OnUnityHostStopped?.Invoke();

    }

    #endregion

    #endregion

    #region Client
    private void StartUnityClient()
    {
        if (NetworkManager.Singleton == null) return;

        try
        {
            NetworkManager.Singleton.OnClientStarted += OnClientStarted;
            NetworkManager.Singleton.OnClientStopped += OnClientStopped;

            NetworkManager.Singleton.StartClient();

        }
        catch (System.Exception ex)
        {
            Debug.LogError($"{ex.Message}");

        }

    }

    private void StopUnityClient()
    {
        if (NetworkManager.Singleton == null) return;

        try
        {
            NetworkManager.Singleton.Shutdown();

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
        NetworkManager.Singleton.OnClientStarted -= OnClientStarted;
        NetworkManager.Singleton.OnClientStopped -= OnClientStopped;

        Debug.Log($"{CheckPrivilege()} Unity Client has stopped!");
        NetworkEventManager.OnUnityClientStopped?.Invoke();

    }

    #endregion

    #endregion

    #region Utility
    public static string CheckPrivilege()
    {
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening) return $"<color={LogColours.Unity}>[UNITY]</color>";

        switch (NetworkManager.Singleton.IsHost)
        {
            case true:
                return $"<color={LogColours.Unity}>[UNITY]</color> <color={LogColours.Host}>[HOST]</color>";
            case false:
                return $"<color={LogColours.Unity}>[UNITY]</color> <color={LogColours.Client}>[CLIENT]</color>";
        }

    }

    #endregion
}

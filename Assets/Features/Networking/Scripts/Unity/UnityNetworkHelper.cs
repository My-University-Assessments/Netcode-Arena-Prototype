using ArenaPrototype.Util;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(NetworkManager))]
public class UnityNetworkHelper : MonoBehaviour
{
    private static NetworkManager _networkManager;

    private void Awake()
    {
        _networkManager = GetComponent<NetworkManager>();

    }

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

        if (_networkManager == null) return;
        _networkManager.OnServerStarted -= OnServerStarted;
        _networkManager.OnServerStopped -= OnServerStopped;

        _networkManager.OnClientStarted -= OnClientStarted;
        _networkManager.OnClientStopped -= OnClientStopped;

    }
    #endregion

    #region Host
    private void StartUnityHost()
    {
        try
        {
            _networkManager.OnServerStarted += OnServerStarted;
            _networkManager.OnServerStopped += OnServerStopped;

            _networkManager.StartHost();

            _networkManager.SceneManager.ActiveSceneSynchronizationEnabled = true;
            _networkManager.SceneManager.PostSynchronizationSceneUnloading = true;
            _networkManager.SceneManager.SetClientSynchronizationMode(UnityEngine.SceneManagement.LoadSceneMode.Additive);



        }
        catch (System.Exception ex)
        {
            Debug.LogError($"{ex.Message}");

        }
    }

    private void StopUnityHost()
    {
        if (_networkManager == null) return;
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
        _networkManager.OnServerStarted -= OnServerStarted;
        _networkManager.OnServerStopped -= OnServerStopped;

        Debug.Log($"{CheckPrivilege()} Unity Host has stopped!");
        NetworkEventManager.OnUnityHostStopped?.Invoke();

    }

    #endregion

    #endregion

    #region Client
    private void StartUnityClient()
    {
        if (_networkManager == null) return;

        try
        {
            _networkManager.OnClientStarted += OnClientStarted;
            _networkManager.OnClientStopped += OnClientStopped;

            _networkManager.StartClient();

        }
        catch (System.Exception ex)
        {
            Debug.LogError($"{ex.Message}");

        }

    }

    private void StopUnityClient()
    {
        if (_networkManager == null) return;

        try
        {
            _networkManager.Shutdown();

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
        _networkManager.OnClientStarted -= OnClientStarted;
        _networkManager.OnClientStopped -= OnClientStopped;

        Debug.Log($"{CheckPrivilege()} Unity Client has stopped!");
        NetworkEventManager.OnUnityClientStopped?.Invoke();

    }

    #endregion

    #endregion

    #region Utility
    public static string CheckPrivilege()
    {
        if (_networkManager == null || !_networkManager.IsListening) return $"<color={LogColours.Unity}>[UNITY]</color>";

        switch (_networkManager.IsHost)
        {
            case true:
                return $"<color={LogColours.Unity}>[UNITY]</color> <color={LogColours.Host}>[HOST]</color>";
            case false:
                return $"<color={LogColours.Unity}>[UNITY]</color> <color={LogColours.Client}>[CLIENT]</color>";
        }

    }

    #endregion
}

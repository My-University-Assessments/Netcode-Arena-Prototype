using System.Collections.Generic;
using System.Threading.Tasks;
using ArenaPrototype.Util;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(NetworkObject))]
public class SceneLoader : NetworkBehaviour
{
    public static SceneLoader Singleton;

    private void Awake()
    {
        #region Singleton
        if (Singleton == null)
        {
            Singleton = this;
            DontDestroyOnLoad(this);

        }
        else
        {
            Destroy(gameObject);

        }
        #endregion

    }

    public void ChangeNetworkScene(string sceneToLoad, string sceneToClose, LoadSceneMode loadSceneMode = LoadSceneMode.Single)
    {
        List<string> scenesToClose = new() { sceneToClose };
        ChangeNetworkScene(sceneToLoad, scenesToClose, loadSceneMode);

    }

    public async void ChangeNetworkScene(string sceneToLoad, List<string> scenesToClose, LoadSceneMode loadSceneMode = LoadSceneMode.Single)
    {


        foreach (string sceneName in scenesToClose)
        {
            if (string.IsNullOrEmpty(sceneName)) continue;
            CloseSceneObserverRPC(sceneName);

        }

        SceneEventProgressStatus sceneLoadStatus = NetworkManager.Singleton.SceneManager.LoadScene(sceneToLoad, LoadSceneMode.Additive);

        if (sceneLoadStatus != SceneEventProgressStatus.Started) { Debug.LogError($"<color={LogColours.Error}>[ERROR]</color> Scene failed to load! {sceneToLoad}!"); return; }
        Debug.Log($"<color={LogColours.Unity}>[NETWORK]</color> Scene transition complete: {sceneToLoad}");

    }

    [Rpc(SendTo.ClientsAndHost)]
    private void CloseSceneObserverRPC(string sceneToClose)
    {
        SceneManager.UnloadSceneAsync(sceneToClose);

    }

}
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoaderNetwork : NetworkBehaviour
{
    public static SceneLoaderNetwork Singleton;

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

    public static void ChangeNetworkScene(string sceneToLoad, string sceneToClose, LoadSceneMode loadSceneMode = LoadSceneMode.Single)
    {
        List<string> scenesToClose = new List<string> { sceneToClose };
        ChangeNetworkScene(sceneToLoad, scenesToClose, loadSceneMode);

    }

    public static async void ChangeNetworkScene(string sceneToLoad, List<string> scenesToClose, LoadSceneMode loadSceneMode = LoadSceneMode.Single)
    {
        foreach (string sceneName in scenesToClose)
        {
            await SceneManager.UnloadSceneAsync(sceneName);
        }

        NetworkManager.Singleton.SceneManager.LoadScene(sceneToLoad, loadSceneMode);

    }

}
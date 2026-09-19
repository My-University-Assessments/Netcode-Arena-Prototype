using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class BootstrapManager : MonoBehaviour
{
    [Header("Scenes")]
    [SerializeField] private string defaultSceneToLoad;
    [SerializeField] private string mainMenuScene;
    [SerializeField] private string gameplayScene;

    private void Start()
    {
        LoadDefaultScene();

    }

    private async void LoadDefaultScene()
    {
        if (string.IsNullOrEmpty(defaultSceneToLoad)) return;
        await SceneManager.LoadSceneAsync(defaultSceneToLoad, LoadSceneMode.Additive);

    }

}

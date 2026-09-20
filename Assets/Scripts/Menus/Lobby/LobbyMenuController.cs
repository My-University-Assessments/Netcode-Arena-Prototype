using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using System.Threading;

[RequireComponent(typeof(NetworkObject))]
public class LobbyMenuController : NetworkBehaviour
{

    [Header("Buttons")]
    [SerializeField] private Button _startGameBtn;
    [SerializeField] private Button _backBtn;

    private void Awake()
    {
        _startGameBtn.interactable = false;

    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (_startGameBtn != null && IsServer)
        {
            _startGameBtn.interactable = true;
            _startGameBtn.onClick.AddListener(StartGame);

        }

    }

    private void StartGame()
    {
        SceneLoader.Singleton.ChangeNetworkSceneAsync("SampleScene", "MainMenuScene", LoadSceneMode.Additive);

    }

}
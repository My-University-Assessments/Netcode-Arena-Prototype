using UnityEngine;
using UnityEngine.UI;

public class HostGame : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button _createLobbyBtn;
    [SerializeField] private Button _backButton;

    private void Start()
    {
        if (_createLobbyBtn != null) _createLobbyBtn.onClick.AddListener(CreateLobby);
        if (_backButton != null) _backButton.onClick.AddListener(GoBack);

    }

    private void CreateLobby()
    {
        NetworkEventManager.OnRequestStartUnityHost?.Invoke(); // TODO: Create lobby system
        Debug.Log($"{UnityNetworkHelper.CheckPrivilege()} Sending lobby create request!");
        LobbyManager.CreateLobbyAsync(2);

    }

    private void GoBack()
    {
        gameObject.SetActive(false);
    }

}
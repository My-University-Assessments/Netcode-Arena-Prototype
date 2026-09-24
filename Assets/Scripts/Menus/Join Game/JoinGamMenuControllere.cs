using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JoinGameMenuController : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button _joinGameBtn;
    [SerializeField] private Button _backButton;

    [Header("Input Field")]
    [SerializeField] private TMP_InputField _inputField;

    private void Start()
    {
        if (_joinGameBtn != null) _joinGameBtn.onClick.AddListener(JoinGame);
        if (_backButton != null) _backButton.onClick.AddListener(GoBack);

    }

    private async void JoinGame()
    {
        if (_inputField == null) { Debug.LogError($"Input field is null!"); return; }
        string joinCode = _inputField.text;

        if (string.IsNullOrEmpty(joinCode)) { Debug.LogWarning($"Join Code is null!"); return; }
        await LobbyManager.JoinLobbyAsync(joinCode);

    }

    private void GoBack()
    {
        gameObject.SetActive(false);
    }
}
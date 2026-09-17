using UnityEngine;
using UnityEngine.UI;

public class JoinGameMenuController : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button _joinGameBtn;
    [SerializeField] private Button _backButton;

    private void Start()
    {
        if (_joinGameBtn != null) _joinGameBtn.onClick.AddListener(JoinGame);
        if (_backButton != null) _backButton.onClick.AddListener(GoBack);

    }

    private void JoinGame()
    {
        Debug.Log($"Join game!");
        NetworkEventManager.OnRequestStartUnityClient?.Invoke();

    }

    private void GoBack()
    {
        gameObject.SetActive(false);
    }
}
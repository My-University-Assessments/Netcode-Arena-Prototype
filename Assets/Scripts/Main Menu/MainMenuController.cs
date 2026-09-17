using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private Button _playSinglePlayerBtn;
    [SerializeField] private Button _playVersusBtn;

    private void Start()
    {
        if (_playSinglePlayerBtn != null) _playSinglePlayerBtn.onClick.AddListener(PlaySinglePlayer);
        if (_playVersusBtn != null) _playVersusBtn.onClick.AddListener(PlayVersus);

    }

    private void PlaySinglePlayer()
    {
        Debug.Log($"Clicked single player!");
    }

    private void PlayVersus()
    {
        Debug.Log($"Clicked versus!");
    }


}

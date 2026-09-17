using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;
using System.Collections.Generic;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private Canvas menuCanvas;

    [Header("Buttons")]
    [SerializeField] private Button _playSinglePlayerBtn;
    [SerializeField] private Button _hostGameBtn;

    [Header("Sub Menus")]
    [SerializeField, DictionaryDisplay(keyLabel = "Sub Menu Type", valueLabel = "Game Object")] private Dictionary<SubMenuType, GameObject> _subMenus;

    private void Start()
    {
        if (_playSinglePlayerBtn != null) _playSinglePlayerBtn.onClick.AddListener(PlaySinglePlayer);
        if (_hostGameBtn != null) _hostGameBtn.onClick.AddListener(HostGame);

        foreach (GameObject gameObject in _subMenus.Values)
        {
            if (gameObject == null) continue;
            gameObject.SetActive(false);

        }

    }

    private void PlaySinglePlayer()
    {
        Debug.Log($"Clicked single player!");
    }

    private void HostGame()
    {
        if (_subMenus == null) { Debug.LogError($"Host game menu is null!"); return; }
        _subMenus[SubMenuType.HostGame].SetActive(true);
        Debug.Log($"Clicked versus!");

    }

    private enum SubMenuType
    {
        HostGame,
        Options,

    }

}

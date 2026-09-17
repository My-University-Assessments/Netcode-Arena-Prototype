using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using UnityEngine.Events;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private Canvas menuCanvas;

    [Header("Buttons")]
    [SerializeField] private Button _playSinglePlayerBtn;

    [Header("Sub Menus")]
    [SerializeField, DictionaryDisplay(keyLabel = "Sub Menu Type", valueLabel = "Sub Menu")] private Dictionary<SubMenuType, SubMenu> _subMenus = new();

    #region Events
    private void OnEnable()
    {
        NetworkEventManager.OnUnityHostStarted += LobbyMenu;
        NetworkEventManager.OnUnityClientStarted += LobbyMenu;
    }

    private void OnDisable()
    {
        NetworkEventManager.OnUnityHostStarted -= LobbyMenu;
        NetworkEventManager.OnUnityClientStarted -= LobbyMenu;

    }
    #endregion

    private void Start()
    {
        if (_playSinglePlayerBtn != null) _playSinglePlayerBtn.onClick.AddListener(PlaySinglePlayer);

        HideAllSubMenus();

        // INFO: Setup Buttons
        SetupMenuButton(SubMenuType.HostGame, HostGame);
        SetupMenuButton(SubMenuType.JoinGame, JoinGame);
        SetupMenuButton(SubMenuType.Options, Options);

    }

    private void PlaySinglePlayer()
    {
        Debug.Log($"Clicked single player!");
    }

    #region Sub Menus
    private void HostGame()
    {
        if (_subMenus.ContainsKey(SubMenuType.HostGame))
            _subMenus[SubMenuType.HostGame].gameObject.SetActive(true);

    }

    private void JoinGame()
    {
        if (_subMenus.ContainsKey(SubMenuType.JoinGame))
            _subMenus[SubMenuType.JoinGame].gameObject.SetActive(true);
    }

    private void LobbyMenu()
    {
        Debug.Log($"Lobby Menu");

        if (_subMenus.ContainsKey(SubMenuType.Lobby))
            _subMenus[SubMenuType.Lobby].gameObject.SetActive(true);

    }

    private void Options()
    {
        Debug.Log($"Clicked options!");
    }
    #endregion

    #region Helper
    private void HideAllSubMenus()
    {
        foreach (SubMenu subMenu in _subMenus.Values)
        {
            if (subMenu.gameObject == null) continue;
            subMenu.gameObject.SetActive(false);

        }

    }


    private void SetupMenuButton(SubMenuType buttonType, UnityAction action)
    {
        // GUARD: Prevent Nulls
        if (!_subMenus.TryGetValue(buttonType, out SubMenu subMenu))
        {
            Debug.LogWarning($"Button not found for {buttonType}");
            return;
        }

        subMenu.button.onClick.AddListener(action);

    }
    #endregion

    #region Utility
    private enum SubMenuType
    {
        HostGame,
        JoinGame,
        Lobby,
        Options,

    }

    [Serializable]
    public class SubMenu
    {
        public GameObject gameObject;
        public Button button;
    }

    #endregion

}



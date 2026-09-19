using System.Collections.Generic;
using ArenaPrototype.Util;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public class PlayerManager : NetworkBehaviour
{
    public static PlayerManager Singleton;

    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private List<Transform> _spawnPositions;

    private Dictionary<ulong, NetworkObject> players = new();

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

    #region Spawn Players
    public async Awaitable<bool> SpawnPlayers()
    {
        if (_playerPrefab == null)
        {
            Debug.LogError($"Player prefab is null!");
            return false;

        }

        Debug.Log($"<color={LogColours.Unity}>[PLAYER MANAGER]</color> <color={LogColours.Host}>[HOST]</color> Spawning {NetworkManager.ConnectedClientsIds.Count} players");

        for (int i = 0; i < NetworkManager.ConnectedClientsIds.Count; i++)
        {
            ulong currentClientId = NetworkManager.ConnectedClientsIds[i];
            GameObject instance = Instantiate(_playerPrefab);

            int nextIndex = (i + 1) % _spawnPositions.Count;
            instance.transform.position = _spawnPositions[nextIndex].position;
            instance.name = $"Client: {currentClientId}";

            NetworkObject netObj = instance.GetComponent<NetworkObject>();
            netObj.SpawnAsPlayerObject(currentClientId, true);
            players.Add(currentClientId, netObj);

        }


        return true;
    }

    #endregion

    #region Handle Player Inputs
    public void HandlePlayerInputs(GameObject gameObject)
    {

    }
    #endregion

}
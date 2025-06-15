using Fusion;
using Fusion.Photon.Realtime;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class LobbyLogic : MonoBehaviour
{
    [SerializeField] private NetworkRunner _networkRunnerPrefab;
    [SerializeField] private LobbyManager _lobbyManagerPrefab; // Assign in inspector
    [SerializeField] private int _maxPlayers = 4;

    [SerializeField] private NetworkRunner _runner;

    public async Task<StartGameResult> CreateRoom(string roomId)
    {
        _runner = Instantiate(_networkRunnerPrefab);

        var result = await _runner.StartGame(new StartGameArgs
        {
            GameMode = GameMode.Host,
            SessionName = roomId,
            PlayerCount = _maxPlayers,
            SceneManager = _runner.GetComponent<INetworkSceneManager>() ??
                      _runner.gameObject.AddComponent<NetworkSceneManagerDefault>()
        });

        if (result.Ok && _runner.IsServer)
        {
            SpawnLobbyManager();
        }

        return result;
    }

    public async Task<StartGameResult> JoinRoom(string roomId)
    {
        _runner = Instantiate(_networkRunnerPrefab);
        return await _runner.StartGame(new StartGameArgs
        {
            GameMode = GameMode.Client,
            SessionName = roomId
        });
    }
    public async Task<StartGameResult> QuickPlay()
    {
        _runner = Instantiate(_networkRunnerPrefab);
        var result = await _runner.StartGame(new StartGameArgs
        {
            GameMode = GameMode.AutoHostOrClient,
            MatchmakingMode = MatchmakingMode.FillRoom
        });

        if (result.Ok && _runner.IsServer)
        {
            SpawnLobbyManager();
        }

        return result;
    }

    // Add this method to properly leave room and shutdown network
    public async Task LeaveRoom()
    {
        if (_runner != null && _runner.IsRunning)
        {
            // Shutdown the network runner
            await _runner.Shutdown();

            // // Destroy the runner GameObject
            // if (_runner.gameObject != null)
            // {
            //     Destroy(_runner.gameObject);
            // }
            // _runner = null;
            MainMenuController.Instance.ShowSection(MenuState.MainMenu);
        }
    }

    private void SpawnLobbyManager()
    {
        if (_runner.IsServer && _lobbyManagerPrefab != null)
        {
            _runner.Spawn(_lobbyManagerPrefab, Vector3.zero, Quaternion.identity);
        }
    }
}
using Fusion;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Fusion.Photon.Realtime;

public class FusionLobbyManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_InputField _roomIdInput;
    [SerializeField] private Button _createRoomBtn;
    [SerializeField] private Button _joinRoomBtn;
    [SerializeField] private Button _quickplayButton;
    [SerializeField] private TMP_Text _statusText;

    [Header("Network")]
    [SerializeField] private NetworkRunner _networkRunnerPrefab;
    [SerializeField] private int _maxPlayers = 4;
    [SerializeField] private string _gameSceneName = "Lobby";

    [Header("Player Settings")]
    [SerializeField] private TMP_InputField _playerNameInput;

    private NetworkRunner _runner;
    private NetworkCallbacks _networkCallbacks;

    private void Awake()
    {
        _createRoomBtn.onClick.AddListener(CreateRoom);
        _joinRoomBtn.onClick.AddListener(() => JoinRoom());
        _quickplayButton.onClick.AddListener(QuickPlay);

        _statusText.text = "";
    }

    private async void CreateRoom()
    {
        string roomId = _roomIdInput.text;
        if (string.IsNullOrEmpty(roomId))
        {
            roomId = $"Room{Random.Range(1000, 9999)}";
            _roomIdInput.text = roomId;
        }

        _statusText.text = "Starting host...";

        _runner = Instantiate(_networkRunnerPrefab);
        _runner.name = "Network Runner [Host]";

        var sceneManager = _runner.GetComponent<INetworkSceneManager>() ?? _runner.gameObject.AddComponent<NetworkSceneManagerDefault>();

        var startGameArgs = new StartGameArgs()
        {
            GameMode = GameMode.Host,
            SessionName = roomId,
            SceneManager = sceneManager,
            PlayerCount = _maxPlayers,
            IsVisible = true,
            IsOpen = true
        };

        var result = await _runner.StartGame(startGameArgs);

        if (result.Ok)
        {
            _statusText.text = $"Host created room: {roomId}";
            // await _runner.LoadScene(_gameSceneName);
        }
        else
        {
            _statusText.text = $"Failed to start: {result.ErrorMessage}";
            Destroy(_runner.gameObject);
        }
    }

    private async void JoinRoom(string roomId = null)
    {
        if (string.IsNullOrEmpty(roomId))
            roomId = _roomIdInput.text;

        if (string.IsNullOrEmpty(roomId))
        {
            _statusText.text = "Please enter a room ID";
            return;
        }

        _statusText.text = "Joining room...";

        _runner = Instantiate(_networkRunnerPrefab);
        _runner.name = "Network Runner [Client]";


        var sceneManager = _runner.GetComponent<INetworkSceneManager>() ?? _runner.gameObject.AddComponent<NetworkSceneManagerDefault>();

        var startGameArgs = new StartGameArgs()
        {
            GameMode = GameMode.Client,
            SessionName = roomId,
            SceneManager = sceneManager
        };

        var result = await _runner.StartGame(startGameArgs);

        if (result.Ok)
        {
            _statusText.text = $"Joined room: {roomId}";
        }
        else
        {
            _statusText.text = $"Failed to join: {result.ErrorMessage}";
            Destroy(_runner.gameObject);
        }
    }

    private async void QuickPlay()
    {
        _statusText.text = "Finding match...";

        _runner = Instantiate(_networkRunnerPrefab);

        // Quickplay with random matchmaking
        var result = await _runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.AutoHostOrClient, // Will join if available, host if not
            MatchmakingMode = MatchmakingMode.FillRoom, // Fill existing rooms first
            PlayerCount = _maxPlayers,
            SceneManager = _runner.GetComponent<INetworkSceneManager>() ??
                         _runner.gameObject.AddComponent<NetworkSceneManagerDefault>()
        });

        if (!result.Ok)
        {
            _statusText.text = $"Matchmaking failed: {result.ErrorMessage}";
            Destroy(_runner.gameObject);
        }
    }


    private void OnDestroy()
    {
        _createRoomBtn.onClick.RemoveListener(CreateRoom);
        _joinRoomBtn.onClick.RemoveListener(() => JoinRoom());

        if (_runner != null && _runner.IsRunning)
            _runner.Shutdown();
    }
}
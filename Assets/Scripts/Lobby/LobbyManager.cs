using Fusion;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyManager : NetworkBehaviour, IPlayerJoined, IPlayerLeft
{
    public static LobbyManager Instance { get; private set; }

    [Networked, Capacity(4), OnChangedRender(nameof(OnPlayersChanged))]
    public NetworkLinkedList<LobbyPlayerData> Players { get; } = default;

    [SerializeField] private NetworkObject _lobbyPlayerPrefab;
    [SerializeField] private LobbyPlayerListDataEvent _onPlayerListChanged;

    [Networked] public string _currentSessionName { get; set; }
    public bool _isInitialized = false;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public override void Spawned()
    {
        OnPlayersChanged();
        _currentSessionName = Runner.SessionInfo.Name;
        _isInitialized = true;
    }

    public void SpawnPlayerData(PlayerRef player)
    {
        var playerObj = Runner.Spawn(_lobbyPlayerPrefab, position: Vector3.zero, inputAuthority: player);
        // playerObj.transform.SetParent(transform, false);
        playerObj.name = "LobbyPlayer_" + player.ToString();

        if (GameConfig.isSharedMode)
        {
            RPC_RequestAddPlayer(playerObj.GetComponent<LobbyPlayerData>());
        }
        else
            Players.Add(playerObj.GetComponent<LobbyPlayerData>());
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_RequestAddPlayer(LobbyPlayerData lobbyPlayerData)
    {
        if (!Players.Contains(lobbyPlayerData))
        {
            Players.Add(lobbyPlayerData);
            Debug.Log($"Player {lobbyPlayerData.PlayerRef} added to lobby. Total players: {Players.Count}");
        }
        else
        {
            Debug.LogWarning($"Player {lobbyPlayerData.PlayerRef} is already in the lobby.");
        }
    }

    public void PlayerJoined(PlayerRef player)
    {
        if (GameConfig.isSharedMode)
        {
            // Debug.Log($"Player {player} joined the lobby. Total players: {Players.Count}");
            // if (player == Runner.LocalPlayer)
            // {
            //     SpawnPlayerData(player);
            //     Debug.Log($"Local player {player} spawned in lobby.");
            // }
        }
        else if (Runner.IsServer)
        {
            SpawnPlayerData(player);
            Debug.Log($"Player {player} joined the lobby. Total players: {Players.Count}");
        }
    }

    public void PlayerLeft(PlayerRef player)
    {
        if (Runner.IsServer || Runner.IsSharedModeMasterClient)
        {
            // LobbyPlayerData playerData = null;
            // foreach (var p in Players)
            // {
            //     if (p == null) continue; // Skip nulls (player already despawned)
            //     if (p.PlayerRef == player)
            //     {
            //         playerData = p;
            //         break;
            //     }
            // }
            // if (playerData != null)
            // {
            //     Players.Remove(playerData);
            //     if (playerData.Object != null)
            //         Runner.Despawn(playerData.Object);
            //     else
            //         Debug.LogWarning("Tried to despawn a null NetworkObject for player: " + player);
            // }
            // OnPlayersChanged();

            // Since when player left, the lobbyPlayerData is removed, so need to update Players, remove the null thing
            for (int i = Players.Count - 1; i >= 0; i--)
            {
                if (Players[i] == null)
                {
                    // Remove by reference since RemoveAt is not supported
                    Players.Remove(Players[i]);
                }
            }
        }
    }

    public void StartGame()
    {
        if ((Runner.IsServer || Runner.IsSharedModeMasterClient) && Players.Count > 0)
        {
            Runner.SessionInfo.IsOpen = false; // Lock the session
            Debug.Log("Starting game with " + Players.Count + " players.");
            SceneRef gameScene = SceneRef.FromIndex(SceneUtility.GetBuildIndexByScenePath($"Assets/Scenes/{GameConfig.GAME_SCENE}.unity"));
            Runner.LoadScene(gameScene, LoadSceneMode.Single);
        }
    }

    public void OnStartGameOrReadyClicked()
    {
        if (Runner.IsServer || Runner.IsSharedModeMasterClient)
        {
            if (!CheckAllPlayersReady())
            {
                Debug.Log("Not all players are ready. Cannot start the game.");
                return;
            }
            StartGame();
        }
        else
        {
            Debug.Log("Local player toggling ready state.");
            TogglePlayerReady();
        }
    }

    #region CheckAllPlayersReady
    public bool CheckAllPlayersReady()
    {
        if (!(Runner.IsServer || Runner.IsSharedModeMasterClient)) return false;

        foreach (var player in FindObjectsOfType<LobbyPlayerData>())
        {
            if (!player.IsReady)
            {
                return false;
            }
        }
        return true;
    }

    public (bool allReady, string statusMessage) GetGameStartStatus()
    {
        bool allReady = true;
        string statusMessage = "";

        foreach (var player in Players)
        {
            if (!player.IsReady) allReady = false;
        }

        if (!allReady)
        {
            statusMessage = "Waiting for all players to be ready...";
        }
        else
        {
            statusMessage = "Ready to start game!";
        }

        return (allReady, statusMessage);
    }
    #endregion

    public void TogglePlayerReady()
    {
        LobbyPlayerData localPlayerData = null;
        foreach (var player in Players)
        {
            if (player.PlayerRef == Runner.LocalPlayer)
            {
                localPlayerData = player;
                break;
            }
        }
        if (localPlayerData != null)
        {
            Debug.Log($"Toggling ready state for player {localPlayerData.PlayerRef}. Current state: {localPlayerData.IsReady}");
            localPlayerData.RPC_SetReady(!localPlayerData.IsReady);
        }
    }

    private void OnPlayersChanged()
    {
        Debug.Log("Player list changed. Total players: " + Players.Count);
        _onPlayerListChanged.Raise(Players);
    }
}
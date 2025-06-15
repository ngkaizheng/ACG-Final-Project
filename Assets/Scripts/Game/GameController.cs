using Fusion;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameController : NetworkBehaviour
{
    public static GameController Instance { get; private set; }

    [Header("Player Setup")]
    [SerializeField] private NetworkObject _playerPrefab;
    [SerializeField] private Transform[] _spawnPoints;
    [SerializeField] private float _respawnDelay = 3f;

    [Header("Game Timer")]
    [SerializeField] private float gameDurationSeconds = 60f; // 1 minute

    [Networked] public TickTimer GameTimer { get; set; }
    [Networked] public NetworkDictionary<PlayerRef, TickTimer> _respawnTimers { get; }
    private Dictionary<PlayerRef, NetworkObject> _spawnedPlayers = new Dictionary<PlayerRef, NetworkObject>();

    public override void Spawned()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (Object.HasStateAuthority)
        {
            GameTimer = TickTimer.CreateFromSeconds(Runner, gameDurationSeconds);
        }
    }

    public override void Render()
    {
        if (!Object.IsValid) return;
        // Only StateAuthority checks and triggers end game
        if (Object.HasStateAuthority && GameTimer.Expired(Runner))
        {
            GameTimer = TickTimer.None; // Prevent multiple triggers
            RPC_ShowEndGame();
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_ShowEndGame()
    {
        EndGameUI.Instance.Show(isHost: Runner.IsServer);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_EndGame()
    {
        StartCoroutine(EndGameRoutine());
    }
    private IEnumerator EndGameRoutine()
    {
        // All clients (including host) disconnect and load main menu
        if (!Runner.IsServer)
        {
            // Client: disconnect and load menu
            Runner.Shutdown();
            SceneManager.LoadScene(GameConfig.MainMenuSceneName);
            yield break;
        }
        else
        {
            // Host: give clients a moment to process the RPC
            yield return new WaitForSeconds(0.5f);
            Runner.Shutdown();
            SceneManager.LoadScene(GameConfig.MainMenuSceneName);
        }
    }


    #region Player Spawn Management
    private void SpawnAllPlayers()
    {
        foreach (var player in Runner.ActivePlayers)
        {
            SpawnPlayer(player);
        }
    }

    public void SpawnPlayer(PlayerRef playerRef)
    {
        if (!Runner.IsServer) return;

        // Get spawn position (round-robin through spawn points)
        var spawnPoint = _spawnPoints[playerRef.PlayerId % _spawnPoints.Length];

        // Spawn with input authority
        var playerObj = Runner.Spawn(
            _playerPrefab,
            spawnPoint.position,
            spawnPoint.rotation,
            playerRef
        );

        //Set PlayerObject
        Runner.SetPlayerObject(playerRef, playerObj);

        _spawnedPlayers.Add(playerRef, playerObj);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_PlayerDied(PlayerRef playerRef)
    {
        if (!_respawnTimers.TryGet(playerRef, out var timer) || timer.ExpiredOrNotRunning(Runner))
        {
            var newTimer = TickTimer.CreateFromSeconds(Runner, _respawnDelay);
            _respawnTimers.Set(playerRef, newTimer);
        }
    }

    public void PlayerDied(PlayerRef playerRef)
    {
        if (!_respawnTimers.TryGet(playerRef, out var timer) || timer.ExpiredOrNotRunning(Runner))
        {
            var newTimer = TickTimer.CreateFromSeconds(Runner, _respawnDelay);
            _respawnTimers.Set(playerRef, newTimer);
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_RequestRespawn(PlayerRef playerRef)
    {
        if (!_respawnTimers.TryGet(playerRef, out var timer) || timer.Expired(Runner))
        {
            if (_spawnedPlayers.ContainsKey(playerRef))
            {
                Runner.Despawn(_spawnedPlayers[playerRef]);
                _spawnedPlayers.Remove(playerRef);
            }
            SpawnPlayer(playerRef);
            _respawnTimers.Remove(playerRef);
        }
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        _spawnedPlayers.Clear();
    }

    public void OnPlayerDied(PlayerRef playerRef)
    {
        if (_spawnedPlayers.TryGetValue(playerRef, out NetworkObject playerObj))
        {
            StartCoroutine(DespawnPlayerAfterDelay(playerRef, playerObj, 3f));
        }
    }

    private IEnumerator DespawnPlayerAfterDelay(PlayerRef playerRef, NetworkObject playerObj, float delay)
    {
        yield return new WaitForSeconds(delay);

        Runner.Despawn(playerObj);
        _spawnedPlayers.Remove(playerRef);
    }

    public bool CanLocalPlayerRespawn()
    {
        var playerRef = Runner.LocalPlayer;
        if (_respawnTimers.TryGet(playerRef, out var timer))
        {
            return timer.Expired(Runner);
        }
        // If no timer exists, allow respawn (first spawn)
        return true;
    }

    public TickTimer GetRespawnTimer(PlayerRef playerRef)
    {
        if (_respawnTimers.TryGet(playerRef, out var timer))
        {
            return timer;
        }
        return TickTimer.None;
    }
    #endregion
}
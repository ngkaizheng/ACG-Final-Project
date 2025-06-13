using Fusion;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

public class GameController : NetworkBehaviour
{
    public static GameController Instance { get; private set; }

    [Header("Player Setup")]
    [SerializeField] private NetworkObject _playerPrefab;
    [SerializeField] private Transform[] _spawnPoints;
    [SerializeField] private float _respawnDelay = 3f;

    [Networked] private TickTimer _respawnTimer { get; set; }
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

        if (Runner.IsServer)
        {
            SpawnAllPlayers();
        }
    }

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

        _spawnedPlayers[playerRef] = playerObj;
    }

    // Called when player dies
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_RequestRespawn(PlayerRef playerRef)
    {
        if (_respawnTimer.ExpiredOrNotRunning(Runner))
        {
            _respawnTimer = TickTimer.CreateFromSeconds(Runner, _respawnDelay);
            StartCoroutine(RespawnPlayerAfterDelay(playerRef));
        }
    }

    private IEnumerator RespawnPlayerAfterDelay(PlayerRef playerRef)
    {
        while (!_respawnTimer.Expired(Runner))
            yield return null;

        if (_spawnedPlayers.ContainsKey(playerRef))
        {
            Runner.Despawn(_spawnedPlayers[playerRef]);
            _spawnedPlayers.Remove(playerRef);
        }
        SpawnPlayer(playerRef);
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        _spawnedPlayers.Clear();
    }
}
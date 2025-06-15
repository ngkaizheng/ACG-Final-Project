using UnityEngine;
using Fusion;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Rendering;

public class InGamePlayerManager : NetworkBehaviour
{
    public static InGamePlayerManager Instance { get; private set; }

    [Header("Config")]
    [SerializeField] private NetworkObject _playerDataPrefab;

    [Header("Event")]
    [SerializeField] private PlayerKillEvent playerKilledEvent;


    [Networked, Capacity(8)]
    public NetworkDictionary<PlayerRef, InGamePlayerData> playerDataDict { get; }

    private void Awake()
    {
        playerKilledEvent.OnRaised.AddListener(OnPlayerKilled);
    }

    private void OnDestroy()
    {
        playerKilledEvent.OnRaised.RemoveListener(OnPlayerKilled);
    }

    public override void Spawned()
    {
        if (Instance != null && Instance != this)
        {
            Runner.Despawn(Object);
            return;
        }
        Instance = this;

        // Server-only initialization
        if (Runner.IsServer)
        {
            InitializeAllPlayerData();
        }
        LeaderboardUI.Instance.UpdateLeaderboard();
    }
    private void InitializeAllPlayerData()
    {
        foreach (var player in Runner.ActivePlayers)
        {
            SpawnPlayerData(player);
        }
    }
    private void OnPlayerKilled(PlayerKillInfo info)
    {
        Debug.Log($"Player {info.Killer} killed {info.Victim}");
        // Find killer and victim InGamePlayerData
        var killerData = GetPlayerData(info.Killer);
        var victimData = GetPlayerData(info.Victim);

        if (killerData != null)
            killerData.Kills++;

        if (victimData != null)
            victimData.Deaths++;

        LeaderboardUI.Instance.UpdateLeaderboard();
    }

    private void SpawnPlayerData(PlayerRef playerRef)
    {
        Runner.Spawn(
            _playerDataPrefab,
            inputAuthority: playerRef,
            onBeforeSpawned: (runner, obj) =>
            {
                var data = obj.GetComponent<InGamePlayerData>();
                playerDataDict.Set(playerRef, data);
            }
        );
    }

    public InGamePlayerData GetPlayerData(PlayerRef playerRef)
    => playerDataDict.TryGet(playerRef, out var data) ? data : null;

#if UNITY_EDITOR
    [Header("Debug")]
    [SerializeField]
    private List<InGamePlayerData> _debugPlayerDataList = new List<InGamePlayerData>();

    private void Update()
    {
        if (Object == null || !Object.IsValid) return;
        // Only update in Editor and in Play mode
        if (!Application.isPlaying) return;
        _debugPlayerDataList = playerDataDict.Select(kv => kv.Value).ToList();
    }
#endif
}
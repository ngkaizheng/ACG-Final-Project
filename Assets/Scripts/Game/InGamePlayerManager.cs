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

    [Networked, Capacity(8)]
    private NetworkDictionary<PlayerRef, InGamePlayerData> _playerDataDict { get; }

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
    }
    private void InitializeAllPlayerData()
    {
        foreach (var player in Runner.ActivePlayers)
        {
            SpawnPlayerData(player);
        }
    }
    private void SpawnPlayerData(PlayerRef playerRef)
    {
        Runner.Spawn(
            _playerDataPrefab,
            inputAuthority: playerRef,
            onBeforeSpawned: (runner, obj) =>
            {
                var data = obj.GetComponent<InGamePlayerData>();
                _playerDataDict.Set(playerRef, data);
            }
        );
    }
    // public List<LobbyPlayerData> GetAllPlayers()
    // {
    //     return FindObjectsOfType<LobbyPlayerData>().ToList();
    // }
    public InGamePlayerData GetPlayerData(PlayerRef playerRef)
    => _playerDataDict.TryGet(playerRef, out var data) ? data : null;

#if UNITY_EDITOR
    [Header("Debug")]
    [SerializeField]
    private List<InGamePlayerData> _debugPlayerDataList = new List<InGamePlayerData>();

    private void Update()
    {
        if (Object == null || !Object.IsValid) return;
        // Only update in Editor and in Play mode
        if (!Application.isPlaying) return;
        _debugPlayerDataList = _playerDataDict.Select(kv => kv.Value).ToList();
    }
#endif
}
using System;
using Fusion;
using UnityEngine;

public class LobbyPlayerData : NetworkBehaviour
{
    [Networked] public PlayerRef PlayerRef { get; set; }
    [Networked, OnChangedRender(nameof(OnDataChanged))] public NetworkString<_32> Nickname { get; set; }
    [Networked, OnChangedRender(nameof(OnDataChanged))] public bool IsReady { get; set; }

    [Header("Event Listening")]
    [SerializeField] private LobbyPlayerDataEvent _onPlayerDataUpdated;
    public bool isInitialized = false;

    public override void Spawned()
    {
        PlayerRef = Object.InputAuthority;

        if (Runner.IsServer || Runner.IsSharedModeMasterClient)
        {
            // Initialize with default values
            Nickname = "...";
            if ((Runner.IsServer || Runner.IsSharedModeMasterClient) && Runner.LocalPlayer == PlayerRef)
                IsReady = true;
            else
                IsReady = false;
        }

        isInitialized = true;

        if (Object.HasInputAuthority)
        {
            RPC_SetNickname(PlayerNickname.Instance.GetNickname());
        }

        DontDestroyOnLoad(gameObject);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    private void RPC_SetNickname(NetworkString<_32> nickname)
    {
        Nickname = nickname;
        Debug.Log($"Player {PlayerRef} set nickname to: {Nickname}");
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SetReady(bool isReady)
    {
        IsReady = isReady;
    }

    // public void RPC_KickPlayer()
    // {
    //     if (Runner.IsServer || Runner.IsSharedModeMasterClient)
    //     {
    //         // Runner.Disconnect(PlayerRef); // Force disconnect
    //         // Runner.Despawn(Object); // Remove player object
    //     }
    // }
    [Rpc(RpcSources.All, RpcTargets.InputAuthority)]
    public void RPC_KickPlayer()
    {
        // This runs on the kicked player's client
        Debug.Log("You have been kicked from the room.");
        Runner.Shutdown();
    }

    private void OnDataChanged()
    {
        if (!isInitialized || string.IsNullOrEmpty(Nickname.ToString()))
            return;
        _onPlayerDataUpdated?.Raise(this);
    }
}
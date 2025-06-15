using System.Linq;
using Fusion;
using UnityEngine;

public class InGamePlayerData : NetworkBehaviour
{
    [Networked] public PlayerRef PlayerRef { get; set; }
    [Networked] public NetworkString<_32> Nickname { get; set; } // Copy from lobby
    [Networked, OnChangedRender(nameof(OnKillDeathsChanged))] public int Kills { get; set; }
    [Networked, OnChangedRender(nameof(OnKillDeathsChanged))] public int Deaths { get; set; }

    #region Initialization
    public override void Spawned()
    {
        PlayerRef = Object.InputAuthority;

        if (Runner.IsServer || GameConfig.isSharedMode)
        {
            if (string.IsNullOrEmpty(Nickname.Value))
            {
                CopyNicknameFromLobby();
            }
            Kills = 0;
            Deaths = 0;
        }
    }

    private void CopyNicknameFromLobby()
    {
        var lobbyData = FindLobbyData();
        if (lobbyData != null)
        {
            Nickname = lobbyData.Nickname;
        }
    }

    private LobbyPlayerData FindLobbyData()
    {
        return FindObjectsOfType<LobbyPlayerData>()
              .FirstOrDefault(p => p.PlayerRef == PlayerRef);
    }
    #endregion

    public string GetNickname()
    {
        return Nickname.Value;
    }

    private void OnKillDeathsChanged()
    {
        // Optionally, you can update the UI or notify other players
        // For example, you could call a method to update the scoreboard
        // UpdateScoreboard();
        LeaderboardUI.Instance.UpdateLeaderboard();
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_AddKill()
    {
        Kills++;
        // Optionally, you can also update the UI or notify other players
        // For example, you could call a method to update the scoreboard
        // UpdateScoreboard();
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_AddDeath()
    {
        Deaths++;
        // Optionally, you can also update the UI or notify other players
        // For example, you could call a method to update the scoreboard
        // UpdateScoreboard();
    }
}
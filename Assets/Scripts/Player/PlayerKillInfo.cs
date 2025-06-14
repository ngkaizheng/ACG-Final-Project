using Fusion;

[System.Serializable]
public struct PlayerKillInfo
{
    public PlayerRef Killer;
    public PlayerRef Victim;

    public PlayerKillInfo(PlayerRef killer, PlayerRef victim)
    {
        Killer = killer;
        Victim = victim;
    }
}
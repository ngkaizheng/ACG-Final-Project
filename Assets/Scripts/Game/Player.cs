using Fusion;
using UnityEngine;

public class Player : NetworkBehaviour
{
    [Networked] public PlayerRef PlayerRef { get; set; }
    [Networked] public NetworkBool IsAlive { get; set; }

    public void Initialize(string nickname)
    {
        PlayerRef = Object.InputAuthority;
        IsAlive = true;
        // Setup visuals, etc.
    }

    public void Die()
    {
        if (IsAlive)
        {
            IsAlive = false;
            RPC_ReportDeath();
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_ReportDeath()
    {
        GameController.Instance?.RPC_RequestRespawn(Object.InputAuthority);
    }

    public void Respawn()
    {
        IsAlive = true;
        // Reset health, position, etc.
    }
}
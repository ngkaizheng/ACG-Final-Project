using Fusion;
using UnityEngine;

public abstract class NetworkHealth : NetworkBehaviour, IDamageable
{
    [Networked] public int CurrentHealth { get; protected set; }
    [Networked] public int MaxHealth { get; protected set; } = 100;

    [Networked, OnChangedRender(nameof(OnAliveStateChanged))] public NetworkBool IsAlive { get; protected set; }

    public override void Spawned()
    {
        CurrentHealth = MaxHealth;
        IsAlive = true;
    }

    public virtual void TakeDamage(int damage, PlayerRef attacker)
    {
        if (!IsAlive) return;

        CurrentHealth -= damage;

        if (CurrentHealth <= 0)
        {
            IsAlive = false;
            OnDeath(attacker);
        }
    }

    public virtual void Heal(int amount)
    {
        CurrentHealth = Mathf.Min(CurrentHealth + amount, MaxHealth);
    }

    protected abstract void OnDeath(PlayerRef killer);

    public abstract void OnAliveStateChanged();
}
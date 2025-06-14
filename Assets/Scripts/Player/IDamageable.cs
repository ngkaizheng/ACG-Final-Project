using Fusion;

public interface IDamageable
{
    void TakeDamage(int damage, PlayerRef attacker);
    void Heal(int amount);
    NetworkBool IsAlive { get; }
}
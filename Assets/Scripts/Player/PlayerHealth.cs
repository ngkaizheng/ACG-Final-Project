using System.Collections;
using Fusion;
using UnityEngine;

public class PlayerHealth : NetworkHealth
{
    [Header("Player Specific")]
    [SerializeField] private float _respawnDelay = 3f;
    [SerializeField] private Transform[] _spawnPoints;

    [Networked, OnChangedRender(nameof(OnDeathAnimationChanged))]
    private float _deathDirection { get; set; } = -1f; // -1 means not dead

    [Header("Events")]
    [SerializeField] private PlayerKillEvent _playerKilledEvent;
    [SerializeField] private PlayerEvent _playerRespawnedEvent;

    [Networked, OnChangedRender(nameof(OnKillInfoChanged))]
    private PlayerKillInfo LastKillInfo { get; set; }

    protected override void OnDeath(PlayerRef killer)
    {
        // 1. Handle kill notification
        // _playerKilledEvent.Raise(new PlayerKillInfo(killer, Object.InputAuthority));
        // Debug.Log($"Player {Object.InputAuthority} killed by {killer}");
        LastKillInfo = new PlayerKillInfo(killer, Object.InputAuthority);

        if (Object.HasStateAuthority)
        {
            GameController.Instance.PlayerDied(Object.InputAuthority);
        }

        // 2. Calculate death direction (0=Back, 1=Front, 2=Left, 3=Right)
        Vector3 damageDir = CalculateDamageDirection(killer);
        _deathDirection = GetDirectionIndex(damageDir);

        // // 3. Show death UI locally (after 1sec delay)
        // if (Object.HasInputAuthority) // Only for local player
        // {
        //     Debug.Log()
        //     StartCoroutine(ShowDeathUIAfterDelay(1f));
        // }
    }

    public override void OnAliveStateChanged()
    {
        if (!IsAlive && Object.HasInputAuthority)
        {
            StartCoroutine(ShowDeathUIAfterDelay(1f));
        }
    }

    private void OnKillInfoChanged()
    {
        _playerKilledEvent.Raise(LastKillInfo);
    }


    private Vector3 CalculateDamageDirection(PlayerRef attacker)
    {
        if (Runner.TryGetPlayerObject(attacker, out var attackerObj))
        {
            return (transform.position - attackerObj.transform.position).normalized;
        }
        return transform.forward; // Default to forward if attacker not found
    }

    private IEnumerator ShowDeathUIAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SpawnUI.Instance.ShowSpawnPanel();
    }

    private float GetDirectionIndex(Vector3 damageDirection)
    {
        // Convert damage direction to local space
        Vector3 localDir = transform.InverseTransformDirection(damageDirection);
        localDir.y = 0; // Ignore vertical difference

        // Calculate angle and convert to blend tree index
        float angle = Mathf.Atan2(localDir.x, localDir.z) * Mathf.Rad2Deg;

        if (angle < -135f || angle > 135f) return 0f; // Back
        if (angle > 45f) return 2f;                   // Left
        if (angle < -45f) return 3f;                  // Right
        return 1f;                                    // Front
    }
    // private float GetDirectionIndex(Vector3 damageDirection)
    // {
    //     // Convert damage direction to local space
    //     Vector3 localDir = transform.InverseTransformDirection(damageDirection);
    //     localDir.y = 0; // Ignore vertical difference

    //     // Normalize the vector (important for accurate angle calculation)
    //     if (localDir.sqrMagnitude > 0.001f)
    //         localDir.Normalize();
    //     else
    //         return 1f; // Default to front if direction is zero

    //     // Calculate angle (-180 to 180 degrees)
    //     float angle = Mathf.Atan2(localDir.x, localDir.z) * Mathf.Rad2Deg;

    //     // Convert angle to a continuous blend value (0-4 range, wrapping around)
    //     float blendValue = (angle + 180f) / 90f; // Maps -180°→0, -90°→1, 0°→2, 90°→3, 180°→4

    //     // Wrap the value to 0-4 range (4 becomes 0 for smooth back-to-back transition)
    //     blendValue = blendValue % 4f;

    //     return blendValue;
    // }


    private void OnDeathAnimationChanged()
    {
        if (_deathDirection >= 0 && TryGetComponent<Animator>(out var animator))
        {
            // Set blend tree parameter and trigger death
            animator.SetFloat("DeathDirection", _deathDirection);
            animator.SetTrigger("Dead");

            // Don't reset _deathDirection here - keep it for respawn checks
        }
    }

    // Player-specific additions
    public void SetMaxHealth(int newMaxHealth)
    {
        MaxHealth = newMaxHealth;
        CurrentHealth = Mathf.Min(CurrentHealth, MaxHealth);
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || _deathDirection < 0) return;

        Color[] dirColors = { Color.red, Color.green, Color.blue, Color.yellow };
        int dirIndex = Mathf.RoundToInt(_deathDirection);
        if (dirIndex >= 0 && dirIndex < dirColors.Length)
        {
            Gizmos.color = dirColors[dirIndex];
            Gizmos.DrawRay(transform.position, transform.forward * 2f);
        }
    }
}
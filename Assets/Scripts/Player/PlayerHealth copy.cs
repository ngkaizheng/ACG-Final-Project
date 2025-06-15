// using Fusion;
// using UnityEngine;

// public class PlayerHealth : NetworkHealth
// {
//     [Header("Player Specific")]
//     [SerializeField] private float _respawnDelay = 3f;
//     [SerializeField] private Transform[] _spawnPoints;

//     [Networked, OnChangedRender(nameof(OnPlayDeathAnimChanged))]
//     private NetworkBool _playDeathAnim { get; set; }

//     [Header("Events")]
//     [SerializeField] private PlayerKillEvent _playerKilledEvent;
//     [SerializeField] private PlayerEvent _playerRespawnedEvent;

//     protected override void OnDeath(PlayerRef killer)
//     {
//         // 1. Handle kill notification
//         _playerKilledEvent.Raise(new PlayerKillInfo(killer, Object.InputAuthority));
//         Debug.Log($"Player {Object.InputAuthority} killed by {killer}");

//         _playDeathAnim = true;

//         // // 2. Disable player temporarily
//         // GetComponent<PlayerMovement>().SetMovementEnabled(false);
//         // GetComponent<PlayerVisuals>().SetDeathState(true);

//         // // 3. Start respawn process
//         // StartCoroutine(RespawnAfterDelay(_respawnDelay));
//     }

//     private void OnPlayDeathAnimChanged()
//     {
//         if (_playDeathAnim)
//         {
//             // Play death animation on all clients
//             GetComponent<Animator>().SetTrigger("Dead");
//             _playDeathAnim = false; // Reset after playing
//         }
//     }

//     // private IEnumerator RespawnAfterDelay(float delay)
//     // {
//     //     yield return new WaitForSeconds(delay);

//     //     // Reset health
//     //     CurrentHealth = MaxHealth;
//     //     IsAlive = true;

//     //     // Move to spawn point
//     //     transform.position = _spawnPoints[Random.Range(0, _spawnPoints.Length)].position;

//     //     // Re-enable player
//     //     GetComponent<PlayerMovement>().SetMovementEnabled(true);
//     //     GetComponent<PlayerVisuals>().SetDeathState(false);

//     //     // Notify listeners
//     //     _playerRespawnedEvent.Raise(Object.InputAuthority);
//     // }

//     // Player-specific additions
//     public void SetMaxHealth(int newMaxHealth)
//     {
//         MaxHealth = newMaxHealth;
//         CurrentHealth = Mathf.Min(CurrentHealth, MaxHealth);
//     }
// }
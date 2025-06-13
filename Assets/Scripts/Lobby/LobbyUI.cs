using System.Collections.Generic;
using System.Linq;
using Fusion;
using UnityEngine;

public class LobbyUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform _playerListContainer;
    [SerializeField] private GameObject _playerListItemPrefab;

    [Header("Event Listening")]
    [SerializeField] private LobbyPlayerListDataEvent _playerListChangedEvent;
    [SerializeField] private LobbyPlayerDataEvent _playerDataUpdatedEvent;

    private NetworkRunner Runner => LobbyManager.Instance.Runner;
    private readonly Dictionary<PlayerRef, PlayerListItem> _playerListItems = new();

    private void OnEnable()
    {
        _playerListChangedEvent.OnRaised.AddListener(UpdatePlayerList);
        _playerDataUpdatedEvent.OnRaised.AddListener(UpdatePlayerListItem);
    }

    private void OnDisable()
    {
        _playerListChangedEvent.OnRaised.RemoveListener(UpdatePlayerList);
        _playerDataUpdatedEvent.OnRaised.RemoveListener(UpdatePlayerListItem);
    }

    private void UpdatePlayerList(NetworkLinkedList<LobbyPlayerData> players)
    {
        foreach (var kvp in _playerListItems.ToList()) // ToList() to avoid modification during iteration
        {
            if (!players.Any(p => p.PlayerRef == kvp.Key)) // <- O(n) lookup
            {
                Destroy(kvp.Value.gameObject);
                _playerListItems.Remove(kvp.Key);
            }
        }

        // Ensure the UI order matches the player list order and labels are sequential
        int displayIndex = 1;
        foreach (var player in players)
        {
            PlayerListItem listItem;
            if (!_playerListItems.TryGetValue(player.PlayerRef, out listItem))
            {
                var item = Instantiate(_playerListItemPrefab, _playerListContainer);
                listItem = item.GetComponent<PlayerListItem>();
                _playerListItems[player.PlayerRef] = listItem;
            }

            // Set the correct sibling index for UI order
            listItem.transform.SetSiblingIndex(displayIndex);

            bool isLocalPlayer = player.PlayerRef == Runner.LocalPlayer;
            bool isHost = Runner != null && Runner.IsServer;

            // Always update the display ID and name to keep them in sync
            listItem.Initialize(
                $"P{displayIndex}",
                player,
                isLocalPlayer,
                isHost
            );
            displayIndex++;
        }
    }
    private void UpdatePlayerListItem(LobbyPlayerData playerData)
    {
        if (_playerListItems.TryGetValue(playerData.PlayerRef, out var listItem))
        {
            bool isLocalPlayer = Runner != null && playerData.PlayerRef == Runner.LocalPlayer;
            listItem.UpdatePlayerItem(playerData.Nickname.ToString(), playerData.IsReady, isLocalPlayer);
        }
    }
}
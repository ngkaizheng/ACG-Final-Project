using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerListItem : MonoBehaviour
{
    [Header("Player Info")]
    [SerializeField] private TMP_Text playerIdText;
    [SerializeField] private TMP_Text playerNameText;

    [Header("Ready")]
    [Tooltip("Icon to indicate if the player is ready.")]
    [SerializeField] private Button readyButton;
    [SerializeField] private Sprite readyIcon;

    [Header("Kick")]
    [Tooltip("Button to kick the player from the lobby. Only visible for host.")]
    [SerializeField] private Button kickButton;
    [SerializeField] private Sprite kickIcon;

    private LobbyPlayerData _playerData;

    public void Initialize(string playerId, LobbyPlayerData playerData, bool isLocalPlayer, bool isHost)
    {
        _playerData = playerData;

        string displayId = playerId;
        var match = _playerIdRegex.Match(playerId);
        if (match.Success)
            displayId = $"P{match.Groups[1].Value}";

        playerIdText.text = displayId;
        playerNameText.text = isLocalPlayer ? $"{playerData.Nickname} (You)" : playerData.Nickname.ToString();

        if (readyButton != null)
        {
            if (isLocalPlayer)
            {
                readyButton.onClick.RemoveAllListeners();
                readyButton.onClick.AddListener(() => OnReadyButtonClicked());
            }
            readyButton.image.sprite = readyIcon;
            readyButton.image.color = playerData.IsReady ? UISettings.EnabledColor : UISettings.DisabledColor;

        }

        if (kickButton != null)
        {
            if (isHost && isLocalPlayer)
            {
                kickButton.gameObject.SetActive(false);
                return;
            }
            kickButton.gameObject.SetActive(isHost);
            kickButton.image.sprite = kickIcon;
            kickButton.onClick.RemoveAllListeners();
            kickButton.onClick.AddListener(() => OnKickButtonClicked());
        }
    }

    private void OnDestroy()
    {
        if (kickButton != null)
        {
            kickButton.onClick.RemoveAllListeners();
        }
    }

    public void UpdatePlayerItem(string newName, bool isReady, bool isLocalPlayer)
    {
        playerNameText.text = isLocalPlayer ? $"{newName} (YOU)" : newName;
        if (readyButton != null)
        {
            readyButton.image.color = isReady ? UISettings.EnabledColor : UISettings.DisabledColor;
        }
    }

    private void OnReadyButtonClicked()
    {
        _playerData.RPC_SetReady(!_playerData.IsReady);
    }

    private void OnKickButtonClicked()
    {
        _playerData.RPC_KickPlayer();
    }

    private static readonly System.Text.RegularExpressions.Regex _playerIdRegex =
    new System.Text.RegularExpressions.Regex(@"\[(?:Player|player):(\d+)\]");
}


using UnityEngine;
using UnityEngine.UI;
using TMPro;

//This class UI is handle for spawn the player.

public class SpawnUI : MonoBehaviour
{
    public static SpawnUI Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject _spawnPanel;
    [SerializeField] private Button _spawnButton;
    [SerializeField] private TMP_Text _spawnTimerText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        _spawnButton.onClick.AddListener(() => OnSpawnButtonClicked());

        _spawnPanel.SetActive(true);
    }

    private void Update()
    {
        if (_spawnPanel.activeSelf && GameController.Instance != null && GameController.Instance.Runner != null)
        {
            bool canRespawn = GameController.Instance.CanLocalPlayerRespawn();
            _spawnButton.interactable = canRespawn;

            if (!canRespawn)
            {
                var respawnTimer = GameController.Instance.GetRespawnTimer(GameController.Instance.Runner.LocalPlayer);
                if (respawnTimer.IsRunning)
                {
                    _spawnTimerText.text = $"Able To Respawn in: {respawnTimer.RemainingTime(GameController.Instance.Runner):F1}s";
                }
                else
                {
                    _spawnTimerText.text = "Cick To Respawn";
                }
            }
            else
            {
                _spawnTimerText.text = "Cick To Respawn";
            }
        }
    }

    public void ShowSpawnPanel()
    {
        _spawnPanel.SetActive(true);
    }

    public void HideSpawnPanel()
    {
        _spawnPanel.SetActive(false);
    }

    public void OnSpawnButtonClicked()
    {
        if (GameConfig.isSharedMode)
        {
            GameController.Instance.RPC_RequestRespawnShared(GameController.Instance.Runner.LocalPlayer);
            GameController.Instance.SpawnPlayer(GameController.Instance.Runner.LocalPlayer);
        }
        else
        {
            GameController.Instance.RPC_RequestRespawn(GameController.Instance.Runner.LocalPlayer);
        }
        HideSpawnPanel();
    }
}
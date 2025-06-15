using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Fusion;

public class EndGameUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private UnityEngine.UI.Button backToMenuButton;

    private bool isHost = false;

    public static EndGameUI Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (backToMenuButton != null)
            backToMenuButton.onClick.AddListener(OnBackToMenuClicked);

        Hide();
    }

    public void Show(string message = "Game Over!", bool isHost = false)
    {
        this.isHost = isHost;
        if (panel != null) panel.SetActive(true);
        if (messageText != null) messageText.text = message;
    }

    public void Hide()
    {
        if (panel != null) panel.SetActive(false);
    }

    private async void OnBackToMenuClicked()
    {
        var runner = FindObjectOfType<NetworkRunner>();
        if (isHost)
        {
            GameController.Instance.RPC_EndGame();
        }
        else
        {
            // Optionally disconnect from Fusion session
            if (runner != null)
                await runner.Shutdown();
            SceneManager.LoadScene(GameConfig.MainMenuSceneName);
        }

    }
}
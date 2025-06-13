using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class MainMenuController : MonoBehaviour
{

    [Header("Main Menu Section")]
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button exitButton;

    [Header("Multiplayer Section")]
    [SerializeField] private Button createRoomButton;
    [SerializeField] private Button joinRoomButton;
    [SerializeField] private Button quickPlayButton;
    [SerializeField] private Button multiplayerBackButton;
    [SerializeField] private TMP_InputField roomIdInputField;
    [SerializeField] private TMP_Text roomInputFieldStatusText;
    [SerializeField] private TMP_Text multiplayerMainStatusText;

    [Header("Lobby Section")]
    [SerializeField] private Button lobbyBackButton;
    [SerializeField] private Button startGameInLobbyButton;

    [Header("Settings Section")]
    [SerializeField] private Button settingsBackButton;

    [Header("Layout Settings")]
    [SerializeField] private GameObject mainMenuLayout;
    [SerializeField] private GameObject settingsLayout;
    [SerializeField] private GameObject multiplayerLayout;
    [SerializeField] private GameObject lobbyLayout;

    private LobbyLogic lobbyLogic;
    public static MainMenuController Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        lobbyLogic = FindObjectOfType<LobbyLogic>();

        // Main menu buttons
        startGameButton.onClick.AddListener(() => ShowSection(MenuState.Multiplayer));
        settingsButton.onClick.AddListener(() => ShowSection(MenuState.Settings));
        exitButton.onClick.AddListener(ExitGame);

        // Multiplayer buttons
        createRoomButton.onClick.AddListener(CreateRoom);
        joinRoomButton.onClick.AddListener(JoinRoom);
        quickPlayButton.onClick.AddListener(QuickPlay);
        multiplayerBackButton.onClick.AddListener(() => ShowSection(MenuState.MainMenu));

        // Lobby buttons
        lobbyBackButton.onClick.AddListener(ExitNetworkAndShowMainMenu);
        startGameInLobbyButton.onClick.AddListener(() => StartGameOrReadyInLobby());

        // Settings buttons
        settingsBackButton.onClick.AddListener(() => ShowSection(MenuState.MainMenu));
    }

    private void Start()
    {
        ShowSection(MenuState.MainMenu);
    }

    #region Navigation Methods

    public void ShowSection(MenuState state)
    {
        mainMenuLayout.SetActive(state == MenuState.MainMenu);
        multiplayerLayout.SetActive(state == MenuState.Multiplayer);
        lobbyLayout.SetActive(state == MenuState.Lobby);
        settingsLayout.SetActive(state == MenuState.Settings);
    }

    private async void ExitNetworkAndShowMainMenu()
    {
        await lobbyLogic.LeaveRoom();
        ShowSection(MenuState.MainMenu);
    }

    private void ExitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
    #endregion

    #region Multiplayer Methods
    private async void CreateRoom()
    {
        //Random a room ID
        string roomId = $"Room{Random.Range(1000, 9999)}";
        var result = await lobbyLogic.CreateRoom(roomId);
        if (result.Ok)
        {
            ShowSection(MenuState.Lobby);
        }
        else
        {
            multiplayerMainStatusText.text = $"Failed to create room: {result.ErrorMessage}";
        }
    }

    private async void JoinRoom()
    {
        string roomId = roomIdInputField.text.Trim();
        if (string.IsNullOrEmpty(roomId))
        {
            roomInputFieldStatusText.text = "Please enter a valid room ID.";
            return;
        }

        var result = await lobbyLogic.JoinRoom(roomId);
        if (result.Ok)
        {
            ShowSection(MenuState.Lobby);
        }
        else
        {
            multiplayerMainStatusText.text = $"Failed to join room: {result.ErrorMessage}";
        }
    }

    private async void QuickPlay()
    {
        var result = await lobbyLogic.QuickPlay();
        if (result.Ok)
        {
            ShowSection(MenuState.Lobby);
        }
        else
        {
            multiplayerMainStatusText.text = $"Quick play failed: {result.ErrorMessage}";
        }
    }

    public async void HandleLeftRoom(bool isKicked = false)
    {
        await lobbyLogic.LeaveRoom();
        ShowSection(MenuState.MainMenu);
        // Optional: Show kick notification
    }

    private void StartGameOrReadyInLobby()
    {
        if (LobbyManager.Instance != null)
        {
            LobbyManager.Instance.OnStartGameOrReadyClicked();
        }
    }
    #endregion
}

public enum MenuState
{
    MainMenu,
    Multiplayer,
    Lobby,
    Settings
}
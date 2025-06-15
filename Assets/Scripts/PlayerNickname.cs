using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Fusion;

public class PlayerNickname : MonoBehaviour
{
    public static PlayerNickname Instance { get; private set; }

    [Header("UI References")]

    [SerializeField] private TMP_InputField _nicknameInput;
    [SerializeField] private Button _confirmButton;

    private string _playerNickname = "Player";

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

        // Load saved nickname if exists
        if (PlayerPrefs.HasKey("Nickname"))
        {
            _playerNickname = PlayerPrefs.GetString("Nickname");
            _nicknameInput.text = _playerNickname;
        }

        _confirmButton.onClick.AddListener(SaveNickname);

        _nicknameInput.onValueChanged.AddListener((value) =>
        {
            UpdateNickname(value);
        });

    }

    private void UpdateNickname(string value)
    {
        _playerNickname = string.IsNullOrWhiteSpace(value) ? "Player" : value.Trim();
        PlayerPrefs.SetString("Nickname", _playerNickname);
        PlayerPrefs.Save();
        Debug.Log("Nickname updated: " + _playerNickname);
    }

    public void SaveNickname()
    {
        _playerNickname = string.IsNullOrWhiteSpace(_nicknameInput.text)
            ? "Player"
            : _nicknameInput.text.Trim();

        PlayerPrefs.SetString("Nickname", _playerNickname);
        PlayerPrefs.Save();
    }

    public string GetNickname()
    {
        return _playerNickname;
    }
}
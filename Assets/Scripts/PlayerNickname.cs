using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Fusion;
using System.Collections;

public class PlayerNickname : MonoBehaviour
{
    public static PlayerNickname Instance { get; private set; }

    [Header("UI References")]

    [SerializeField] private TMP_InputField _nicknameInput;
    [SerializeField] private Button _confirmButton;
    [SerializeField] private TMP_Text _playerUpdateStatusText;

    private string _playerNickname = "Player";
    private Coroutine _clearStatusCoroutine;

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
        ShowStatus("Update Successful!", true, true);
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
        Debug.Log($"Current Player Nickname: {_playerNickname}");
        return string.IsNullOrEmpty(_playerNickname) ? "Player" : _playerNickname;
    }

    public void ShowStatus(string message, bool autoClear, bool Successful)
    {
        _playerUpdateStatusText.text = message;

        if (Successful)
        {
            _playerUpdateStatusText.color = Color.green;
        }
        else
        {
            _playerUpdateStatusText.color = Color.red;
        }

        if (autoClear)
        {
            if (_clearStatusCoroutine != null)
                StopCoroutine(_clearStatusCoroutine);
            _clearStatusCoroutine = StartCoroutine(ClearStatusAfterDelay(3f));
        }
    }

    private IEnumerator ClearStatusAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        _playerUpdateStatusText.text = "";
        _clearStatusCoroutine = null;
    }
}
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement; // Ensure you have TextMeshPro installed and imported

public class GameSettings : MonoBehaviour
{
    [Header("Settings UI Elements")]
    [SerializeField] private GameObject settingsPanel;  // Reference to the settings panel
    [SerializeField] private Slider musicSlider;  // Reference to the music volume slider
    [SerializeField] private Slider sfxSlider;    // Reference to the SFX volume slider
    [SerializeField] private Toggle fullscreenToggle;  // Reference to the fullscreen toggle
    [SerializeField] private TMP_Dropdown resolutionDropdown;  // Reference to the resolution dropdown
    [SerializeField] private Slider lookSensitivitySlider;  // Reference to the look sensitivity slider
    [SerializeField] private TMP_Text lookSensitivityText;  // Reference to the text displaying look sensitivity value
    [SerializeField] private Button toggleSettingsButton;  // Button to toggle settings panel visibility

    [Header("Button")]
    [SerializeField] private Button activeButton;  // Button to activate the settings panel

    // Keys for PlayerPrefs
    public static string MusicVolumeKey = "MusicVolume";
    public static string SFXVolumeKey = "SFXVolume";
    public static string FullscreenKey = "Fullscreen";
    public static string ResolutionIndexKey = "ResolutionIndex";
    public static string LookSensitivityKey = "LookSensitivity";

    public static GameSettings Instance { get; private set; }

    private static bool isFullscreen;
    private static int resolutionsSelected = 0; // Global variable to store the selected resolution index
    // List of supported resolutions
    private Resolution[] resolutions;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        settingsPanel.SetActive(false);

        SceneManager.sceneLoaded += OnSceneLoaded; // Subscribe to scene loaded event
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == GameConfig.GAME_SCENE || scene.name == GameConfig.MAIN_MENU_SCENE)
        {
            if (activeButton != null)
            {
                activeButton.gameObject.SetActive(true);
            }
        }
        else
        {
            activeButton.gameObject.SetActive(false);
        }
    }

    private void Start()
    {
        // Ensure AudioController is initialized
        if (AudioController.Instance == null)
        {
            Debug.LogError("AudioController not found in scene!");
            return;
        }

        // Load saved volume settings or use defaults
        float savedMusicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, AudioController.Instance.musicVolume);
        float savedSFXVolume = PlayerPrefs.GetFloat(SFXVolumeKey, AudioController.Instance.sfxVolume);


        // Set slider values to match loaded volumes
        musicSlider.value = savedMusicVolume;
        sfxSlider.value = savedSFXVolume;

        // Add listeners to update volumes and save when sliders change
        musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);

        // Add listener to toggle settings panel visibility
        toggleSettingsButton.onClick.AddListener(ToggleSettingsPanel);
        if (activeButton != null)
            activeButton.onClick.AddListener(ToggleSettingsPanel);

        InitializeResolutionDropdown();
        // Initialize fullscreen toggle
        isFullscreen = PlayerPrefs.GetInt(FullscreenKey, Screen.fullScreen ? 1 : 0) == 1;
        fullscreenToggle.isOn = isFullscreen;
        fullscreenToggle.onValueChanged.AddListener(OnFullscreenToggled);
        ApplyFullscreen(isFullscreen);

        // Initialize resolution dropdown
        int savedResolutionIndex = PlayerPrefs.GetInt(ResolutionIndexKey, 0);
        resolutionsSelected = Mathf.Clamp(savedResolutionIndex, 0, resolutions.Length - 1);
        resolutionDropdown.value = resolutionsSelected;
        resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
        ApplyResolution(resolutionsSelected);

        // Initialize look sensitivity slider and text (only if not null)
        if (lookSensitivitySlider != null)
        {
            float savedLookSensitivity = PlayerPrefs.GetFloat(LookSensitivityKey, 0.1f); // Default sensitivity of 0.1
            GameConfig.LOOK_SENSITIVITY = savedLookSensitivity;
            lookSensitivitySlider.value = savedLookSensitivity;
            UpdateLookSensitivityText(savedLookSensitivity);
            lookSensitivitySlider.onValueChanged.AddListener(OnLookSensitivityChanged);
        }
    }

    private void OnMusicVolumeChanged(float value)
    {
        AudioController.Instance.SetMusicVolume(value);
        PlayerPrefs.SetFloat(MusicVolumeKey, value); // Save to PlayerPrefs
        PlayerPrefs.Save(); // Ensure data is written to disk
    }

    private void OnSFXVolumeChanged(float value)
    {
        AudioController.Instance.SetSFXVolume(value);
        PlayerPrefs.SetFloat(SFXVolumeKey, value); // Save to PlayerPrefs
        PlayerPrefs.Save(); // Ensure data is written to disk
    }

    #region Resolution Management
    private void InitializeResolutionDropdown()
    {
        // Get all supported resolutions
        Resolution[] allResolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        // Create a list of resolution options (width x height) with 16:9 aspect ratio
        System.Collections.Generic.List<Resolution> filteredResolutions = new System.Collections.Generic.List<Resolution>();
        const float targetAspectRatio = 16f / 9f; // 16:9 aspect ratio
        const float tolerance = 0.01f; // Tolerance for floating-point comparison

        for (int i = 0; i < allResolutions.Length; i++)
        {
            Resolution res = allResolutions[i];
            float aspectRatio = (float)res.width / res.height;

            // Check if the aspect ratio is approximately 16:9
            if (Mathf.Abs(aspectRatio - targetAspectRatio) < tolerance)
            {
                filteredResolutions.Add(res);
            }
        }

        // Assign the filtered resolutions to the class variable
        resolutions = filteredResolutions.ToArray();

        // Clear and populate dropdown with filtered resolutions
        resolutionDropdown.ClearOptions();
        System.Collections.Generic.List<string> options = new System.Collections.Generic.List<string>();
        for (int i = 0; i < resolutions.Length; i++)
        {
            Resolution res = resolutions[i];
            string option = $"{res.width}x{res.height}" + "@" + Mathf.RoundToInt((float)res.refreshRateRatio.value) + "Hz";
            options.Add(option);
        }

        // Add options to dropdown
        resolutionDropdown.AddOptions(options);
    }

    private void OnFullscreenToggled(bool Fullscreen)
    {
        isFullscreen = Fullscreen;
        ApplyFullscreen(isFullscreen);
        PlayerPrefs.SetInt(FullscreenKey, isFullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void OnResolutionChanged(int index)
    {
        resolutionsSelected = index;
        ApplyResolution(resolutionsSelected);
        PlayerPrefs.SetInt(ResolutionIndexKey, resolutionsSelected);
        PlayerPrefs.Save();
    }

    private void ApplyFullscreen(bool isFullscreen)
    {
        if (resolutionsSelected >= 0 && resolutionsSelected < resolutions.Length)
        {
            Resolution res;
            if (isFullscreen)
            {
                res = resolutions[resolutions.Length - 1];
            }
            else
            {
                res = resolutions[resolutionsSelected];
            }
            Screen.SetResolution(res.width, res.height, isFullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed, res.refreshRateRatio);
        }
        else
        {
            Resolution defaultRes = Screen.currentResolution;
            Screen.SetResolution(defaultRes.width, defaultRes.height, isFullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed, defaultRes.refreshRateRatio);
        }
    }

    private void ApplyResolution(int index)
    {
        if (index >= 0 && index < resolutions.Length && !isFullscreen)
        {
            Resolution res = resolutions[index];
            Screen.SetResolution(res.width, res.height, isFullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed, res.refreshRateRatio);
            Debug.Log($"Resolution set to: {res.width} x {res.height}");
        }
    }
    #endregion

    #region Look Sensitivity
    private void OnLookSensitivityChanged(float value)
    {
        if (lookSensitivitySlider != null && lookSensitivityText != null)
        {
            UpdateLookSensitivityText(value);
            PlayerPrefs.SetFloat(LookSensitivityKey, value); // Save to PlayerPrefs
            PlayerPrefs.Save(); // Ensure data is written to disk

            ApplyLookSensitivity(value);
        }
    }

    private void UpdateLookSensitivityText(float value)
    {
        if (lookSensitivityText != null)
        {
            lookSensitivityText.text = value.ToString("F2"); // Display with 2 decimal places
        }
    }

    private void ApplyLookSensitivity(float sensitivity)
    {
        // Placeholder: Replace with your camera control logic
        // Example: If you have a CameraController script, call a method like:
        // CameraController.Instance.SetLookSensitivity(sensitivity);
        GameConfig.LOOK_SENSITIVITY = sensitivity; // Update global variable
        Debug.Log($"Look Sensitivity set to: {sensitivity}");
    }
    #endregion

    #region OnPanelToggle
    public void ToggleSettingsPanel()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(!settingsPanel.activeSelf);
        }
    }
    public bool ToggleSettingsPanelBool()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(!settingsPanel.activeSelf);
            return settingsPanel.activeSelf;
        }
        return false;
    }
    #endregion

    #region Cleanup
    private void OnDestroy()
    {
        // Clean up listeners to prevent memory leaks
        musicSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
        sfxSlider.onValueChanged.RemoveListener(OnSFXVolumeChanged);
        fullscreenToggle.onValueChanged.RemoveListener(OnFullscreenToggled);
        resolutionDropdown.onValueChanged.RemoveListener(OnResolutionChanged);
    }
    #endregion
}
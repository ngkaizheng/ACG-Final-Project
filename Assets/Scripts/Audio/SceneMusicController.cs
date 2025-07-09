using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneMusicController : MonoBehaviour
{
    private void Awake()
    {
        // Subscribe to scene loaded event
        SceneManager.sceneLoaded += OnSceneLoaded;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        AudioController.Instance.FadeOutMusic(1f);
        // if (scene.name == GameConfig.LOGIN_SCENE)
        // {
        //     AudioController.Instance.FadeInMusic(MusicTrack.LoginBGM, 1f);
        // }
        // else 
        if (scene.name == GameConfig.MAIN_MENU_SCENE)
        {
            AudioController.Instance.FadeInMusic(MusicTrack.MainMenuBGM, 1f);
        }
        else if (scene.name == GameConfig.GAME_SCENE)
        {
            AudioController.Instance.FadeInMusic(MusicTrack.GameBGM, 1f);
        }
    }

}
using UnityEngine;
using UnityEngine.SceneManagement;

public class BlockerController : MonoBehaviour
{
    public static BlockerController Instance { get; private set; }

    [Header("Blocker UI")]
    [SerializeField] private GameObject holder;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Hide();
    }

    public void Show()
    {
        if (holder != null)
            holder.SetActive(true);
    }

    public void Hide()
    {
        if (holder != null)
            holder.SetActive(false);
    }
}
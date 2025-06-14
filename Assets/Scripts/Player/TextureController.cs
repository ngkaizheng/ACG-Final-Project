using UnityEngine;

public class TextureController : MonoBehaviour
{
    public static TextureController Instance { get; private set; }
    public Texture[] TextureList;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
}
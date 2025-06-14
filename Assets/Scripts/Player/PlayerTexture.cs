using UnityEngine;
using Fusion;

public class PlayerTexture : NetworkBehaviour
{
    [SerializeField] private Renderer targetRenderer;
    [Networked, OnChangedRender(nameof(OnTextureChanged))]
    private int TextureIndex { get; set; }

    public override void Spawned()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponentInChildren<Renderer>();
        ApplyTexture(TextureIndex);
    }

    public void NextTexture()
    {
        if (!Object.HasInputAuthority) return;
        int nextIndex = (TextureIndex + 1) % TextureController.Instance.TextureList.Length;
        RPC_ChangeTexture(nextIndex);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_ChangeTexture(int index, RpcInfo info = default)
    {
        TextureIndex = index;
    }

    private void OnTextureChanged()
    {
        ApplyTexture(TextureIndex);
    }

    private void ApplyTexture(int index)
    {
        if (TextureController.Instance == null || targetRenderer == null) return;
        targetRenderer.material.mainTexture = TextureController.Instance.TextureList[index];
    }
}
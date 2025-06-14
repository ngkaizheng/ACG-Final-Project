using UnityEngine;
using Fusion;


public enum InputButton
{
    Jump,
    LeftClick,
    RightClick,
    Escape,
    Reload,
    E,
}

public struct NetInput : INetworkInput
{
    public NetworkButtons Buttons;
    public Vector2 Direction;
    public Vector2 LookDelta;
    public Vector3 LookDirection;
}

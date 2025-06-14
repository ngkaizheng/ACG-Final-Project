using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;

public class InputManager : SimulationBehaviour, IBeforeUpdate, INetworkRunnerCallbacks
{
    private NetInput accumulatedInput;
    private bool isInputDisabled;
    private Player _localPlayer;

    public void SetLocalPlayer(Player player)
    {
        _localPlayer = player;
    }

    void IBeforeUpdate.BeforeUpdate()
    {

        if (isInputDisabled)
        {
            accumulatedInput = default;
            return;
        }

        if (!Runner.IsRunning || _localPlayer == null)
        {
            accumulatedInput = default;
            return; // Ensure we only process input when the runner is running and local player is set
        }

        Vector2 mousePos = Mouse.current.position.ReadValue();
        if (mousePos.x >= 0 && mousePos.x <= Screen.width &&
            mousePos.y >= 0 && mousePos.y <= Screen.height)
        {
            // // Get mouse position and convert to world direction
            // Vector3 mouseScreenPos = Mouse.current.position.ReadValue();
            // mouseScreenPos.z = Mathf.Abs(Camera.main.transform.position.y - _localPlayer.transform.position.y);
            Vector3 mouseScreenPos = new Vector3(mousePos.x, mousePos.y, Mathf.Abs(Camera.main.transform.position.y - _localPlayer.transform.position.y));
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
            Vector3 lookDirection = (mouseWorldPos - _localPlayer.transform.position);

            // Store normalized XZ direction in input
            accumulatedInput.LookDirection = new Vector2(lookDirection.x, lookDirection.z);
        }
        else
        {
            // Optionally, zero out look direction if mouse is outside
            accumulatedInput.LookDirection = Vector2.zero;
        }

        Keyboard keyboard = Keyboard.current;
        Mouse mouse = Mouse.current;
        if (mouse != null)
        {
            Vector2 mouseDelta = mouse.delta.ReadValue();
            Vector2 lookRotationDelta = new(-mouseDelta.y, mouseDelta.x);

            accumulatedInput.LookDelta += lookRotationDelta;
        }

        NetworkButtons buttons = default;

        if (keyboard != null)
        {
            Vector2 moveDirection = Vector2.zero;

            if (keyboard.wKey.isPressed == true) { moveDirection += Vector2.up; }
            if (keyboard.sKey.isPressed == true) { moveDirection += Vector2.down; }
            if (keyboard.aKey.isPressed == true) { moveDirection += Vector2.left; }
            if (keyboard.dKey.isPressed == true) { moveDirection += Vector2.right; }

            //Normalized Called in OnInput
            accumulatedInput.Direction = moveDirection;

            buttons.Set(InputButton.Jump, keyboard.spaceKey.isPressed);
            buttons.Set(InputButton.LeftClick, mouse.leftButton.isPressed);
            buttons.Set(InputButton.RightClick, mouse.rightButton.isPressed);
            buttons.Set(InputButton.Escape, keyboard.escapeKey.isPressed);
            buttons.Set(InputButton.Reload, keyboard.rKey.isPressed);
            buttons.Set(InputButton.E, keyboard.eKey.isPressed);
        }

        // accumulatedInput.Buttons = new NetworkButtons(accumulatedInput.Buttons.Bits | buttons.Bits);
        accumulatedInput.Buttons = buttons; // Direct assignment instead of accumulation
    }

    void INetworkRunnerCallbacks.OnConnectedToServer(Fusion.NetworkRunner runner)
    {

    }


    void INetworkRunnerCallbacks.OnConnectFailed(Fusion.NetworkRunner runner, Fusion.Sockets.NetAddress remoteAddress, Fusion.Sockets.NetConnectFailedReason reason)
    {

    }

    void INetworkRunnerCallbacks.OnConnectRequest(Fusion.NetworkRunner runner, Fusion.NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {

    }

    void INetworkRunnerCallbacks.OnCustomAuthenticationResponse(Fusion.NetworkRunner runner, System.Collections.Generic.Dictionary<string, object> data)
    {

    }

    void INetworkRunnerCallbacks.OnDisconnectedFromServer(Fusion.NetworkRunner runner, Fusion.Sockets.NetDisconnectReason reason)
    {

    }
    void INetworkRunnerCallbacks.OnHostMigration(Fusion.NetworkRunner runner, Fusion.HostMigrationToken hostMigrationToken)
    {

    }

    void INetworkRunnerCallbacks.OnInput(NetworkRunner runner, NetworkInput input)
    {
        //Check current player
        Debug.Log($"Input received for {runner.LocalPlayer}");

        if (runner.IsPlayer)
        {
            Debug.Log($"Input for player {runner.LocalPlayer}: Direction={accumulatedInput.Direction}, LookDelta={accumulatedInput.LookDelta}, Buttons={accumulatedInput.Buttons}");
            input.Set(accumulatedInput);
            accumulatedInput.LookDelta = default; // Reset look delta immediately
        }
    }

    void INetworkRunnerCallbacks.OnInputMissing(Fusion.NetworkRunner runner, Fusion.PlayerRef player, Fusion.NetworkInput input)
    {

    }

    void INetworkRunnerCallbacks.OnObjectEnterAOI(Fusion.NetworkRunner runner, Fusion.NetworkObject obj, Fusion.PlayerRef player)
    {

    }
    void INetworkRunnerCallbacks.OnObjectExitAOI(Fusion.NetworkRunner runner, Fusion.NetworkObject obj, Fusion.PlayerRef player)
    {

    }

    void INetworkRunnerCallbacks.OnPlayerJoined(Fusion.NetworkRunner runner, Fusion.PlayerRef player)
    {
    }

    void INetworkRunnerCallbacks.OnPlayerLeft(Fusion.NetworkRunner runner, Fusion.PlayerRef player)
    {

    }

    void INetworkRunnerCallbacks.OnReliableDataProgress(Fusion.NetworkRunner runner, Fusion.PlayerRef player, Fusion.Sockets.ReliableKey key, float progress)
    {

    }
    void INetworkRunnerCallbacks.OnReliableDataReceived(Fusion.NetworkRunner runner, Fusion.PlayerRef player, Fusion.Sockets.ReliableKey key, System.ArraySegment<byte> data) { }
    void INetworkRunnerCallbacks.OnSceneLoadStart(NetworkRunner runner)
    {
        // isInputDisabled = true;
        // accumulatedInput = default;
    }

    void INetworkRunnerCallbacks.OnSceneLoadDone(NetworkRunner runner)
    {
        // StartCoroutine(EnableInputAfterDelay());
        // Cursor.lockState = CursorLockMode.Locked;
        // Cursor.visible = false;
        // Debug.Log("Scene load done, cursor locked, starting input enable delay.");
    }
    private IEnumerator EnableInputAfterDelay()
    {
        yield return new WaitForSeconds(0.5f);
        isInputDisabled = false;
        Debug.Log("Input enabled after 0.5-second delay.");
    }

    void INetworkRunnerCallbacks.OnSessionListUpdated(Fusion.NetworkRunner runner, System.Collections.Generic.List<Fusion.SessionInfo> sessionList) { }
    void INetworkRunnerCallbacks.OnShutdown(Fusion.NetworkRunner runner, Fusion.ShutdownReason shutdownReason)
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    void INetworkRunnerCallbacks.OnUserSimulationMessage(Fusion.NetworkRunner runner, Fusion.SimulationMessagePtr message) { }
}

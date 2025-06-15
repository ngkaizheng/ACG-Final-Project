using System;
using System.Diagnostics;
using Fusion;
using UnityEngine;

public class PlayerInputController : NetworkBehaviour
{
    private Player _player;
    private PlayerTexture _playerTexture;
    [Networked] private NetworkButtons PreviousButtons { get; set; }

    public void Initialize(Player player)
    {
        _player = player;
    }

    private void Awake()
    {
        _playerTexture = GetComponent<PlayerTexture>();
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetInput input))
        {
            if (!_player.isAlive()) return;

            // Debug.Log($"Processing input for player {Object.InputAuthority} at Tick: {Runner.Tick}, IsForward: {Runner.IsForward}, IsSimulation: {Runner.IsResimulation}");
            ProcessButtons(input, PreviousButtons);
            PreviousButtons = input.Buttons;
        }
    }

    public override void Render()
    {
        // if (Object.IsProxy)
        // {
        //     ProcessButtons(Runner.GetInput<NetInput>(), PreviousButtons);
        // }
    }

    private void ProcessButtons(NetInput input, NetworkButtons previousButtons)
    {
        if (input.Buttons.IsSet(InputButton.LeftClick))
        {
            _player.LeftClick();
        }

        if (input.Buttons.IsSet(InputButton.RightClick))
        {
            _player.RightClick();
        }

        if (input.Buttons.WasPressed(previousButtons, InputButton.Reload))
        {
            _player.Reload();
        }

        if (input.Buttons.WasPressed(previousButtons, InputButton.E))
        {
            _playerTexture.NextTexture();
        }
    }
}
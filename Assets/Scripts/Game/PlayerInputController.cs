using Fusion;

public class PlayerInputController : NetworkBehaviour
{
    private Player _player;
    [Networked] private NetworkButtons PreviousButtons { get; set; }

    public void Initialize(Player player)
    {
        _player = player;
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetInput input))
        {
            ProcessButtons(input, PreviousButtons);
            PreviousButtons = input.Buttons;
        }
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
    }
}
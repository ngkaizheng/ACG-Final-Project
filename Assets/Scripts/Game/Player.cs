using Fusion;
using UnityEngine;

public class Player : NetworkBehaviour
{
    private NetworkCharacterController _cc;

    private void Awake()
    {
        _cc = GetComponent<NetworkCharacterController>();
    }

    public override void Spawned()
    {
        if (Object.InputAuthority == Runner.LocalPlayer)
        {
            // This is the local player, set up the camera or other local player-specific logic here
            Debug.Log("Local player spawned");
            // Example: Set up a camera for the local player
            // Camera.main.transform.SetParent(transform);
            // Camera.main.transform.localPosition = new Vector3(0, 1.5f, -2);
            // Camera.main.transform.LookAt(transform);

            CameraController.Instance.SetFollowTarget(transform);
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetInput data))
        {
            Debug.Log($"Player {Object.InputAuthority} input received: Direction={data.Direction}, LookDelta={data.LookDelta}, Buttons={data.Buttons}");

            Vector2 move2D = data.Direction.normalized;
            Vector3 move = new Vector3(move2D.x, 0, move2D.y); // XZ plane

            _cc.Move(5 * move * Runner.DeltaTime);

            if (data.Buttons.IsSet(InputButton.Jump))
            {
                _cc.Jump();
            }
            // transform.Rotate(0, data.LookDelta.x * Runner.DeltaTime, 0);
        }
    }
}
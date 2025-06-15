using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : NetworkBehaviour
{
    [Header("Player Settings")]
    [SerializeField] private float _rotationSpeed = 10f;
    [SerializeField] private float _moveSpeed = 10f;

    [Networked] public PlayerHealth Health { get; private set; }

    private NetworkCharacterController _cc;
    private CharacterController _characterController;
    private PlayerInputController _inputController;

    private Gun _gun;

    private void Awake()
    {
        _cc = GetComponent<NetworkCharacterController>();
        _characterController = GetComponent<CharacterController>();
        _inputController = GetComponent<PlayerInputController>();

        _gun = GetComponentInChildren<Gun>();

        _inputController.Initialize(this);
    }

    public override void Spawned()
    {
        Health = GetComponent<PlayerHealth>();

        if (Object.InputAuthority == Runner.LocalPlayer)
        {
            CameraController.Instance.SetFollowTarget(transform);

            InputManager inputManager = FindObjectOfType<InputManager>();
            if (inputManager != null)
            {
                inputManager.SetLocalPlayer(this);
                Debug.Log("Local player set in InputManager");
            }
            else
            {
                Debug.LogWarning("InputManager not found in the scene.");
            }
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetInput input))
        {
            if (!isAlive()) return;

            UpdateMoveDirection(input);
            UpdateRotation(input);
            // if (data.Buttons.IsSet(InputButton.Jump))
            // {
            //     _cc.Jump();
            // }
        }
    }

    private void UpdateMoveDirection(NetInput input)
    {
        Vector2 move2D = input.Direction.normalized;
        Vector3 move = new Vector3(move2D.x, 0, move2D.y); // XZ plane
        _cc.Move(move);
        // _characterController.Move(move * _moveSpeed * Runner.DeltaTime);
        // Vector3 move = new Vector3(input.Direction.x, 0, input.Direction.y).normalized * Runner.DeltaTime * 5f;
        // transform.position += move;
    }

    private void UpdateRotation(NetInput input)
    {
        Vector3 lookDirection = new Vector3(
            input.LookDirection.x,
            0,
            input.LookDirection.y
        ).normalized;
        if (lookDirection.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            Quaternion smoothedRotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                _rotationSpeed * Runner.DeltaTime
            );
            transform.rotation = smoothedRotation;
        }

        if (Object.HasStateAuthority)
        {
            Debug.DrawLine(
                transform.position,
                transform.position + lookDirection * 2f,
                Color.red,
                1f
            );
        }
        else
        {
            Debug.DrawLine(
                transform.position,
                transform.position + lookDirection * 2f,
                Color.blue,
                1f
            );
        }
    }

    #region Player Actions
    public void LeftClick()
    {
        Debug.Log("Left Click Action Triggered");
        // Implement left click action logic here
        _gun.Shoot();
    }
    public void RightClick()
    {
        Debug.Log("Right Click Action Triggered");
        // Implement right click action logic here
        // Deduct 10 HP from the player
        if (Health != null && Health.IsAlive)
        {
            Health.TakeDamage(10, Object.InputAuthority);
        }

    }
    public void Reload()
    {
        Debug.Log("Reload Action Triggered");
        // Implement reload action logic here
    }
    #endregion

    #region Event Handlers
    private void HandleDeath(PlayerRef killer)
    {
        Debug.Log($"{Object.InputAuthority} has died, killed by {killer}");
        // Handle player death logic here, such as respawning or updating UI
    }
    #endregion

    public bool isAlive()
    {
        return Health != null && Health.IsAlive;
    }
}
// //Can you implement simple player movement in Unity using the new input system, I already attacah NetworkCharacterController to the player prefab, and I have a PlayerMovement script that handles the movement logic. Here's a basic implementation:
// using UnityEngine;
// using UnityEngine.InputSystem;
// using Fusion;

// public class PlayerMovement : NetworkBehaviour
// {
//     [SerializeField] private float moveSpeed = 5f;
//     [SerializeField] private float jumpForce = 5f;

//     private NetworkCharacterController _characterController;
//     private Vector2 _moveInput;
//     private bool _jumpInput;

//     public override void Spawned()
//     {
//         base.Spawned();
//         _characterController = GetComponent<NetworkCharacterController>();
//     }

//     public void OnMove(InputAction.CallbackContext context)
//     {
//         if (context.performed)
//         {
//             _moveInput = context.ReadValue<Vector2>();
//         }
//         else if (context.canceled)
//         {
//             _moveInput = Vector2.zero;
//         }
//     }

//     public void OnJump(InputAction.CallbackContext context)
//     {
//         if (context.performed)
//         {
//             _jumpInput = true;
//         }
//         else if (context.canceled)
//         {
//             _jumpInput = false;
//         }
//     }

//     public override void FixedUpdateNetwork()
//     {
//         if (!Object.HasStateAuthority) return;

//         Vector3 moveDirection = new Vector3(_moveInput.x, 0, _moveInput.y);
//         moveDirection *= moveSpeed * Runner.DeltaTime;

//         if (_jumpInput && _characterController.IsGrounded)
//         {
//             moveDirection.y = jumpForce;
//             _jumpInput = false; // Reset jump input after applying jump force
//         }

//         _characterController.Move(moveDirection);
//     }
// }
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCharacterInputController : MonoBehaviour
{
    [SerializeField]
    private PlayerCharacter _playerCharacter;

    private InputAction _jumpAction;

    private void Awake()
    {
        _jumpAction = InputSystem.actions.FindAction("Jump");
    }

    private void OnEnable()
    {
        _jumpAction.performed += OnJumpPerformed;
    }

    private void OnDisable()
    {
        _jumpAction.performed -= OnJumpPerformed;
    }

    private void OnJumpPerformed(InputAction.CallbackContext ctx)
    {
        _playerCharacter.Jump();
    }
}
using UnityEngine;
using UnityEngine.InputSystem;

public class TitleUIController : MonoBehaviour
{
    private InputAction _clickAction;

    private void Awake()
    {
        _clickAction = InputSystem.actions.FindAction("Click");
    }

    private void OnEnable()
    {
        _clickAction.performed += OnScreenTouched;
    }

    private void OnDisable()
    {
        _clickAction.performed -= OnScreenTouched;
    }

    private void OnScreenTouched(InputAction.CallbackContext ctx)
    {
        GameManager.Instance.MoveToStage();
        this.enabled = false;
    }
}
using UnityEngine;

public class ScriptableActionTrigger : MonoBehaviour
{
    [SerializeField]
    private bool _triggerOnlyOnce;
    [SerializeField]
    private ScriptableActionBase[] _actions;

    private bool _hasTriggered;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_triggerOnlyOnce && _hasTriggered)
            return;
        _hasTriggered = true;

        foreach (var action in _actions)
        {
            action.SetTarget(other.gameObject);
            action.Invoke();
        }
    }
}
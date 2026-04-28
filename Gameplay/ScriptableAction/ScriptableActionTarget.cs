using UnityEngine;

public class ScriptableActionTarget : MonoBehaviour
{
    [SerializeField]
    private ScriptableActionBase[] _actions;

    private void OnEnable()
    {
        SetTarget(gameObject);
    }

    private void OnDisable()
    {
        SetTarget(null);
    }

    private void SetTarget(GameObject target)
    {
        foreach (var action in _actions)
        {
            action.SetTarget(target);
        }
    }
}
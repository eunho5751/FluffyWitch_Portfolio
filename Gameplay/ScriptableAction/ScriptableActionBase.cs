using UnityEngine;

public abstract class ScriptableActionBase : ScriptableObject
{
    public void Invoke()
    {
        if (Target == null)
        {
            Debug.LogError("Target not found!");
            return;
        }

        OnInvoke();
    }

    public void SetTarget(GameObject target)
    {
        var prevTarget = Target;
        Target = target;
        if (target != null)
        {
            if (prevTarget != target)
                OnTargetSet(target);
        }
    }

    protected virtual void OnTargetSet(GameObject target) { }
    protected abstract void OnInvoke();

    public GameObject Target { get; private set; }
}
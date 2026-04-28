using UnityEngine;
using Sirenix.OdinInspector;

[HideMonoScript]
public abstract class MovePatternBase : ScriptableObject
{
    protected virtual void OnConstruct(CharacterBase character) { }
 
    public virtual void ResetState(CharacterBase character) { }
    public abstract Vector2 Evaluate(MoveContext ctx);

    public MovePatternBase CreateOrGetInstance(CharacterBase character)
    {
        var instance = IsStateful ? Instantiate(this) : this;
        instance.OnConstruct(character);
        return instance;
    }

    protected abstract bool IsStateful { get; }
}
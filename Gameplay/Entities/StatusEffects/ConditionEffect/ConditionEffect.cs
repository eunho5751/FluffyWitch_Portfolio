
public class ConditionEffect : StatusEffectBase<ConditionEffectData>
{
    public ConditionEffect(ConditionEffectData data) : base(data)
    {
    }

    public override void OnApply()
    {
        Target.Conditions.Add(Data.Condition);
    }

    public override void OnRemove()
    {
        Target.Conditions.Remove(Data.Condition);
    }
}
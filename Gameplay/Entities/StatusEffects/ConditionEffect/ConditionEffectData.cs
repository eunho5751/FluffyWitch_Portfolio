using UnityEngine;

[CreateAssetMenu(menuName = "FluffyWitch/StatusEffects/Condition")]
public class ConditionEffectData : StatusEffectDataBase
{
    [SerializeField]
    private CharacterCondition _condition;

    public override StatusEffectBase CreateInstance() => new ConditionEffect(this);

    public CharacterCondition Condition => _condition;
}
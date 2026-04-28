using UnityEngine;

[CreateAssetMenu(menuName = "FluffyWitch/MovePatterns/Straight")]
public class StraightPattern : MovePatternBase
{
    public override Vector2 Evaluate(MoveContext ctx) => Vector2.left * ctx.Speed;
    
    protected override bool IsStateful => false;
}
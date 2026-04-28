using UnityEngine;

[CreateAssetMenu(menuName = "FluffyWitch/MovePatterns/Fix")]
public class FixPattern : MovePatternBase
{
    public override Vector2 Evaluate(MoveContext ctx) => Vector2.zero;
    
    protected override bool IsStateful => false;
}
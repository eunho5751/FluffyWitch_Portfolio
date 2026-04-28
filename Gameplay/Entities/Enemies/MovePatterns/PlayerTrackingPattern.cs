using UnityEngine;

[CreateAssetMenu(menuName = "FluffyWitch/MovePatterns/PlayerTracking")]
public class PlayerTrackingPattern : MovePatternBase
{
    [SerializeField]
    private float _keepDistance = 0.5f;

    public override Vector2 Evaluate(MoveContext ctx)
    {
        Vector2 offset = StageManager.Instance.PlayerCharacter.PhysicsPosition - ctx.Character.PhysicsPosition;
        Vector2 dir = Mathf.Abs(offset.x) <= _keepDistance ? new Vector2(0f, offset.y) : offset;
        return dir.normalized * ctx.Speed;
    }

    protected override bool IsStateful => false;
}
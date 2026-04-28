using UnityEngine;

[CreateAssetMenu(menuName = "FluffyWitch/MovePatterns/UpDown")]
public class UpDownPattern : MovePatternBase
{
    [SerializeField] 
    private bool _startMovingUp = true;

    private bool _movingUp;

    public override void ResetState(CharacterBase character)
    {
        _movingUp = _startMovingUp;
    }

    public override Vector2 Evaluate(MoveContext ctx)
    {
        var bounds = ctx.Character.Bounds;
        var checkOffset = Vector2.up * (ctx.Speed * Time.deltaTime);
        var checkMin = (Vector2)bounds.min + (_movingUp ? checkOffset : -checkOffset);
        var checkMax = (Vector2)bounds.max + (_movingUp ? checkOffset : -checkOffset);
        if (StageManager.Instance.IsOverStageBounds(checkMin, checkMax))
        {
            _movingUp = !_movingUp;
        }
        
        return (_movingUp ? Vector2.up : Vector2.down) * ctx.Speed;
    }

    protected override bool IsStateful => true;
}
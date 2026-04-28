
public class Boss : Enemy
{
    protected override void OnSpawn()
    {
        base.OnSpawn();

        
    }

    public override CharacterType Type => CharacterType.Boss;
}
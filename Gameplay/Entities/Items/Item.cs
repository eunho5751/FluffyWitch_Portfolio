using UnityEngine;

public class Item : EntityBase
{
    private EntityPool _pool;

    public void Construct(ItemData data, EntityPool pool)
    {
        Data = data;
        _pool = pool;
    }

    protected override void OnDespawn()
    {
        _pool.ReleaseItem(this);
    }

    protected override void OnMove(ref Vector2 velocity)
    {
        velocity = Vector2.left * Data.Common.Speed;
    }

    protected override void OnCollision(EntityBase entity)
    {
        if (entity is PlayerCharacter playerCharacter)
        {
            Data.Effect.Apply(playerCharacter);
            Despawn();
        }
    }

    public ItemData Data { get; private set; }
}
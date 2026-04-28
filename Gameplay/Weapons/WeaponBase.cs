using System.Threading;
using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    private CancellationTokenSource _unequipCTS;

    public void Construct(CharacterBase owner, WeaponDataBase weaponData)
    {
        Owner = owner;
        WeaponData = weaponData;
        
        var scale = transform.localScale;
        scale.x *= Direction.x;
        transform.localScale = scale;
        
        OnConsturct();
    }

    public void Equip()
    {
        if (IsEquipped)
            return;
        
        _unequipCTS = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);
        UnequipToken = _unequipCTS.Token;
        OnEquip();
        IsEquipped = true;
    }

    public void Unequip()
    {
        if (!IsEquipped)
            return;

        if (_unequipCTS != null)
        {
            _unequipCTS.Cancel();
            _unequipCTS.Dispose();
            _unequipCTS = null;
        }
        OnUnequip();
        IsEquipped = false;
    }

    public float CalculateCooldown(float baseCooldown)
    {
        float finalCooldown = baseCooldown;
        float attackSpeed = Owner.GetStat(StatType.AttackSpeed).Value;
        attackSpeed = Mathf.Approximately(attackSpeed, 0f) ? 1f : attackSpeed;
        finalCooldown /= attackSpeed;
        return finalCooldown;
    }

    protected virtual void OnConsturct() { }
    protected virtual void OnDestroy() { }
    protected virtual void OnEquip() { }
    protected virtual void OnUnequip() { }

    public CancellationToken UnequipToken { get; private set; }

    public bool IsEquipped { get; private set; }
    public CharacterBase Owner { get; private set; }
    public WeaponDataBase WeaponData { get; private set; }
    public Vector2 Direction => Owner.IsPlayerCharacter ? Vector2.right : Vector2.left;
    public EntityLayerFlags OpponentLayer => Owner.IsPlayerCharacter ? EntityLayerFlags.Enemy : EntityLayerFlags.PlayerCharacter;
}
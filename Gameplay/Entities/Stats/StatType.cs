
public enum StatType
{
    // Character Common
    MaxHp               = 1,
    AttackDamage        = 2,
    AttackSpeed         = 3,

    // PlayerCharacter Only
    HpRegen             = 101,
    CriticalChance      = 102,
    CriticalHit         = 103,
    Armor               = 104,
    DamageMultiplier    = 105,
    DamageReduction     = 106,
    EvadeChance         = 107,
    LifestealChance     = 108,
    GravityScale        = 109,
    HitboxScale         = 110,
    ItemMagnetRadius    = 111,
    ItemDropChance      = 112,
    ExpGainMultiplier   = 113,
    MpGainMultiplier    = 114,

    // Enemy Only
    CollisionDamage     = 201,
    MoveSpeed           = 202,
}
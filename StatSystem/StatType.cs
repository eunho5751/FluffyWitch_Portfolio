
public enum StatType
{
    // Character Common
    MaxHp                       = 100,
    HpRegen                     = 101,
    AttackPower                 = 110,
    AttackSpeed                 = 120,

    // PlayerCharacter Only
    Armor                       = 1000,
    DamageIncrease              = 1010,
    DamageReduction             = 1011,
    CriticalChance              = 1020,
    CriticalHit                 = 1021,
    EvadeChance                 = 1030,
    GravityIncrease             = 1050,
    HitboxReduction             = 1060,
    ItemMagnetRangeIncrease     = 1070,
    ItemDropChanceIncrease      = 1071,
    ExpGainIncrease             = 1080,
    MpGainIncrease              = 1090,

    // Enemy Only
    MoveSpeed                   = 2000,
    CollisionDamage             = 2010,
}
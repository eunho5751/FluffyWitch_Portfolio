using System;

[Flags]
public enum DamageFlags
{
    None                = 0,
    TrueDamage          = 1 << 0,
    IgnoreInvincible    = 1 << 1,
}
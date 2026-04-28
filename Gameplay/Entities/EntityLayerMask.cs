using System;
using UnityEngine;

[Flags]
public enum EntityLayerFlags
{
    None                = 0,
    PlayerCharacter     = 1 << 0,
    Hurdle              = 1 << 1,
    Enemy               = 1 << 2,
    Projectile          = 1 << 3,
}

public static class EntityLayerMask
{
    private static readonly (EntityLayerFlags flag, string layerName)[] FlagToLayer =
    {
        (EntityLayerFlags.PlayerCharacter, "PlayerCharacter"),
        (EntityLayerFlags.Hurdle,          "Hurdle"),
        (EntityLayerFlags.Enemy,           "Enemy"),
        (EntityLayerFlags.Projectile,      "Projectile"),
    };

    public static LayerMask Get(EntityLayerFlags flags)
    {
        int mask = 0;
        for (int i = 0; i < FlagToLayer.Length; i++)
        {
            if (flags.HasFlag(FlagToLayer[i].flag))
                mask |= 1 << LayerMask.NameToLayer(FlagToLayer[i].layerName);
        }
        return mask;
    }
}
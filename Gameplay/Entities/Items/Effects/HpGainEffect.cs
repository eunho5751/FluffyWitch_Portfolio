using System;
using UnityEngine;

[Serializable]
public class HpGainEffect : IItemEffect
{
    [SerializeField, Range(0f, 1f)]
    private float _gainPercent = 0.3f;

    public void Apply(PlayerCharacter playerCharacter)
    {
        float maxHp = playerCharacter.GetStat(StatType.MaxHp).Value;
        float gainAmount = maxHp * _gainPercent;
        playerCharacter.CurrentHp += gainAmount;
    }
}
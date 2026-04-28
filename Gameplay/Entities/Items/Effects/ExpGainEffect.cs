using System;
using UnityEngine;

[Serializable]
public class ExpGainEffect : IItemEffect
{
    [SerializeField]
    private int _baseGainAmount = 100;
    [SerializeField]
    private int _gainAmountPerStage = 100;

    public void Apply(PlayerCharacter playerCharacter)
    {
        int gainAmount = _baseGainAmount + StageManager.Instance.CurrentStage * _gainAmountPerStage;
        playerCharacter.AddExp(gainAmount);
    }
}
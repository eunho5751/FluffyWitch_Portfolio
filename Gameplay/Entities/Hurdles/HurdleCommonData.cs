using UnityEngine;
using Sirenix.OdinInspector;

[HideMonoScript]
[CreateAssetMenu(menuName = "FluffyWitch/Hurdles/Common", fileName = "HurdleCommonData")]
public class HurdleCommonData : ScriptableObject
{
    [TitleGroup("Attributes")]
    [SerializeField, Range(0f, 1f)]
    private float _damagePercent = 0.9f;
    [SerializeField, MinValue(0f)]
    private float _bonusDamage;
    [SerializeField, MinValue(1)]
    private int _powerUpStageInterval = 1;

    [Space(5)]

    [TitleGroup("Rewards")]
    [SerializeField, MinValue(0)]
    private int _rewardExp;
    [SerializeField, MinValue(0f)]
    private float _rewardExpBonusPerStage;

    public float DamagePercent => _damagePercent;
    public float BonusDamage => _bonusDamage;
    public float PowerUpStageInterval => _powerUpStageInterval;

    public int RewardExp => _rewardExp;
    public float RewardExpBonusPerStage => _rewardExpBonusPerStage;
}
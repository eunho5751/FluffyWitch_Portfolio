using UnityEngine;

public class HurdlePassRewardHandler
{
    public HurdlePassRewardHandler()
    {
        Hurdle.Passed += OnHurdlePassed;
    }

    public void Dispose()
    {
        Hurdle.Passed -= OnHurdlePassed;
    }

    private void OnHurdlePassed(Hurdle hurdle)
    {
        int rewardExp = CalculateRewardExp(hurdle, StageManager.Instance.CurrentStage);
        var playerCharacter = StageManager.Instance.PlayerCharacter;
        playerCharacter.AddExp(rewardExp);
    }

    private int CalculateRewardExp(Hurdle hurdle, int stage)
    {
        var hurdleData = hurdle.Data.Common;
        return hurdleData.RewardExp + Mathf.FloorToInt((stage - 1) * hurdleData.RewardExpBonusPerStage);
    }
}
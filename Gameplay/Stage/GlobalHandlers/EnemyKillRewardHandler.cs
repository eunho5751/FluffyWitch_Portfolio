using UnityEngine;

public class EnemyKillRewardHandler
{
    public EnemyKillRewardHandler()
    {
        CombatSystem.Instance.Killed += OnCharacterKilled;
    }

    public void Dispose()
    {
        CombatSystem.Instance.Killed -= OnCharacterKilled;
    }

    private void OnCharacterKilled(CharacterBase attacker, CharacterBase victim)
    {
        if (attacker is not PlayerCharacter playerCharacter || victim is not Enemy enemy)
        {
            return;
        }
        
        // Exp
        int rewardExp = CalculateRewardExp(enemy, StageManager.Instance.CurrentStage);
        playerCharacter.AddExp(rewardExp);

        // Drop
        var dropTable = StageManager.Instance.StageConfig.DropTable;
        float totalWeight = dropTable.MissWeight;
        foreach (var entry in dropTable)
            totalWeight += entry.Weight;

        float roll = Random.value * totalWeight;
        float cumulative = 0f;
        foreach (var entry in dropTable)
        {
            cumulative += entry.Weight;
            if (roll < cumulative)
            {
                var itemData = GameManager.Instance.Database.GetItemData(entry.Guid);
                var item = StageManager.Instance.Entities.GetItem(itemData);
                item.Spawn(victim.Position);
                break;
            }
        }
    }

    private int CalculateRewardExp(Enemy enemy, int stage)
    {
        var enemyData = enemy.Data;
        return enemyData.RewardExp + Mathf.FloorToInt((stage - 1) * enemyData.RewardExpBonusPerStage);
    }
}
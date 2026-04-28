using System.Collections.Generic;
using UnityEngine;

public static class EntityHelper
{
    public static CharacterBase FindClosestOpponent(bool isPlayerCharacter, Vector2 position, float minRange, float maxRange, HashSet<EntityBase> excludeSet = null)
    {
        if (!isPlayerCharacter)
        {
            var player = StageManager.Instance.PlayerCharacter;
            if (excludeSet != null && excludeSet.Contains(player))
                return null;
            return player;
        }
        return FindClosestEnemy(position, minRange, maxRange, excludeSet);
    }

    public static Enemy FindClosestEnemy(Vector2 position, float minRange, float maxRange, HashSet<EntityBase> excludeSet = null)
    {
        var enemies = StageManager.Instance.Entities.Enemies;
        if (enemies.Count == 0)
            return null;

        Enemy closest = null;
        float minDistSq = minRange * minRange;
        float closestDistSq = maxRange * maxRange;
        foreach (var enemy in enemies)
        {
            if (excludeSet != null && excludeSet.Contains(enemy))
                continue;

            float distSq = Vector2.SqrMagnitude(enemy.Position - position);
            if (distSq >= minDistSq && distSq < closestDistSq)
            {
                closestDistSq = distSq;
                closest = enemy;
            }
        }

        return closest;
    }

    public static CharacterBase FindFrontOpponent(bool isPlayerCharacter, Vector2 position, float minRange, float maxRange, HashSet<EntityBase> excludeSet = null)
    {
        if (!isPlayerCharacter)
        {
            var player = StageManager.Instance.PlayerCharacter;
            if (excludeSet != null && excludeSet.Contains(player))
                return null;

            Vector2 offset = player.Position - position;
            if (offset.x > 0f)
                return null;

            float distSq = offset.sqrMagnitude;
            float minDistSq = minRange * minRange;
            float maxDistSq = maxRange * maxRange;
            if (distSq >= minDistSq && distSq < maxDistSq)
                return player;

            return null;
        }
        return FindFrontEnemy(position, minRange, maxRange, excludeSet);
    }

    public static Enemy FindFrontEnemy(Vector2 position, float minRange, float maxRange, HashSet<EntityBase> excludeSet = null)
    {
        var enemies = StageManager.Instance.Entities.Enemies;
        if (enemies.Count == 0)
            return null;

        Enemy front = null;
        float minDistSq = minRange * minRange;
        float closestDistSq = maxRange * maxRange;
        foreach (var enemy in enemies)
        {
            if (excludeSet != null && excludeSet.Contains(enemy))
                continue;

            Vector2 offset = enemy.Position - position;
            if (offset.x < 0f)
                continue;

            float distSq = offset.sqrMagnitude;
            if (distSq >= minDistSq && distSq < closestDistSq)
            {
                closestDistSq = distSq;
                front = enemy;
            }
        }

        return front;
    }
}

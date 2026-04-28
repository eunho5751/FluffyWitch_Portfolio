using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class EntityPool : MonoBehaviour
{
    [SerializeField, Required]
    private Transform _enemiesParent;
    [SerializeField, Required]
    private Transform _hurdlesParent;
    [SerializeField, Required]
    private Transform _projectilesParent;
    [SerializeField, Required]
    private Transform _itemsParent;

    private readonly List<Enemy> _enemies = new();
    private readonly List<Hurdle> _hurdles = new();
    private readonly List<ProjectileBase> _projectiles = new();
    private readonly List<Item> _items = new();

    public Enemy GetEnemy(EnemyData enemyData)
    {
        var enemy = Get(_enemies, enemyData.Prefab.gameObject, _enemiesParent);
        enemy.Construct(enemyData, this);
        return enemy;
    }

    public void ReleaseEnemy(Enemy enemy)
    {
        Release(_enemies, enemy);
    }

    public Hurdle GetHurdle(HurdleData hurdleData)
    {
        var hurdle = Get(_hurdles, hurdleData.Prefab.gameObject, _hurdlesParent);
        hurdle.Construct(hurdleData, this);
        return hurdle;
    }

    public void ReleaseHurdle(Hurdle hurdle)
    {
        Release(_hurdles, hurdle);
    }

    public T GetProjectile<T>(T projectilePrefab) where T : ProjectileBase
    {
        var projectile = Get(_projectiles, projectilePrefab.gameObject, _projectilesParent);
        projectile.Construct(this);
        return projectile as T;
    }

    public void ReleaseProjectile(ProjectileBase projectile)
    {
        Release(_projectiles, projectile);
    }

    public Item GetItem(ItemData itemData)
    {
        var item = Get(_items, itemData.Prefab.gameObject, _itemsParent);
        item.Construct(itemData, this);
        return item;
    }

    public void ReleaseItem(Item item)
    {
        Release(_items, item);
    }

    private T Get<T>(List<T> entities, GameObject prefab, Transform parent) where T : MonoBehaviour
    {
        var entity = PoolManager.Instance.Get<T>(prefab);
        entity.transform.parent = parent;
        entities.Add(entity);
        return entity;
    }

    private void Release<T>(List<T> entities, T entity) where T : MonoBehaviour
    {
        var go = entity.gameObject;
        entities.Remove(entity);
        PoolManager.Instance.Release(go);
    }

    public IReadOnlyList<Enemy> Enemies => _enemies;
    public IReadOnlyList<Hurdle> Hurdles => _hurdles;
    public IReadOnlyList<ProjectileBase> Projectiles => _projectiles;
}
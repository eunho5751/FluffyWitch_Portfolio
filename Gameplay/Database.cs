using Cysharp.Threading.Tasks;

public class Database
{
    private static class Keys
    {
        public static readonly string Enemy = "Enemy";
        public static readonly string Hurdle = "Hurdle";
        public static readonly string Weapon = "Weapon";
        public static readonly string Item = "Item";
    }

    private readonly GuidRepository<EnemyData> _enemyRepo = new();
    private readonly GuidRepository<HurdleData> _hurdleRepo = new();
    private readonly GuidRepository<WeaponDataBase> _weaponRepo = new();
    private readonly GuidRepository<ItemData> _itemRepo = new();

    public async UniTask LoadAsync()
    {
        await _enemyRepo.LoadAsync(Keys.Enemy);
        await _hurdleRepo.LoadAsync(Keys.Hurdle);
        await _weaponRepo.LoadAsync(Keys.Weapon);
        await _itemRepo.LoadAsync(Keys.Item);
    }

    public void Unload()
    {
        _enemyRepo.Unload();
        _hurdleRepo.Unload();
        _weaponRepo.Unload();
        _itemRepo.Unload();
    }

    public bool TryGetEnemyData(string guid, out EnemyData enemyData) => _enemyRepo.TryGet(guid, out enemyData);
    public EnemyData GetEnemyData(string guid) => _enemyRepo.Get(guid);

    public bool TryGetHurdleData(string guid, out HurdleData hurdleData) => _hurdleRepo.TryGet(guid, out hurdleData);
    public HurdleData GetHurdleData(string guid) => _hurdleRepo.Get(guid);

    public bool TryGetWeaponData(string guid, out WeaponDataBase weaponData) => _weaponRepo.TryGet(guid, out weaponData);
    public WeaponDataBase GetWeaponData(string guid) => _weaponRepo.Get(guid);

    public bool TryGetItemData(string guid, out ItemData itemData) => _itemRepo.TryGet(guid, out itemData);
    public ItemData GetItemData(string guid) => _itemRepo.Get(guid);
}
using UnityEngine;
using Cysharp.Threading.Tasks;

public class VFXManager : SingletonBehaviourBase<VFXManager>
{
    private static readonly string Key = "VFX";

    private readonly GuidRepository<VFXData> _repository = new();

    public UniTask LoadAsync()
    {
        return _repository.LoadAsync(Key);
    }

    public void Unload()
    {
        _repository.Unload();
    }

    public VFXData GetData(string guid)
    {
        return _repository.Get(guid);
    }

    public GameObject Spawn(string guid, Vector2 position)
    {
        var data = GetData(guid);
        return Spawn(data, position);
    }

    public GameObject Spawn(VFXData data, Vector2 position)
    {
        if (data == null)
        {
            Debug.LogError("VFXData is null!");
            return null;
        }

        var instance = PoolManager.Instance.Get(data.Prefab);
        instance.transform.position = position;
        instance.transform.localScale = Vector3.one * data.Scale;
        instance.SetActive(true);
        return instance;
    }

    public void Despawn(GameObject instance)
    {
        if (instance == null)
            return;
        instance.SetActive(false);
        PoolManager.Instance.Release(instance);
    }
}

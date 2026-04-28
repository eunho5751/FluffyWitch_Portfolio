using System;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Cysharp.Threading.Tasks;

public class GuidRepository<T> where T : GuidScriptableObjectBase
{
    private readonly Dictionary<string, T> _resourceMap = new();
    private AsyncOperationHandle<IList<T>> _handle;

    public async UniTask LoadAsync(string key)
    {
        if (_handle.IsValid())
            throw new InvalidOperationException($"Repository has already been loaded.");

        try
        {
            _handle = Addressables.LoadAssetsAsync<T>(key);
            var resources = await _handle;
            foreach (var resource in resources)
            {
                AddResource(resource);
            }
        }
        catch
        {
            Unload();
            throw;
        }
    }

    public void Unload()
    {
        if (!_handle.IsValid())
            return;

        Addressables.Release(_handle);
        _handle = default;
        _resourceMap.Clear();
    }

    public T Get(string id)
    {
        return _resourceMap[id];
    }

    public bool TryGet(string id, out T resource)
    {
        return _resourceMap.TryGetValue(id, out resource);
    }

    private void AddResource(T resource)
    {
        if (resource == null)
            throw new ArgumentNullException(nameof(resource));
        _resourceMap.Add(resource.Guid, resource);
    }
}
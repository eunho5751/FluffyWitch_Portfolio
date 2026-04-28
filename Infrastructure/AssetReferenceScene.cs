using System;
using UnityEngine.AddressableAssets;

#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public class AssetReferenceScene : AssetReference
{
    public AssetReferenceScene(string guid) : base(guid) { }

    public override bool ValidateAsset(string path)
    {
#if UNITY_EDITOR
        var type = AssetDatabase.GetMainAssetTypeAtPath(path);
        return type == typeof(SceneAsset);
#else
        return false;
#endif
    }
}

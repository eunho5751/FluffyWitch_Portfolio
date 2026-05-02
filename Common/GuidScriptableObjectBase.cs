using UnityEngine;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor;
#endif

[HideMonoScript]
public abstract class GuidScriptableObjectBase : SerializedScriptableObject
{
    [SerializeField, ReadOnly]
    private string _guid;

#if UNITY_EDITOR
    protected virtual void OnValidate()
    {
        string path = AssetDatabase.GetAssetPath(this);
        if (string.IsNullOrEmpty(path))
            return;

        string assetGuid = AssetDatabase.AssetPathToGUID(path);
        if (string.IsNullOrEmpty(assetGuid))
            return;

        if (string.IsNullOrEmpty(_guid) || _guid != assetGuid)
        {
            _guid = assetGuid;
            EditorUtility.SetDirty(this);
        }
    }
#endif

    public string Guid => _guid;
}
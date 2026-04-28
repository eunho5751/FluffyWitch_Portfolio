using UnityEngine;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "FluffyWitch/Items/Item")]
public class ItemData : GuidScriptableObjectBase
{
    [SerializeField, DisableIf("@_commonData != null")]
    private ItemCommonData _commonData;

    [TitleGroup("Base")]
    [SerializeField, Required, AssetSelector]
    private Item _prefab;
    [SerializeReference, Required]
    private IItemEffect _effect;

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        
        if (_commonData == null)
        {
            string dataPath = $"Assets/_Project/Data/Entities/{nameof(ItemCommonData)}.asset";
            _commonData = AssetDatabase.LoadAssetAtPath<ItemCommonData>(dataPath);

            if (_commonData == null)
                Debug.LogError($"Can't find {nameof(ItemCommonData)}.asset in {System.IO.Path.GetDirectoryName(dataPath)}.");
        }
    }
#endif

    public ItemCommonData Common => _commonData;

    public Item Prefab => _prefab;
    public IItemEffect Effect => _effect;
}
using UnityEngine;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "FluffyWitch/Hurdles/Hurdle")]
public class HurdleData : GuidScriptableObjectBase
{
    [SerializeField, DisableIf("@_commonData != null")]
    private HurdleCommonData _commonData;

    [TitleGroup("Base")]
    [SerializeField, Required]
    private Hurdle _prefab;

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        
        if (_commonData == null)
        {
            string dataPath = $"Assets/_Project/Data/Entities/{nameof(HurdleCommonData)}.asset";
            _commonData = AssetDatabase.LoadAssetAtPath<HurdleCommonData>(dataPath);

            if (_commonData == null)
                Debug.LogError($"Can't find {nameof(HurdleCommonData)}.asset in {System.IO.Path.GetDirectoryName(dataPath)}.");
        }
    }
#endif

    public HurdleCommonData Common => _commonData;

    public Hurdle Prefab => _prefab;
}
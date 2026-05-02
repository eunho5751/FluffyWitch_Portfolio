using System;
using UnityEngine;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using System.Collections.Generic;
using System.Reflection;
using Sirenix.OdinInspector.Editor;
#endif

[Serializable, InlineProperty]
public class GuidRef<T> where T : GuidScriptableObjectBase
{
    [SerializeField, HideInInspector]
    private string _guid;

    public static implicit operator string(GuidRef<T> guidRef)
    {
        return guidRef.Guid;
    }

#if UNITY_EDITOR
    [OnValueChanged(nameof(OnDataChanged))]
    [SerializeField, HideLabel]
    private T _data;

    private void OnDataChanged(T data) => _guid = data != null ? data.Guid : null;
#endif

    public string Guid => _guid;
    public bool IsValid => !string.IsNullOrEmpty(_guid);
}

#if UNITY_EDITOR
public class GuidRefAttributeProcessor<T> : OdinAttributeProcessor<GuidRef<T>> where T : GuidScriptableObjectBase
{
    public override void ProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
    {
        if (member.Name == "_data" && parentProperty.GetAttribute<RequiredAttribute>() != null)
        {
            attributes.Add(new RequiredAttribute($"{parentProperty.NiceName} is required"));
        }
    }
}
#endif

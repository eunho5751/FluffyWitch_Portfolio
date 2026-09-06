using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = nameof(ScreenTransitionSettings), menuName = "Screen Transition/Screen Transition Settings")]
public class ScreenTransitionSettings : SerializedScriptableObject
{
    [SerializeField, DisableContextMenu]
    private Dictionary<string, ScreenTransitionProfile> _profileMap = new();

    public ScreenTransitionProfile GetProfile(string key) => _profileMap[key];
    public bool TryGetProfile(string key, out ScreenTransitionProfile profile) => _profileMap.TryGetValue(key, out profile);
}
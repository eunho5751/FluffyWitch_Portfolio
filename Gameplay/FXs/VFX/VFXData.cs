using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(menuName = "FluffyWitch/FXs/VFX")]
public class VFXData : GuidScriptableObjectBase
{
    [SerializeField, Required]
    private GameObject _prefab;
    [SerializeField, MinValue(0f)]
    private float _scale = 1f;
    
    public GameObject Prefab => _prefab;
    public float Scale => _scale;
}
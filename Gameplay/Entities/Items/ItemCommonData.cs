using UnityEngine;
using Sirenix.OdinInspector;

[HideMonoScript]
[CreateAssetMenu(menuName = "FluffyWitch/Items/Common", fileName = "ItemCommonData")]
public class ItemCommonData : ScriptableObject
{
    [TitleGroup("Attributes")]
    [SerializeField, MinValue(0f)]
    private float _speed = 4f;

    public float Speed => _speed;
}
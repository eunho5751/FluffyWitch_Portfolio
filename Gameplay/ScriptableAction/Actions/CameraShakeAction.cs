using UnityEngine;

[CreateAssetMenu(menuName = "FluffyWitch/ScriptableActions/CameraShake")]
public class CameraShakeAction : ScriptableActionBase
{
    [SerializeField]
    private float _duration;
    [SerializeField]
    private float _magnitude;

    private CameraShaker _cameraShaker;

    protected override void OnTargetSet(GameObject target)
    {
        if (!target.TryGetComponent(out _cameraShaker))
        {
            Debug.LogError($"'{nameof(CameraShaker)}' component not found!");
        }
    }

    protected override void OnInvoke()
    {
        if (_cameraShaker != null)
            _cameraShaker.Shake(_duration, _magnitude);
    }
}
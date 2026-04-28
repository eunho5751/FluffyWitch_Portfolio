
public class HitFeedbackHandler
{
    private readonly CameraShakeAction _cameraShake;
    private readonly PlayerCharacter _playerCharacter;

    public HitFeedbackHandler(CameraShakeAction cameraShake)
    {
        _cameraShake = cameraShake;
        _playerCharacter = StageManager.Instance.PlayerCharacter;
        _playerCharacter.HitTaken += OnHitTaken;
    }

    public void Dispose()
    {
        _playerCharacter.HitTaken -= OnHitTaken;
    }

    private void OnHitTaken(HitResult result)
    {
        if (result.IsHit && result.DamageKind == DamageKind.Impact)
        {
            _cameraShake.Invoke();
        }
    }
}
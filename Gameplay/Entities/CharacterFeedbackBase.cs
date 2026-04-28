using System;
using System.Threading;
using UnityEngine;
using Sirenix.OdinInspector;
using Cysharp.Threading.Tasks;

public abstract class CharacterFeedbackBase : MonoBehaviour
{
    [SerializeField, Required]
    private CharacterBase _character;
    [SerializeField, Required]
    private SpriteRenderer _renderer;

    [Space(10)]

    [SerializeField]
    private FXClip _damagedFX;

    private StatusEffectFXHandler _statusEffectFXHandler;
    private CancellationTokenSource _hitFlashCTS;

    private void Start()
    {
        _statusEffectFXHandler = new(_character.StatusEffects);
        _character.StatusEffects.Applied += HandleStatusEffectApplied;
        _character.StatusEffects.Removed += HandleStatusEffectRemoved;
        _character.HitTaken += HandleHitTaken;
        _character.Died += OnDied;
        OnInitialize();
    }

    private void OnDestroy()
    {
        _statusEffectFXHandler.Dispose();
        _character.StatusEffects.Applied -= HandleStatusEffectApplied;
        _character.StatusEffects.Removed -= HandleStatusEffectRemoved;
        _character.HitTaken -= HandleHitTaken;
        _character.Died -= OnDied;
        OnCleanup();
    }

    protected virtual void OnInitialize() { }
    protected virtual void OnCleanup() { }

    protected virtual void OnStatusEffectApplied(StatusEffectBase statusEffect) { }
    protected virtual void OnStatusEffectRemoved(StatusEffectBase statusEffect) { }
    protected virtual void OnHitTaken(HitResult result) { }
    protected virtual void OnDied() { }

    private void HandleStatusEffectApplied(StatusEffectBase statusEffect)
    {
        if (statusEffect.Data == StatusEffectRegistry.HitInvincibility)
        {
            SetInvincibleBlinkEnabled(true);
        }
    }

    private void HandleStatusEffectRemoved(StatusEffectBase statusEffect)
    {
        if (statusEffect.Data == StatusEffectRegistry.HitInvincibility)
        {
            SetInvincibleBlinkEnabled(false);
        }
    }

    private void HandleHitTaken(HitResult result)
    {
        if (result.IsHit)
        {
            if (_hitFlashCTS != null)
            {
                _hitFlashCTS.Cancel();
                _hitFlashCTS.Dispose();
            }
            _hitFlashCTS = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);
            PlayHitFlash(_hitFlashCTS.Token).Forget();

            _damagedFX.Play(Character.Position);
        }

        OnHitTaken(result);
    }

    private async UniTaskVoid PlayHitFlash(CancellationToken token)
    {
        try
        {
            float interval = Renderer.material.GetFloat("_FlashInterval");
            Renderer.material.SetFloat("_FlashStartTime", Time.time);
            Renderer.material.SetFloat("_FlashEnabled", 1);
            await UniTask.WaitForSeconds(interval, cancellationToken: token);
        }
        finally
        {
            if (Renderer != null)
                Renderer.material.SetFloat("_FlashEnabled", 0);
        }
    }

    private void SetInvincibleBlinkEnabled(bool enabled)
    {
        if (enabled)
            Renderer.material.SetFloat("_BlinkStartTime", Time.time);
        Renderer.material.SetFloat("_BlinkEnabled", enabled ? 1 : 0);
    }

    public CharacterBase Character => _character;
    public SpriteRenderer Renderer => _renderer;
}
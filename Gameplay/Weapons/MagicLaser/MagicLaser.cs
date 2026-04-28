using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;

public class MagicLaser : MonoBehaviour
{

    [SerializeField, Required]
    private Transform _beamStart;
    [SerializeField, Required]
    private Transform _beamCenter;
    [SerializeField, Required]
    private Transform _beamEnd;

    private static readonly Collider2D[] HitBuffer = new Collider2D[32];

    private MagicLaserWeapon _weapon;
    private CancellationTokenSource _hitCheckCTS;

    private Animator[] _beamAnims;

    private SpriteRenderer _startRenderer;
    private SpriteRenderer _centerRenderer;
    private SpriteRenderer _endRenderer;
    private Vector2 _initialCenterSize;
    private float _startLeftOffset;
    private float _endRightOffset;
    private float _initialCenterPivot;
    private float _initialEndPivot;
    private float _startOffset;

    private ContactFilter2D _hitFilter;
    private readonly Dictionary<CharacterBase, float> _hitTimers = new();

    public void Construct(MagicLaserWeapon weapon)
    {
        _weapon = weapon;

        _beamAnims = GetComponentsInChildren<Animator>(true);
        _startRenderer = _beamStart.GetComponent<SpriteRenderer>();
        _centerRenderer = _beamCenter.GetComponent<SpriteRenderer>();
        _endRenderer = _beamEnd.GetComponent<SpriteRenderer>();

        _initialCenterSize = _centerRenderer.sprite.bounds.size;
        _startLeftOffset = _startRenderer.sprite.bounds.min.x;
        _endRightOffset = _endRenderer.sprite.bounds.max.x;
        _initialCenterPivot = _initialCenterSize.x * 0.5f;
        _initialEndPivot = _beamEnd.localPosition.x - _beamCenter.localPosition.x - _initialCenterPivot;
        _startOffset = _beamCenter.localPosition.x - _initialCenterPivot;

        _startRenderer.sprite = null;
        _centerRenderer.sprite = null;
        _endRenderer.sprite = null;
    }

    public void StartLaunch()
    {
        Launch(_weapon.UnequipToken).Forget();
    }

    public void StopLaunch()
    {
        StopFiring();
    }

    private async UniTaskVoid Launch(CancellationToken token)
    {
        var weaponData = _weapon.WeaponData;
        while (!token.IsCancellationRequested)
        {
            Vector2 spawnPosition = _weapon.Owner.Position + _weapon.Direction * weaponData.AttackOffset;
            transform.position = spawnPosition;

            StartFiring(token);
            await UniTask.WaitForSeconds(weaponData.Duration, cancellationToken: token);
            StopFiring();

            float cooldown = _weapon.CalculateCooldown(weaponData.AttackCoolodwn);
            await UniTask.WaitForSeconds(cooldown, cancellationToken: token);
        }
    }

    private void StartFiring(CancellationToken token)
    {
        var weaponData = _weapon.WeaponData;
        SetBeamActive(true);
        SetBeamScale(weaponData.BeamScale);

        _hitFilter = new()
        {
            useTriggers = true,
            useLayerMask = true,
            layerMask = EntityLayerMask.Get(_weapon.OpponentLayer)
        };
        _hitCheckCTS = CancellationTokenSource.CreateLinkedTokenSource(token);
        CheckHit(weaponData, _hitCheckCTS.Token).Forget();
    }

    private void StopFiring()
    {
        _hitTimers.Clear();
        CancelAndDispose(ref _hitCheckCTS);
        SetBeamActive(false);
    }

    private async UniTaskVoid CheckHit(MagicLaserWeaponData weaponData, CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            float left = _beamStart.position.x + _startLeftOffset * _beamStart.lossyScale.x;
            float right = _beamEnd.position.x + _endRightOffset * _beamEnd.lossyScale.x;
            float height = _initialCenterSize.y * _beamCenter.lossyScale.y;
            Vector2 center = new((left + right) * 0.5f, _beamCenter.position.y);
            Vector2 size = new(Mathf.Abs(right - left), height);
            int hitCount = Physics2D.OverlapBox(center, size, 0f, _hitFilter, HitBuffer);
            for (int i = 0; i < hitCount; i++)
            {
                if (HitBuffer[i].TryGetComponent(out CharacterBase target))
                {
                    if (_hitTimers.TryGetValue(target, out float lastHit)
                        && Time.time - lastHit < weaponData.HitInterval)
                        continue;

                    float damage = weaponData.Damage;
                    HitContext ctx = HitContext.Impact(damage, DamageFlags.None, _weapon.Owner, target);
                    if (CombatSystem.Instance.TryApplyHit(ctx, out var result))
                    {
                        if (result.IsResolved)
                        {
                            _hitTimers[target] = Time.time;
                        }
                    }
                }
            }
            
            await UniTask.Yield(PlayerLoopTiming.FixedUpdate, token);
        }
    }

    private void SetBeamScale(Vector2 scale)
    {
        var startScale = _beamStart.localScale;
        startScale.y = scale.y;
        _beamStart.localScale = startScale;
        
        var centerScale = _beamCenter.localScale;
        centerScale.x = scale.x;
        centerScale.y = scale.y;
        _beamCenter.localScale = centerScale;

        Vector3 centerPosition = _beamCenter.localPosition;
        centerPosition.x = _startOffset + _initialCenterPivot * centerScale.x;
        _beamCenter.localPosition = centerPosition;

        var endScale = _beamEnd.localScale;
        endScale.y = scale.y;
        _beamEnd.localScale = endScale;

        Vector3 endPosition = _beamEnd.localPosition;
        endPosition.x = _startOffset + _initialCenterSize.x * centerScale.x + _initialEndPivot;
        _beamEnd.localPosition = endPosition;
    }

    private void SetBeamActive(bool active)
    {
        foreach (var anim in _beamAnims)
        {
            anim.SetBool("IsActive", active);
        }
    }

    private static void CancelAndDispose(ref CancellationTokenSource cts)
    {
        if (cts == null) 
            return;
        cts.Cancel();
        cts.Dispose();
        cts = null;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (_beamStart == null || _beamCenter == null || _beamEnd == null) return;
        if (_startLeftOffset == 0f && _endRightOffset == 0f) return;

        float left = _beamStart.position.x + _startLeftOffset * _beamStart.lossyScale.x;
        float right = _beamEnd.position.x + _endRightOffset * _beamEnd.lossyScale.x;
        float height = _initialCenterSize.y * _beamCenter.lossyScale.y;

        Vector2 center = new((left + right) * 0.5f, _beamCenter.position.y);
        Vector2 size = new(right - left, height);

        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Gizmos.DrawCube(center, size);
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(center, size);
    }
#endif
}

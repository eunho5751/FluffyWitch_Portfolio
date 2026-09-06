using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using Sirenix.OdinInspector;
using Cysharp.Threading.Tasks;

public class ScreenTransitioner : SingletonBehaviourBase<ScreenTransitioner>
{
    [SerializeField, Required]
    private ScreenTransitionSettings _settings;
    [SerializeField, Required]
    private CanvasScaler _canvasScaler;

    private ScreenTransitionEffectBase _lastTransition;

    protected override void OnAwake()
    {
        Assert.IsNotNull(_settings);
        Assert.IsNotNull(_canvasScaler);
    }

    public async UniTask StartTransitionInAsync(string key) => await StartTransitionAsync(key, false);
    public async UniTask StartTransitionOutAsync(string key) => await StartTransitionAsync(key, true);

    private async UniTask StartTransitionAsync(string key, bool transitionOut)
    {
        if (key == null)
            throw new ArgumentNullException(nameof(key));

        if (CheckTransitionIsInProgress())
            return;
        if (_lastTransition != null)
            Destroy(_lastTransition.gameObject);

        var info = _settings.GetProfile(key);
        _canvasScaler.referenceResolution = info.ReferenceResolution;

        var transition = Instantiate(info.EffectPrefab, _canvasScaler.transform);
        transition.SetSettings(info.GetEffectSettings<object>());
        _lastTransition = transition;

        IsInTransition = true;
        if (transitionOut)
        {
            await transition.StartTransitionOutAsync();
        }
        else
        {
            await transition.StartTransitionInAsync();
            Destroy(transition.gameObject);
            _lastTransition = null;
        }
        IsInTransition = false;
    }

    private bool CheckTransitionIsInProgress()
    {
        if (IsInTransition)
        {
            Debug.LogError("Transition is not allowed while another is in progress.");
            return true;
        }
        return false;
    }

    public ScreenTransitionSettings Settings => _settings;
    public bool IsInTransition { get; private set; }
}